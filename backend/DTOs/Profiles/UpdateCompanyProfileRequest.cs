using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Profiles;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class UpdateCompanyProfileRequest : IValidatableObject
{
    [Required(ErrorMessage = "Şirket adı zorunludur."), StringLength(200, ErrorMessage = "Şirket adı en fazla 200 karakter olabilir.")]
    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [StringLength(6000, ErrorMessage = "Açıklama en fazla 6000 karakter olabilir.")]
    [JsonPropertyName("description")]
    public string? Description { get; set; }

    [StringLength(2048, ErrorMessage = "Web adresi en fazla 2048 karakter olabilir.")]
    [JsonPropertyName("websiteUrl")]
    public string? WebsiteUrl { get; set; }

    [StringLength(150, ErrorMessage = "Sektör en fazla 150 karakter olabilir.")]
    [JsonPropertyName("industry")]
    public string? Industry { get; set; }

    [StringLength(250, ErrorMessage = "Konum en fazla 250 karakter olabilir.")]
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!string.IsNullOrWhiteSpace(WebsiteUrl) &&
            (!Uri.TryCreate(WebsiteUrl.Trim(), UriKind.Absolute, out var address) ||
             (address.Scheme != Uri.UriSchemeHttps && address.Scheme != Uri.UriSchemeHttp) ||
             string.IsNullOrWhiteSpace(address.Host)))
        {
            yield return new ValidationResult("http:// veya https:// ile başlayan geçerli bir web adresi girin.", [nameof(WebsiteUrl)]);
        }
    }
}
