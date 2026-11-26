namespace StajFlow.Api.Configurations;

public class GroqOptions
{
    public const string SectionName = "Groq";
    public bool Enabled { get; set; } = true;
    public string BaseUrl { get; set; } = "https://api.groq.com/openai/v1/";
    public string Model { get; set; } = "openai/gpt-oss-120b";
    public int DailyRequestLimit { get; set; } = 200;
    public int RequestsPerMinute { get; set; } = 10;
    public int MaxInputCharacters { get; set; } = 20000;
    public int MaxOutputTokens { get; set; } = 4096;
    public int TokensPerMinute { get; set; } = 7200;
    public int DailyTokenLimit { get; set; } = 180000;
    public int TokenSafetyMargin { get; set; } = 256;
    public string ReasoningEffort { get; set; } = "low";
    public int MaxResponseBytes { get; set; } = 1024 * 1024;
    public int RequestTimeoutSeconds { get; set; } = 90;
    public int RetryBaseDelaySeconds { get; set; } = 30;
    public int MaxRetryDelaySeconds { get; set; } = 90;
}
