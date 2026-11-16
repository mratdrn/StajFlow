using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.PostingManagement;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CreatePostingRequest : PostingContentRequest
{
    public override IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var error in base.Validate(validationContext)) yield return error;
        if (Status == "Closed")
            yield return new ValidationResult("Yeni ilan taslak veya yayımlanmış olarak oluşturulabilir.", [nameof(Status)]);
        if (Requirements?.Any(requirement => requirement?.Id is not null) == true)
            yield return new ValidationResult("Yeni ilan için mevcut nitelik kimliği gönderilemez.", [nameof(Requirements)]);
    }
}
