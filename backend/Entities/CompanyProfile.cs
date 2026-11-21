using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

[Table("CompanyProfiles")]
public class CompanyProfile
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("IdentityUserId")]
    public string? IdentityUserId { get; set; }
    [Column("Name")]
    public required string Name { get; set; }
    [Column("ContactEmail")]
    public required string ContactEmail { get; set; }
    [Column("Description")]
    public string? Description { get; set; }
    [Column("WebsiteUrl")]
    public string? WebsiteUrl { get; set; }
    [Column("Industry")]
    public string? Industry { get; set; }
    [Column("Location")]
    public string? Location { get; set; }
    public byte[]? LogoData { get; set; }
    public string? LogoContentType { get; set; }
    public Guid? LogoVersion { get; set; }
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<InternshipPosting> InternshipPostings { get; set; } = new List<InternshipPosting>();
}

