using StajFlow.Api.Entities;

namespace StajFlow.Api.DTOs.Analysis;

public sealed record CompatibilityDto(int PostingId, int? ResumeId, string ScoringRuleVersion,
    EvaluationStatus Status, decimal? Score, EvaluationUnavailableReason? UnavailableReason, string? Message,
    CompatibilityAnalysisSourceDto? ResumeAnalysis, CompatibilityAnalysisSourceDto? PostingAnalysis,
    List<CriterionEvaluationData> Required, List<CriterionEvaluationData> Preferred,
    List<MandatoryConditionWarningData> MandatoryWarnings, List<SocialIndicatorData> SocialIndicators);

public sealed record CompatibilityAnalysisSourceDto(int Id, int Version, string SchemaVersion, AnalysisStatus Status,
    AnalysisErrorCode? ErrorCode, string? SourceRowVersion = null);
