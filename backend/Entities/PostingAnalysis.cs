namespace StajFlow.Api.Entities;

public class PostingAnalysis
{
    public int Id { get; set; }
    public int InternshipPostingId { get; set; }
    public int Version { get; set; } = 1;
    public string SchemaVersion { get; set; } = "1";
    public required byte[] SourceRowVersion { get; set; }
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    public string? DataJson { get; set; }
    public AnalysisErrorCode? ErrorCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public InternshipPosting InternshipPosting { get; set; } = null!;
    public ICollection<ApplicationEvaluation> Evaluations { get; set; } = new List<ApplicationEvaluation>();
}
