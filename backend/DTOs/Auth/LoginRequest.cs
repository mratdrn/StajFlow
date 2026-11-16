using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace StajFlow.Api.DTOs.Auth;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class LoginRequest : EmailRequest
{
    [Required(ErrorMessage = "Bu alan zorunludur."), StringLength(128, ErrorMessage = "Bu alan en fazla {1} karakter olabilir.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}

