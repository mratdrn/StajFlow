namespace StajFlow.Api.Services;

public sealed class GeminiApiKeyProvider(IHostEnvironment environment)
{
    public string? GetKey()
    {
        var configured = Environment.GetEnvironmentVariable("Gemini__ApiKey");
        if (string.IsNullOrWhiteSpace(configured)) configured = Environment.GetEnvironmentVariable("GEMINI_API_KEY");
        if (!string.IsNullOrWhiteSpace(configured))
        {
            var value = configured.Trim();
            return value.Any(char.IsControl) ? null : value;
        }
        if (!environment.IsDevelopment()) return null;
        var path = Path.Combine(environment.ContentRootPath, ".env");
        try
        {
            foreach (var line in File.ReadLines(path))
            {
                var entry = line.Trim();
                if (entry.StartsWith("export ", StringComparison.Ordinal)) entry = entry[7..].TrimStart();
                var separator = entry.IndexOf('=');
                if (separator < 0 || entry[..separator].Trim() != "GEMINI_API_KEY") continue;
                var value = entry[(separator + 1)..].Trim();
                if (value.Length > 0 && value[0] is '\'' or '"')
                {
                    var closing = value.IndexOf(value[0], 1);
                    if (closing < 0) return null;
                    var suffix = value[(closing + 1)..].Trim();
                    if (suffix.Length > 0 && !suffix.StartsWith('#')) return null;
                    value = value[1..closing];
                }
                else
                {
                    var comment = value.IndexOf(" #", StringComparison.Ordinal);
                    if (comment >= 0) value = value[..comment].TrimEnd();
                }
                return string.IsNullOrWhiteSpace(value) || value.Any(char.IsControl) ? null : value;
            }
        }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException) { }
        return null;
    }
}
