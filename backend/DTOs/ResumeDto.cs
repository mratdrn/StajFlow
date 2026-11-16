using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs;

public record ResumeDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("originalFileName")] string OriginalFileName,
    [property: JsonPropertyName("fileSizeBytes")] long FileSizeBytes,
    [property: JsonPropertyName("uploadedAt")] DateTimeOffset UploadedAt);

public record ResumeListDto(
    [property: JsonPropertyName("maxFileSizeBytes")] long MaxFileSizeBytes,
    [property: JsonPropertyName("resumes")] IReadOnlyList<ResumeDto> Resumes);
