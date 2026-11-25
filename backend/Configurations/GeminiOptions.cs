namespace StajFlow.Api.Configurations;

public class GeminiOptions
{
    public const string SectionName = "Gemini";
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "https://generativelanguage.googleapis.com/v1beta/";
    public string Model { get; set; } = "gemini-3.5-flash-lite";
    public int DailyRequestLimit { get; set; } = 200;
    public int RequestsPerMinute { get; set; } = 10;
    public int MaxInputCharacters { get; set; } = 20000;
    public int MaxOutputTokens { get; set; } = 8192;
    public int MaxResponseBytes { get; set; } = 1024 * 1024;
    public int RequestTimeoutSeconds { get; set; } = 90;
    public int RetryBaseDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 90;
}
