using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Applications;

public record InternshipApplicationDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("internshipPostingId")] int InternshipPostingId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("companyName")] string CompanyName,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("workArrangement")] string WorkArrangement,
    [property: JsonPropertyName("postingStatus")] string PostingStatus,
    [property: JsonPropertyName("resumeId")] int ResumeId,
    [property: JsonPropertyName("resumeFileName")] string ResumeFileName,
    [property: JsonPropertyName("coverLetter")] string? CoverLetter,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("appliedAt")] DateTimeOffset AppliedAt,
    [property: JsonPropertyName("statusUpdatedAt")] DateTimeOffset? StatusUpdatedAt,
    [property: JsonPropertyName("applicationDeadline")] DateTimeOffset? ApplicationDeadline = null,
    [property: JsonPropertyName("evaluation")] ApplicationEvaluationDto? Evaluation = null);
