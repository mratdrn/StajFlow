using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StajFlow.Api.Configurations;
using StajFlow.Api.Entities;
using StajFlow.Api.Services.Interfaces;

namespace StajFlow.Api.Services;

public sealed class GroqService(HttpClient client, GroqApiKeyProvider keyProvider, ILlmQuotaService quota,
    IOptions<GroqOptions> options, GroqTokenBudget tokenBudget, TimeProvider clock, ILogger<GroqService>? logger = null) : ILlmService
{
    public async Task<LlmJsonResult> GenerateJsonAsync(LlmJsonRequest request, CancellationToken cancellationToken)
    {
        var settings = options.Value;
        var key = keyProvider.GetKey();
        if (!settings.Enabled || string.IsNullOrWhiteSpace(key)) return LlmJsonResult.Failure(AnalysisErrorCode.ConfigurationError);
        if (request.Text.Length > settings.MaxInputCharacters) return LlmJsonResult.Failure(AnalysisErrorCode.InputTooLong);
        var schema = GroqJsonSchema.Create(request.Schema);
        var lastError = AnalysisErrorCode.ProviderUnavailable;
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var tokens = tokenBudget.Estimate(request, schema);
            if (tokens.Total > settings.TokensPerMinute) return LlmJsonResult.Failure(AnalysisErrorCode.InputTooLong);
            var reservation = await quota.ReserveAsync(tokens, cancellationToken);
            while (!reservation.Allowed && reservation.RetryAt is { } retryAt)
            {
                var wait = retryAt - clock.GetUtcNow();
                if (wait > TimeSpan.Zero) await Task.Delay(wait, clock, cancellationToken);
                reservation = await quota.ReserveAsync(tokens, cancellationToken);
            }
            if (!reservation.Allowed) return LlmJsonResult.Failure(AnalysisErrorCode.QuotaExceeded);
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(settings.RequestTimeoutSeconds));
            TimeSpan? retryDelay = null;
            try
            {
                using var message = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(settings.BaseUrl), "chat/completions"));
                message.Headers.Authorization = new AuthenticationHeaderValue("Bearer", key);
                message.Headers.Accept.ParseAdd("application/json");
                message.Content = JsonContent.Create(new
                {
                    model = settings.Model,
                    messages = new[]
                    {
                        new { role = "system", content = request.CreateInstruction() },
                        new { role = "user", content = request.CreateUserMessage() }
                    },
                    response_format = new { type = "json_schema", json_schema = new { name = request.TextPropertyName + "Analysis", strict = true, schema } },
                    max_completion_tokens = tokens.OutputTokens,
                    reasoning_effort = settings.ReasoningEffort,
                    include_reasoning = false,
                    stream = false
                });
                using var response = await SendAsync(message, timeout.Token);
                var root = response.RootElement;
                if (root.TryGetProperty("usage", out var usage) && usage.ValueKind == JsonValueKind.Object &&
                    TryReadTokenCount(usage, "prompt_tokens", out var inputTokens) && TryReadTokenCount(usage, "completion_tokens", out var outputTokens))
                    await quota.RecordTokensAsync(reservation, inputTokens, outputTokens, cancellationToken);
                if (!root.TryGetProperty("model", out var model) || model.ValueKind != JsonValueKind.String || model.GetString() != settings.Model)
                    return LlmJsonResult.Failure(AnalysisErrorCode.ModelUnavailable);
                if (!root.TryGetProperty("choices", out var choices) || choices.ValueKind != JsonValueKind.Array || choices.GetArrayLength() != 1)
                    throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ChoiceShape");
                var choice = choices[0];
                var finish = choice.GetProperty("finish_reason").GetString();
                if (finish == "content_filter") return LlmJsonResult.Failure(AnalysisErrorCode.ContentBlocked);
                if (finish != "stop")
                    throw new GroqCallException(AnalysisErrorCode.InvalidResponse,
                        finish != "length" || request.TryIncreaseOutputTokens(settings.MaxOutputTokens),
                        reason: finish == "length" ? "OutputTokenLimit" : "CompletionState");
                var content = choice.GetProperty("message");
                if (content.TryGetProperty("refusal", out var refusal) && refusal.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(refusal.GetString()))
                    return LlmJsonResult.Failure(AnalysisErrorCode.ContentBlocked);
                var text = content.GetProperty("content").GetString();
                if (string.IsNullOrWhiteSpace(text)) throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true, reason: "EmptyContent");
                var json = request.Validate(text);
                if (json is null) throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ValidationRejected");
                return LlmJsonResult.Success(json);
            }
            catch (GroqCallException error)
            {
                lastError = error.Code;
                logger?.LogWarning("Groq denemesi başarısız. Attempt: {Attempt}, Code: {Code}, Reason: {Reason}", attempt + 1, error.Code, error.Reason);
                if (error.BlockUntil is { } until) await quota.BlockAsync(until, cancellationToken);
                if (!error.Retryable) return LlmJsonResult.Failure(error.Code);
                retryDelay = error.Delay;
            }
            catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
            {
                lastError = AnalysisErrorCode.ProviderUnavailable;
                logger?.LogWarning("Groq denemesi zaman aşımına uğradı. Attempt: {Attempt}", attempt + 1);
            }
            catch (HttpRequestException error)
            {
                lastError = AnalysisErrorCode.ProviderUnavailable;
                logger?.LogWarning("Groq bağlantısı kurulamadı. Attempt: {Attempt}, Category: {Category}", attempt + 1, error.HttpRequestError);
            }
            catch (Exception error) when (error is JsonException or InvalidOperationException or FormatException or KeyNotFoundException)
            {
                lastError = AnalysisErrorCode.InvalidResponse;
                logger?.LogWarning("Groq yanıtı beklenen veri yapısında değil. Attempt: {Attempt}", attempt + 1);
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
                throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ResponseSizeLimit");
            buffer.Write(chunk, 0, count);
        }
        JsonDocument? body = null;
        try { body = JsonDocument.Parse(buffer.ToArray(), new JsonDocumentOptions { MaxDepth = 64 }); }
        catch (JsonException) when (!response.IsSuccessStatusCode) { }
        if (response.IsSuccessStatusCode) return body ?? throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true);
        using (body)
        {
            var status = response.StatusCode;
            logger?.LogWarning("Groq HTTP hatası. StatusCode: {StatusCode}", (int)status);
            var errorText = body?.RootElement.GetRawText() ?? string.Empty;
            if (status is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
                throw new GroqCallException(AnalysisErrorCode.AuthenticationFailed, false);
            if (status == HttpStatusCode.NotFound || errorText.Contains("model_not_found", StringComparison.OrdinalIgnoreCase))
                throw new GroqCallException(AnalysisErrorCode.ModelUnavailable, false);
            if (status == HttpStatusCode.RequestEntityTooLarge) throw new GroqCallException(AnalysisErrorCode.InputTooLong, false);
            if (status == HttpStatusCode.TooManyRequests)
            {
                var now = clock.GetUtcNow();
                var daily = errorText.Contains("per day", StringComparison.OrdinalIgnoreCase) || errorText.Contains("TPD", StringComparison.OrdinalIgnoreCase) ||
                    errorText.Contains("RPD", StringComparison.OrdinalIgnoreCase) || errorText.Contains("daily", StringComparison.OrdinalIgnoreCase);
                var delay = response.Headers.RetryAfter?.Delta ?? (response.Headers.RetryAfter?.Date - now) ?? TimeSpan.FromMinutes(1);
                if (delay < TimeSpan.FromSeconds(1)) delay = TimeSpan.FromSeconds(1);
                var until = daily && response.Headers.RetryAfter is null ? LlmQuotaService.GetNextQuotaDay(now) : now.Add(delay);
                throw new GroqCallException(AnalysisErrorCode.QuotaExceeded, !daily && delay.TotalSeconds <= options.Value.MaxRetryDelaySeconds, delay, until);
            }
            if (status == HttpStatusCode.RequestTimeout || (int)status >= 500)
                throw new GroqCallException(AnalysisErrorCode.ProviderUnavailable, true);
            if (errorText.Contains("json_validate_failed", StringComparison.OrdinalIgnoreCase))
                throw new GroqCallException(AnalysisErrorCode.InvalidResponse, true, reason: "ProviderValidation");
            throw new GroqCallException(AnalysisErrorCode.ConfigurationError, false);
        }
    }

    private static bool TryReadTokenCount(JsonElement usage, string name, out int count)
    {
        count = 0;
        return usage.TryGetProperty(name, out var value) && value.ValueKind == JsonValueKind.Number && value.TryGetInt32(out count) && count >= 0;
    }

    private sealed class GroqCallException(AnalysisErrorCode code, bool retryable, TimeSpan? delay = null,
        DateTimeOffset? blockUntil = null, string reason = "ProviderResponse") : Exception("Harici model isteği tamamlanamadı.")
    {
        public AnalysisErrorCode Code { get; } = code;
        public bool Retryable { get; } = retryable;
        public TimeSpan? Delay { get; } = delay;
        public DateTimeOffset? BlockUntil { get; } = blockUntil;
        public string Reason { get; } = reason;
    }
}
