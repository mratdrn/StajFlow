using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Applications;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class CreateApplicationRequest
{
    [JsonPropertyName("internshipPostingId")]
    [Range(1, int.MaxValue, ErrorMessage = "Geçerli bir ilan seçin.")]
    public int InternshipPostingId { get; set; }

    [JsonPropertyName("resumeId")]
    [Range(1, int.MaxValue, ErrorMessage = "Kendi özgeçmişlerinizden birini seçin.")]
    public int ResumeId { get; set; }

    [JsonPropertyName("coverLetter")]
    [StringLength(6000, ErrorMessage = "Ön yazı en fazla 6000 karakter olabilir.")]
    public string? CoverLetter { get; set; }
}
