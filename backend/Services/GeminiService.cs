using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class GeminiService(HttpClient client, GeminiApiKeyProvider keyProvider, ILlmQuotaService quota,
    IOptions<GeminiOptions> options, TimeProvider clock, ILogger<GeminiService>? logger = null) : ILlmService
{
    public async Task<LlmJsonResult> GenerateJsonAsync(LlmJsonRequest request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var key = keyProvider.GetKey();
        if (!settings.Enabled || string.IsNullOrWhiteSpace(key)) return LlmJsonResult.Failure(AnalysisErrorCode.ConfigurationError);
        if (request.Text.Length > settings.MaxInputCharacters) return LlmJsonResult.Failure(AnalysisErrorCode.InputTooLong);
        var verified = false;
        var lastError = AnalysisErrorCode.ProviderUnavailable;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var reservation = await quota.ReserveAsync(cancellationToken);
            while (!reservation.Allowed && reservation.RetryAt is { } retryAt)
            {
                await DelayUntilAsync(retryAt, cancellationToken);
                reservation = await quota.ReserveAsync(cancellationToken);
            }
            if (!reservation.Allowed) return LlmJsonResult.Failure(AnalysisErrorCode.QuotaExceeded);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(settings.RequestTimeoutSeconds));
            TimeSpan? retryDelay = null;
            try
            {
                if (!verified)
                {
                    using var modelRequest = CreateRequest(HttpMethod.Get, $"models/{Uri.EscapeDataString(settings.Model)}", key);
                    using var model = await SendAsync(modelRequest, timeout.Token);
                    if (!model.RootElement.TryGetProperty("name", out var name) || name.GetString() != $"models/{settings.Model}" ||
                        !model.RootElement.TryGetProperty("supportedGenerationMethods", out var methods) ||
                        !methods.EnumerateArray().Any(method => method.GetString() == "generateContent"))
                        return LlmJsonResult.Failure(AnalysisErrorCode.ModelUnavailable);
                    verified = true;
                }
                using var message = CreateRequest(HttpMethod.Post, $"models/{Uri.EscapeDataString(settings.Model)}:generateContent", key);
                message.Content = JsonContent.Create(new
                {
                    systemInstruction = new { parts = new[] { new { text = request.Instruction } } },
                    contents = new[] { new { role = "user", parts = new[] { new { text = JsonSerializer.Serialize(
                        new Dictionary<string, string> { [request.TextPropertyName] = request.Text }) } } } },
                    generationConfig = new { responseMimeType = "application/json", responseJsonSchema = request.Schema, maxOutputTokens = settings.MaxOutputTokens }
                });
                using var response = await SendAsync(message, timeout.Token);
                var root = response.RootElement;
                var inputTokens = 0;
                var outputTokens = 0;
                if (root.TryGetProperty("usageMetadata", out var usage))
                {
                    inputTokens = ReadTokenCount(usage, "promptTokenCount");
                    outputTokens = ReadTokenCount(usage, "candidatesTokenCount") + ReadTokenCount(usage, "thoughtsTokenCount");
                }
                await quota.RecordTokensAsync(reservation.Date, inputTokens, outputTokens, cancellationToken);
                if (root.TryGetProperty("promptFeedback", out var feedback) && feedback.TryGetProperty("blockReason", out _))
                    return LlmJsonResult.Failure(AnalysisErrorCode.ContentBlocked);
                if (!root.TryGetProperty("candidates", out var candidates) || candidates.ValueKind != JsonValueKind.Array || candidates.GetArrayLength() != 1)
                    throw new GeminiCallException(AnalysisErrorCode.InvalidResponse, true, reason: "CandidateShape");
                var candidate = candidates[0];
                var finish = candidate.TryGetProperty("finishReason", out var reason) ? reason.GetString() : null;
                if (finish is "SAFETY" or "BLOCKLIST" or "PROHIBITED_CONTENT" or "RECITATION")
                    return LlmJsonResult.Failure(AnalysisErrorCode.ContentBlocked);
                if (finish != "STOP" || !candidate.TryGetProperty("content", out var content) || !content.TryGetProperty("parts", out var parts))
                    throw new GeminiCallException(AnalysisErrorCode.InvalidResponse, true,
                        reason: finish == "MAX_TOKENS" ? "OutputTokenLimit" : "CompletionState");
                var text = string.Concat(parts.EnumerateArray()
                    .Where(part => !(part.TryGetProperty("thought", out var thought) && thought.ValueKind == JsonValueKind.True))
                    .Select(part => part.TryGetProperty("text", out var value) ? value.GetString() : null));
                var validJson = request.Validate(text);
                if (validJson is null) throw new GeminiCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ValidationRejected");
                return LlmJsonResult.Success(validJson);
            }
            catch (GeminiCallException error)
            {
                lastError = error.Code;
                logger?.LogWarning("Gemini denemesi başarısız. Attempt: {Attempt}, Code: {Code}, Reason: {Reason}",
                    attempt + 1, error.Code, error.Reason);
                if (error.BlockUntil is { } until) await quota.BlockAsync(until, cancellationToken);
                if (!error.Retryable) return LlmJsonResult.Failure(error.Code);
                retryDelay = error.Delay;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = AnalysisErrorCode.ProviderUnavailable;
                logger?.LogWarning("Gemini denemesi zaman aşımına uğradı. Attempt: {Attempt}", attempt + 1);
            }
            catch (HttpRequestException error)
            {
                lastError = AnalysisErrorCode.ProviderUnavailable;
                logger?.LogWarning("Gemini bağlantısı kurulamadı. Attempt: {Attempt}, Category: {Category}", attempt + 1, error.HttpRequestError);
            }
            catch (JsonException)
            {
                lastError = AnalysisErrorCode.InvalidResponse;
                logger?.LogWarning("Gemini yanıtı geçerli JSON değil. Attempt: {Attempt}", attempt + 1);
            }
            catch (Exception error) when (error is InvalidOperationException or FormatException)
            {
                lastError = AnalysisErrorCode.InvalidResponse;
                logger?.LogWarning("Gemini yanıtının alan türleri geçersiz. Attempt: {Attempt}", attempt + 1);
            }
            if (attempt < 2)
            {
                var delay = retryDelay ?? TimeSpan.FromSeconds(settings.RetryBaseDelaySeconds * Math.Pow(2, attempt) + Random.Shared.NextDouble() * 0.25);
                if (delay.TotalSeconds > settings.MaxRetryDelaySeconds) return LlmJsonResult.Failure(lastError);
                await Task.Delay(delay, clock, cancellationToken);
            }
        }
        return LlmJsonResult.Failure(lastError);
    }

    private HttpRequestMessage CreateRequest(HttpMethod method, string path, string key)
    {
        var request = new HttpRequestMessage(method, new Uri(new Uri(options.Value.BaseUrl), path));
        request.Headers.Add("x-goog-api-key", key);
        request.Headers.Accept.ParseAdd("application/json");
        return request;
    }

    private async Task<JsonDocument> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        using var response = await client.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var buffer = new MemoryStream();
        var chunk = new byte[8192];
        int count;
        while ((count = await stream.ReadAsync(chunk, cancellationToken)) > 0)
        {
            if (buffer.Length + count > options.Value.MaxResponseBytes)
                throw new GeminiCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ResponseSizeLimit");
            buffer.Write(chunk, 0, count);
        }
        JsonDocument? body = null;
        try { body = JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 64 }); }
        catch (JsonException) when (!response.IsSuccessStatusCode) { }
        if (response.IsSuccessStatusCode) return body ?? throw new GeminiCallException(AnalysisErrorCode.InvalidResponse, true);
        using (body)
        {
            var status = response.StatusCode;
            logger?.LogWarning("Gemini HTTP hatası. Operation: {Operation}, StatusCode: {StatusCode}",
                request.Method == HttpMethod.Get ? "ModelCheck" : "GenerateContent", (int)status);
            var errorText = body?.RootElement.GetRawText() ?? string.Empty;
            if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden || errorText.Contains("API_KEY_INVALID", StringComparison.OrdinalIgnoreCase))
                throw new GeminiCallException(AnalysisErrorCode.AuthenticationFailed, false);
            if (status == HttpStatusCode.NotFound) throw new GeminiCallException(AnalysisErrorCode.ModelUnavailable, false);
            if (status == HttpStatusCode.TooManyRequests)
            {
                var now = clock.GetUtcNow();
                var daily = errorText.Contains("perday", StringComparison.OrdinalIgnoreCase) || errorText.Contains("per_day", StringComparison.OrdinalIgnoreCase) || errorText.Contains("daily", StringComparison.OrdinalIgnoreCase);
                var delay = response.Headers.RetryAfter?.Delta ?? (response.Headers.RetryAfter?.Date - now);
                if (delay is null && body?.RootElement.ValueKind == JsonValueKind.Object &&
                    body.RootElement.TryGetProperty("error", out var error) && error.ValueKind == JsonValueKind.Object &&
                    error.TryGetProperty("details", out var details) && details.ValueKind == JsonValueKind.Array)
                    foreach (var detail in details.EnumerateArray())
                        if (detail.ValueKind == JsonValueKind.Object && detail.TryGetProperty("retryDelay", out var value) && value.ValueKind == JsonValueKind.String &&
                            double.TryParse(value.GetString()?.TrimEnd('s'), System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var seconds) &&
                            double.IsFinite(seconds) && seconds <= (DateTimeOffset.MaxValue - now).TotalSeconds - 1)
                            delay = TimeSpan.FromSeconds(Math.Max(1, seconds));
                delay ??= TimeSpan.FromMinutes(1);
                if (delay < TimeSpan.Zero) delay = TimeSpan.FromSeconds(1);
                var until = daily ? LlmQuotaService.GetNextQuotaDay(now) : now.Add(delay.Value);
                throw new GeminiCallException(AnalysisErrorCode.QuotaExceeded, !daily && delay.Value.TotalSeconds <= options.Value.MaxRetryDelaySeconds, delay, until);
            }
            if (status == HttpStatusCode.RequestTimeout || (int)status >= 500)
                throw new GeminiCallException(AnalysisErrorCode.ProviderUnavailable, true);
            throw new GeminiCallException(AnalysisErrorCode.ConfigurationError, false);
        }
    }

    private async Task DelayUntilAsync(DateTimeOffset until, CancellationToken cancellationToken)
    {
        var delay = until - clock.GetUtcNow();
        if (delay > TimeSpan.Zero) await Task.Delay(delay, clock, cancellationToken);
    }

    private static int ReadTokenCount(JsonElement usage, string name) =>
        usage.TryGetProperty(name, out var value) && value.TryGetInt32(out var count) ? Math.Clamp(count, 0, int.MaxValue / 2) : 0;

    private sealed class GeminiCallException(AnalysisErrorCode code, bool retryable, TimeSpan? delay = null,
        DateTimeOffset? blockUntil = null, string reason = "ProviderResponse")
        : Exception("Harici model isteği tamamlanamadı.")
    {
        public AnalysisErrorCode Code { get; } = code;
        public bool Retryable { get; } = retryable;
        public TimeSpan? Delay { get; } = delay;
        public DateTimeOffset? BlockUntil { get; } = blockUntil;
        public string Reason { get; } = reason;
    }
}
