using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Applications;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateApplicationStatusRequest : IValidatableObject
{
    [Required(ErrorMessage = "Başvuru durumunu seçin.")]
    [AllowedValues("UnderReview", "Accepted", "Rejected", ErrorMessage = "İnceleniyor, Kabul edildi veya Reddedildi seçin.")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [Required(ErrorMessage = "Başvurunun güncel sürümü gereklidir.")]
    [JsonPropertyName("rowVersion")]
    public string RowVersion { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (string.IsNullOrEmpty(RowVersion) || !Convert.TryFromBase64String(RowVersion, new byte[8], out var length) || length != 8)
            yield return new ValidationResult("Başvuru sürümü geçersiz; başvuruyu yeniden yükleyin.", [nameof(RowVersion)]);
    }
}
