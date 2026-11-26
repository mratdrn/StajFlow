using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.PostingManagement;

public abstract class PostingContentRequest : IValidatableObject
{
    [Required(ErrorMessage = "İlan başlığı zorunludur.")]
    [StringLength(200, ErrorMessage = "Başlık en fazla 200 karakter olabilir.")]
    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [StringLength(1000, ErrorMessage = "Kısa açıklama en fazla 1000 karakter olabilir.")]
    [JsonPropertyName("shortDescription")]
    public string? ShortDescription { get; set; }

    [Required(ErrorMessage = "İlan açıklaması zorunludur.")]
    [StringLength(10000, ErrorMessage = "İlan açıklaması en fazla 10000 karakter olabilir.")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [StringLength(250, ErrorMessage = "Konum en fazla 250 karakter olabilir.")]
    [JsonPropertyName("location")]
    public string? Location { get; set; }

    [Required(ErrorMessage = "Çalışma biçimini seçin.")]
    [AllowedValues("OnSite", "Hybrid", "Remote", ErrorMessage = "Çalışma biçimi geçersiz.")]
    [JsonPropertyName("workArrangement")]
    public string WorkArrangement { get; set; } = "OnSite";

    [Required(ErrorMessage = "İlan durumunu seçin.")]
    [AllowedValues("Draft", "Published", ErrorMessage = "İlan durumu geçersiz.")]
    [JsonPropertyName("status")]
    public string Status { get; set; } = "Published";

    [JsonPropertyName("applicationDeadline")]
    public DateTimeOffset? ApplicationDeadline { get; set; }

    [JsonPropertyName("startDate")]
    public DateOnly? StartDate { get; set; }

    [JsonPropertyName("endDate")]
    public DateOnly? EndDate { get; set; }

    [Required(ErrorMessage = "Nitelik listesi boş bir dizi olabilir; null olamaz.")]
    [JsonPropertyName("requirements")]
    public List<PostingRequirementRequest> Requirements { get; set; } = [];

    public virtual IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (StartDate.HasValue && EndDate < StartDate)
            yield return new ValidationResult("Bitiş tarihi başlangıç tarihinden önce olamaz.", [nameof(EndDate)]);
        if (Requirements is null) yield break;
        if (Requirements.Any(requirement => requirement is null))
            yield return new ValidationResult("Nitelik listesinde boş kayıt bulunamaz.", [nameof(Requirements)]);
        var ids = Requirements.Where(requirement => requirement?.Id is not null)
            .Select(requirement => requirement.Id).ToList();
        if (ids.Count != ids.Distinct().Count())
            yield return new ValidationResult("Aynı nitelik birden fazla gönderilemez.", [nameof(Requirements)]);
    }
}
