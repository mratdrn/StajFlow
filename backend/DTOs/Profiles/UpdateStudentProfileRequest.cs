using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Profiles;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateStudentProfileRequest
{
    [Required(ErrorMessage = "Ad zorunludur."), StringLength(100, ErrorMessage = "Ad en fazla 100 karakter olabilir.")]
    [JsonPropertyName("firstName")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Soyad zorunludur."), StringLength(100, ErrorMessage = "Soyad en fazla 100 karakter olabilir.")]
    [JsonPropertyName("lastName")]
    public string LastName { get; set; } = string.Empty;

    [StringLength(32, ErrorMessage = "Telefon numarası en fazla 32 karakter olabilir.")]
    [Phone(ErrorMessage = "Geçerli bir telefon numarası girin.")]
    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [StringLength(200, ErrorMessage = "Üniversite en fazla 200 karakter olabilir.")]
    [JsonPropertyName("university")]
    public string? University { get; set; }

    [StringLength(200, ErrorMessage = "Bölüm en fazla 200 karakter olabilir.")]
    [JsonPropertyName("department")]
    public string? Department { get; set; }

    [StringLength(4000, ErrorMessage = "Tanıtım en fazla 4000 karakter olabilir.")]
    [JsonPropertyName("biography")]
    public string? Biography { get; set; }
}
