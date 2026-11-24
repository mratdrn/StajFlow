using System.Text.Json;
using StajFlow.Api.DTOs.Analysis;
using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Applications;

public sealed record ApplicationEvaluationDto(string SchemaVersion, string ScoringRuleVersion,
    EvaluationStatus Status, decimal? Score, EvaluationUnavailableReason? UnavailableReason,
    RequiredCriteriaStatus RequiredCriteriaStatus, DateTimeOffset EvaluatedAt, ApplicationEvaluationData? Data)
{
    internal ApplicationEvaluationDto WithData(string? json)
    {
        if (json is null || SchemaVersion != "1") return this;
        try { return this with { Data = JsonSerializer.Deserialize<ApplicationEvaluationData>(json, JsonSerializerOptions.Web) }; }
        catch (JsonException) { return this; }
    }
}
