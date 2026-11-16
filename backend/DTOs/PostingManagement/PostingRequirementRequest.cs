using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.PostingManagement;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public class PostingRequirementRequest
{
    [Range(1, int.MaxValue, ErrorMessage = "Nitelik kimliği geçersiz.")]
    [JsonPropertyName("id")]
    public int? Id { get; set; }

    [Required(ErrorMessage = "Nitelik açıklaması zorunludur.")]
    [StringLength(1000, ErrorMessage = "Nitelik açıklaması en fazla 1000 karakter olabilir.")]
    [JsonPropertyName("description")]
    public string Description { get; set; } = string.Empty;

    [Required(ErrorMessage = "Nitelik türünü seçin.")]
    [AllowedValues("Required", "Preferred", ErrorMessage = "Zorunlu veya tercih edilen nitelik seçilmelidir.")]
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
}
