using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Auth;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class DeleteAccountRequest
{
    [Required(ErrorMessage = "Hesabınızı silmek için mevcut parolanızı girin.")]
    [StringLength(128, ErrorMessage = "Parola en fazla {1} karakter olabilir.")]
    [JsonPropertyName("password")]
    public string Password { get; set; } = string.Empty;
}
