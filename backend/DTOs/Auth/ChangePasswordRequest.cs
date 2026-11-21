using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Auth;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class ChangePasswordRequest
{
    [Required(ErrorMessage = "Bu alan zorunludur."), StringLength(128, ErrorMessage = "Bu alan en fazla {1} karakter olabilir.")]
    [JsonPropertyName("currentPassword")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur."), StringLength(128, MinimumLength = 10, ErrorMessage = "Bu alan {2}–{1} karakter uzunluğunda olmalıdır.")]
    [JsonPropertyName("newPassword")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur."), Compare(nameof(NewPassword), ErrorMessage = "Parolalar eşleşmiyor.")]
    [JsonPropertyName("confirmPassword")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
