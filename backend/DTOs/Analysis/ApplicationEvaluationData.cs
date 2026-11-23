using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ApplicationEvaluationData
{
    [JsonPropertyName("resumeAnalysisStatus")]
    public AnalysisStatus? ResumeAnalysisStatus { get; init; }

    [JsonPropertyName("postingAnalysisStatus")]
    public AnalysisStatus? PostingAnalysisStatus { get; init; }

    [JsonPropertyName("criteria")]
    public List<CriterionEvaluationData> Criteria { get; init; } = [];

    [JsonPropertyName("mandatoryWarnings")]
    public List<MandatoryConditionWarningData> MandatoryWarnings { get; init; } = [];

    [JsonPropertyName("socialIndicators")]
    public List<SocialIndicatorData> SocialIndicators { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class CriterionEvaluationData
{
    [JsonPropertyName("criterionId")]
    public required string CriterionId { get; init; }

    [JsonPropertyName("category")]
    public CriterionCategory Category { get; init; }

    [JsonPropertyName("type")]
    [JsonConverter(typeof(JsonStringEnumConverter<RequirementType>))]
    public RequirementType Type { get; init; }

    [JsonPropertyName("description")]
    public required string Description { get; init; }

    [JsonPropertyName("matchStatus")]
    public CriterionMatchStatus MatchStatus { get; init; } = CriterionMatchStatus.NoEvidence;
    [JsonPropertyName("postingEvidence")]
    public EvidenceData? PostingEvidence { get; init; }

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class MandatoryConditionWarningData
{
    [JsonPropertyName("criterionId")]
    public string? CriterionId { get; init; }

    [JsonPropertyName("message")]
    public required string Message { get; init; }
}
