using System.Text.Json.Serialization;
namespace StajFlow.Api.DTOs;

public record InternshipRequirementDto([property: JsonPropertyName("id")] int Id, [property: JsonPropertyName("description")] string Description, [property: JsonPropertyName("type")] string Type);

