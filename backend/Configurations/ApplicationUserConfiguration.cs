using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class ApplicationUserConfiguration : IEntityTypeConfiguration<ApplicationUser>
{
    public void Configure(EntityTypeBuilder<ApplicationUser> builder)
    {
        builder.HasIndex(record => record.NormalizedEmail).HasDatabaseName("EmailIndex")
            .IsUnique().HasFilter("[NormalizedEmail] IS NOT NULL");
        builder.HasOne(record => record.StudentProfile).WithOne()
            .HasForeignKey<StudentProfile>(record => record.IdentityUserId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(record => record.CompanyProfile).WithOne()
            .HasForeignKey<CompanyProfile>(record => record.IdentityUserId).OnDelete(DeleteBehavior.NoAction);
    }
}

