using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs;

public record StudentProfileDto(
    [property: JsonPropertyName("id")] int Id,
    [property: JsonPropertyName("firstName")] string FirstName,
    [property: JsonPropertyName("lastName")] string LastName,
    [property: JsonPropertyName("email")] string Email,
    [property: JsonPropertyName("university")] string? University,
    [property: JsonPropertyName("department")] string? Department,
    [property: JsonPropertyName("phoneNumber")] string? PhoneNumber,
    [property: JsonPropertyName("biography")] string? Biography);

