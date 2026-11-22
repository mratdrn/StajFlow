using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PostingAnalysisData
{
    [JsonPropertyName("criteria")]
    public List<PostingCriterionData> Criteria { get; init; } = [];

    [JsonPropertyName("socialExpectations")]
    public List<SocialExpectationData> SocialExpectations { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class PostingCriterionData
{
    [JsonPropertyName("id")]
    public required string Id { get; init; }

    [JsonPropertyName("sourceRequirementId")]
    public int? SourceRequirementId { get; init; }

    [JsonPropertyName("category")]
    public CriterionCategory Category { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<RequirementType>))]
    public RequirementType Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; init; } = [];

    [JsonPropertyName("minimumDurationMonths")]
    public decimal? MinimumDurationMonths { get; init; }

    [JsonPropertyName("educationLevel")]
    public string? EducationLevel { get; init; }

    [JsonPropertyName("fieldOfStudy")]
    public string? FieldOfStudy { get; init; }

    [JsonPropertyName("minimumStudyYear")]
    public int? MinimumStudyYear { get; init; }

    [JsonPropertyName("evidence")]
    public EvidenceData? Evidence { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SocialExpectationData
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("evidence")]
    public EvidenceData? Evidence { get; init; }
}
