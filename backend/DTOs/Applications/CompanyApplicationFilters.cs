using System.ComponentModel.DataAnnotations;

namespace StajFlow.Api.DTOs.Applications;

public sealed class CompanyApplicationFilters : IValidatableObject
{
    [AllowedValues(null, "Newest", "ScoreDesc", "ScoreAsc", ErrorMessage = "Sıralama seçimi geçersiz.")]
    public string? Sort { get; set; }

    [AllowedValues(null, "Calculated", "NotCalculated", ErrorMessage = "Puan durumu geçersiz.")]
    public string? ScoreStatus { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "En düşük puan 0 ile 100 arasında olmalıdır.")]
    public decimal? MinScore { get; set; }

    [Range(typeof(decimal), "0", "100", ErrorMessage = "En yüksek puan 0 ile 100 arasında olmalıdır.")]
    public decimal? MaxScore { get; set; }

    [AllowedValues(null, "AllMatched", "MissingEvidence", "NoRequiredCriteria", "Unavailable",
        ErrorMessage = "Zorunlu kriter durumu geçersiz.")]
    public string? RequiredCriteriaStatus { get; set; }

    internal bool IsSpecified => Sort is not null || ScoreStatus is not null || MinScore.HasValue ||
        MaxScore.HasValue || RequiredCriteriaStatus is not null;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (MinScore.HasValue && MaxScore < MinScore)
            yield return new ValidationResult("En yüksek puan en düşük puandan küçük olamaz.", [nameof(MaxScore)]);
    }
}
