using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.PostingManagement;

public record CompanyPostingDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("description")] string Description,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("workArrangement")] string WorkArrangement,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("applicationDeadline")] DateTimeOffset? ApplicationDeadline,
    [property: JsonPropertyName("startDate")] DateOnly? StartDate,
    [property: JsonPropertyName("endDate")] DateOnly? EndDate,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("requirements")] IReadOnlyList<InternshipRequirementDto> Requirements,
    [property: JsonPropertyName("rowVersion")] string RowVersion,
    [property: JsonPropertyName("shortDescription")] string? ShortDescription = null);
