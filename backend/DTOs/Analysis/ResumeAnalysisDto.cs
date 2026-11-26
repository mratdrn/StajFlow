using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Analysis;

public record ResumeAnalysisDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("resumeId")] int ResumeId,
    [property: JsonPropertyName("version")] int Version,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("startedAt")] DateTimeOffset? StartedAt,
    [property: JsonPropertyName("completedAt")] DateTimeOffset? CompletedAt,
    [property: JsonPropertyName("errorCode")] string? ErrorCode,
    [property: JsonPropertyName("message")] string? Message,
    [property: JsonPropertyName("data")] MatchingProfile? Data,
    bool RequiresReanalysis = false);
