using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

public enum PostingStatus { Draft, Published, Closed }

[Table("InternshipPostings")]
public class InternshipPosting
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("CompanyProfileId")]
    public int CompanyProfileId { get; set; }
    [Column("Title")]
    public required string Title { get; set; }
    [Column("ShortDescription")]
    public string? ShortDescription { get; set; }
    [Column("Description")]
    public required string Description { get; set; }
    [Column("Location")]
    public string? Location { get; set; }
    [Column("IsRemote")]
    public bool IsRemote { get; set; }
    public PostingStatus Status { get; set; } = PostingStatus.Draft;
    [Column("ApplicationDeadline")]
    public DateTimeOffset? ApplicationDeadline { get; set; }
    [Column("StartDate")]
    public DateOnly? StartDate { get; set; }
    [Column("EndDate")]
    public DateOnly? EndDate { get; set; }
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    [Column("RowVersion")]
    public byte[] RowVersion { get; set; } = [];
    public CompanyProfile CompanyProfile { get; set; } = null!;
    public ICollection<InternshipRequirement> Requirements { get; set; } = new List<InternshipRequirement>();
    public ICollection<InternshipApplication> Applications { get; set; } = new List<InternshipApplication>();
}

