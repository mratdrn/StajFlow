namespace StajFlow.Api.Entities;

public class LlmDailyUsage
{
    public DateOnly Date { get; set; }
    public int RequestCount { get; set; }
    public DateTimeOffset WindowStartedAt { get; set; }
    public int WindowRequestCount { get; set; }
    public DateTimeOffset? BlockedUntil { get; set; }
    public long InputTokens { get; set; }
    public long OutputTokens { get; set; }
    public byte[] RowVersion { get; set; } = [];
}
