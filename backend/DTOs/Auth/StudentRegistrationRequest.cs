using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace StajFlow.Api.DTOs.Auth;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class StudentRegistrationRequest : EmailRequest
{
    [Required(ErrorMessage = "Bu alan zorunludur."), StringLength(201, ErrorMessage = "Bu alan en fazla {1} karakter olabilir.")]
    [JsonPropertyName("fullName")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur."), StringLength(128, MinimumLength = 10, ErrorMessage = "Bu alan {2}–{1} karakter uzunluğunda olmalıdır.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur."), Compare(nameof(Password), ErrorMessage = "Parolalar eşleşmiyor.")]
    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

