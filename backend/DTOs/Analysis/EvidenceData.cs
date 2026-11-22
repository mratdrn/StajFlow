using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class EvidenceData
{
    [JsonPropertyName("text")]
    public string? Text { get; init; }

    [JsonPropertyName("sourceQuote")]
    public string? SourceQuote { get; init; }

    [JsonPropertyName("sourcePage")]
    public int? SourcePage { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class SocialIndicatorData
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("evidenceStatus")]
    public EvidenceStatus EvidenceStatus { get; init; } = EvidenceStatus.NoEvidence;

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}
