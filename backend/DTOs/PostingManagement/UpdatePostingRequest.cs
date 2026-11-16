using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.PostingManagement;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdatePostingRequest : PostingContentRequest
{
    [Required(ErrorMessage = "İlanın güncel sürümü gereklidir.")]
    [JsonPropertyName("rowVersion")]
    public string RowVersion { get; set; } = string.Empty;

    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var error in base.Validate(validationContext)) yield return error;
        if (!Convert.TryFromBase64String(RowVersion, new byte[8], out var length) || length != 8)
            yield return new ValidationResult("İlan sürümü geçersiz; ilanı yeniden yükleyin.", [nameof(RowVersion)]);
    }
}
