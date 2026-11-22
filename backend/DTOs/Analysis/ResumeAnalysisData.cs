using System.Text.Json.Serialization;
using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResumeAnalysisData
{
    [JsonPropertyName("technicalSkills")]
    public List<TechnicalSkillData> TechnicalSkills { get; init; } = [];

    [JsonPropertyName("projects")]
    public List<ResumeProjectData> Projects { get; init; } = [];

    [JsonPropertyName("education")]
    public List<ResumeEducationData> Education { get; init; } = [];

    [JsonPropertyName("experiences")]
    public List<ResumeExperienceData> Experiences { get; init; } = [];

    [JsonPropertyName("socialIndicators")]
    public List<SocialIndicatorData> SocialIndicators { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class TechnicalSkillData
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResumeProjectData
{
    [JsonPropertyName("name")]
    public required string Name { get; init; }

    [JsonPropertyName("description")]
    public string? Description { get; init; }

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; init; } = [];

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResumeEducationData
{
    [JsonPropertyName("institution")]
    public string? Institution { get; init; }

    [JsonPropertyName("fieldOfStudy")]
    public string? FieldOfStudy { get; init; }

    [JsonPropertyName("level")]
    public string? Level { get; init; }

    [JsonPropertyName("studyYear")]
    public int? StudyYear { get; init; }

    [JsonPropertyName("startYear")]
    public int? StartYear { get; init; }

    [JsonPropertyName("endYear")]
    public int? EndYear { get; init; }

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}

[JsonUnmappedMemberHandling(JsonUnmappedMemberHandling.Disallow)]
public sealed class ResumeExperienceData
{
    [JsonPropertyName("type")]
    public ExperienceType Type { get; init; } = ExperienceType.Unspecified;

    [JsonPropertyName("organization")]
    public string? Organization { get; init; }

    [JsonPropertyName("role")]
    public string? Role { get; init; }

    [JsonPropertyName("durationMonths")]
    public decimal? DurationMonths { get; init; }

    [JsonPropertyName("durationText")]
    public string? DurationText { get; init; }

    [JsonPropertyName("tasks")]
    public List<string> Tasks { get; init; } = [];

    [JsonPropertyName("technologies")]
    public List<string> Technologies { get; init; } = [];

    [JsonPropertyName("evidence")]
    public List<EvidenceData> Evidence { get; init; } = [];
}
