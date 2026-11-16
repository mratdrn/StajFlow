using System.Text.Json.Serialization;
using System.ComponentModel.DataAnnotations;

namespace StajFlow.Api.DTOs.Auth;

public abstract class EmailRequest
{
    private string email = string.Empty;

    [Required(ErrorMessage = "Bu alan zorunludur."), EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi girin."), StringLength(254, ErrorMessage = "Bu alan en fazla {1} karakter olabilir.")]
    [JsonPropertyName("email")]
    public string Email
    {
        get => email;
        set => email = value?.Trim() ?? string.Empty;
    }
}

