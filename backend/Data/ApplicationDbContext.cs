using StajFlow.Api.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace StajFlow.Api.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext<ApplicationUser>(options)
{
    public DbSet<StudentProfile> StudentProfiles => Set<StudentProfile>();
    public DbSet<CompanyProfile> CompanyProfiles => Set<CompanyProfile>();
    public DbSet<InternshipPosting> InternshipPostings => Set<InternshipPosting>();
    public DbSet<InternshipRequirement> InternshipRequirements => Set<InternshipRequirement>();
    public DbSet<InternshipApplication> Applications => Set<InternshipApplication>();
    public DbSet<Resume> Resumes => Set<Resume>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "role-student", Name = Entities.UserRoles.Student, NormalizedName = "STUDENT", ConcurrencyStamp = "role-student-v1" },
            new IdentityRole { Id = "role-company", Name = Entities.UserRoles.Company, NormalizedName = "COMPANY", ConcurrencyStamp = "role-company-v1" });
    }
}

