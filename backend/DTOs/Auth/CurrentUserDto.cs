using System.Text.Json.Serialization;
namespace StajFlow.Api.DTOs.Auth;

public record CurrentUserDto([property: JsonPropertyName("id")] string Id, [property: JsonPropertyName("email")] string Email, [property: JsonPropertyName("displayName")] string DisplayName, [property: JsonPropertyName("role")] string Role, [property: JsonPropertyName("profileId")] int ProfileId);

