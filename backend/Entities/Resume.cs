using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

[Table("Resumes")]
public class Resume
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("StudentProfileId")]
    public int StudentProfileId { get; set; }
    [Column("OriginalFileName")]
    public required string OriginalFileName { get; set; }
    [Column("StorageKey")]
    public required string StorageKey { get; set; }
    [Column("ContentType")]
    public required string ContentType { get; set; }
    [Column("FileSizeBytes")]
    public long FileSizeBytes { get; set; }
    [Column("UploadedAt")]
    public DateTimeOffset UploadedAt { get; set; } = DateTimeOffset.UtcNow;
    public StudentProfile StudentProfile { get; set; } = null!;
    public ICollection<InternshipApplication> Applications { get; set; } = new List<InternshipApplication>();
}

