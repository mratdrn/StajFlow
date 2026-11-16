using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Applications;

public record CompanyApplicationDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("internshipPostingId")] int InternshipPostingId,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("studentFullName")] string StudentFullName,
    [property: JsonPropertyName("studentEmail")] string StudentEmail,
    [property: JsonPropertyName("university")] string? University,
    [property: JsonPropertyName("department")] string? Department,
    [property: JsonPropertyName("resumeFileName")] string ResumeFileName,
    [property: JsonPropertyName("coverLetter")] string? CoverLetter,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("appliedAt")] DateTimeOffset AppliedAt,
    [property: JsonPropertyName("statusUpdatedAt")] DateTimeOffset? StatusUpdatedAt,
    [property: JsonPropertyName("rowVersion")] string RowVersion);
