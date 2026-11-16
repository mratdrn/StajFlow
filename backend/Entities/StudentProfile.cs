using System.ComponentModel.DataAnnotations.Schema;

namespace StajFlow.Api.Entities;

[Table("StudentProfiles")]
public class StudentProfile
{
    [Column("Id")]
    public int Id { get; set; }
    [Column("IdentityUserId")]
    public string? IdentityUserId { get; set; }
    [Column("FirstName")]
    public required string FirstName { get; set; }
    [Column("LastName")]
    public required string LastName { get; set; }
    [Column("Email")]
    public required string Email { get; set; }
    [Column("PhoneNumber")]
    public string? PhoneNumber { get; set; }
    [Column("University")]
    public string? University { get; set; }
    [Column("Department")]
    public string? Department { get; set; }
    [Column("Biography")]
    public string? Biography { get; set; }
    [Column("CreatedAt")]
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public ICollection<Resume> Resumes { get; set; } = new List<Resume>();
    public ICollection<InternshipApplication> Applications { get; set; } = new List<InternshipApplication>();
}

