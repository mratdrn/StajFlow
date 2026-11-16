using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs;

public record CompanyProfileDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("name")] string Name,
    [property: JsonPropertyName("contactEmail")] string ContactEmail,
    [property: JsonPropertyName("industry")] string? Industry,
    [property: JsonPropertyName("location")] string? Location,
    [property: JsonPropertyName("description")] string? Description,
    [property: JsonPropertyName("websiteUrl")] string? WebsiteUrl);

