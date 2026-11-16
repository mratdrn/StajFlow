using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

public enum RequirementType { Required, Preferred }

[Table("InternshipRequirements")]
public class InternshipRequirement
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("InternshipPostingId")]
    public int InternshipPostingId { get; set; }
    [Column("Description")]
    public required string Description { get; set; }
    public RequirementType Type { get; set; }
    public InternshipPosting InternshipPosting { get; set; } = null!;
}

