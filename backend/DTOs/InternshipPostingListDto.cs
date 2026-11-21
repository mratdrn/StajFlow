using System.Text.Json.Serialization;
namespace StajFlow.Api.DTOs;

public record InternshipPostingListDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("title")] string Title,
    [property: JsonPropertyName("companyName")] string CompanyName,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("workArrangement")] string WorkArrangement,
    [property: JsonPropertyName("status")] string Status,
    [property: JsonPropertyName("applicationDeadline")] DateTimeOffset? ApplicationDeadline,
    [property: JsonPropertyName("startDate")] DateOnly? StartDate,
    [property: JsonPropertyName("endDate")] DateOnly? EndDate,
    [property: JsonPropertyName("createdAt")] DateTimeOffset CreatedAt,
    [property: JsonPropertyName("shortDescription")] string? ShortDescription = null,
    [property: JsonPropertyName("companyLogoUrl")] string? CompanyLogoUrl = null);

