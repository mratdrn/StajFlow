using System.Text.Json.Serialization;

namespace StajFlow.Api.DTOs.Analysis;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MatchingProfile
{
    [JsonPropertyName("departments")]
    public required List<string> Departments { get; init; }

    [JsonPropertyName("minExperienceMonths")]
    public required int? MinExperienceMonths { get; init; }

    [JsonPropertyName("experiences")]
    public required List<MatchingPeriod> Experiences { get; init; }

    [JsonPropertyName("skills")]
    public required List<MatchingSkill> Skills { get; init; }

    [JsonPropertyName("softSkills")]
    public required List<string> SoftSkills { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MatchingPeriod
{
    [JsonPropertyName("start")]
    public required string Start { get; init; }

    [JsonPropertyName("end")]
    public required string End { get; init; }
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed record MatchingSkill
{
    [JsonPropertyName("names")]
    public required List<string> Names { get; init; }

    [JsonPropertyName("required")]
    public required bool Required { get; init; }
}
