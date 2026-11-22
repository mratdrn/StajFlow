namespace StajFlow.Api.Entities;

public class ApplicationEvaluation
{
    public int Id { get; init; }
    public int InternshipApplicationId { get; init; }
    public int ResumeId { get; init; }
    public int InternshipPostingId { get; init; }
    public int? ResumeAnalysisId { get; init; }
    public int? PostingAnalysisId { get; init; }
    public string SchemaVersion { get; init; } = "1";
    public required string ScoringRuleVersion { get; init; }
    public EvaluationStatus Status { get; init; } = EvaluationStatus.NotCalculated;
    public decimal? Score { get; init; }
    public EvaluationUnavailableReason? UnavailableReason { get; init; }
    public required string ResultJson { get; init; }
    public DateTimeOffset EvaluatedAt { get; init; } = DateTimeOffset.UtcNow;
    public InternshipApplication InternshipApplication { get; init; } = null!;
    public ResumeAnalysis? ResumeAnalysis { get; init; }
    public PostingAnalysis? PostingAnalysis { get; init; }
}
