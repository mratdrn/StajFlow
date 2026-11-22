using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

public enum ApplicationStatus { Submitted, UnderReview, Shortlisted, Accepted, Rejected, Withdrawn }

[Table("Applications")]
public class InternshipApplication
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("StudentProfileId")]
    public int StudentProfileId { get; set; }
    [Column("InternshipPostingId")]
    public int InternshipPostingId { get; set; }
    [Column("ResumeId")]
    public int ResumeId { get; set; }
    [Column("CoverLetter")]
    public string? CoverLetter { get; set; }
    public ApplicationStatus Status { get; set; } = ApplicationStatus.Submitted;
    [Column("AppliedAt")]
    public DateTimeOffset AppliedAt { get; set; } = DateTimeOffset.UtcNow;
    [Column("StatusUpdatedAt")]
    public DateTimeOffset? StatusUpdatedAt { get; set; }
    [Column("RowVersion")]
    public byte[] RowVersion { get; set; } = [];
    public StudentProfile StudentProfile { get; set; } = null!;
    public InternshipPosting InternshipPosting { get; set; } = null!;
    public Resume Resume { get; set; } = null!;
    public ApplicationEvaluation? Evaluation { get; set; }
}

