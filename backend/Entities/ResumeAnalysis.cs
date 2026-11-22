namespace StajFlow.Api.Entities;

public class ResumeAnalysis
{
    public int Id { get; set; }
    public int ResumeId { get; set; }
    public int Version { get; set; } = 1;
    public string SchemaVersion { get; set; } = "1";
    public string? SourceContentHash { get; set; }
    public AnalysisStatus Status { get; set; } = AnalysisStatus.Pending;
    public string? DataJson { get; set; }
    public AnalysisErrorCode? ErrorCode { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? StartedAt { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
    public byte[] RowVersion { get; set; } = [];
    public Resume Resume { get; set; } = null!;
    public ICollection<ApplicationEvaluation> Evaluations { get; set; } = new List<ApplicationEvaluation>();
}
