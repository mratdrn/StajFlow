using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class InternshipApplicationConfiguration : IEntityTypeConfiguration<InternshipApplication>
{
    public void Configure(EntityTypeBuilder<InternshipApplication> builder)
    {
        builder.ToTable("Applications", table =>
        {
            table.HasCheckConstraint("CK_Applications_Status", "[Status] IN ('Submitted','UnderReview','Shortlisted','Accepted','Rejected','Withdrawn')");
            table.HasCheckConstraint("CK_Applications_StatusUpdatedAt", "[StatusUpdatedAt] IS NULL OR [StatusUpdatedAt] >= [AppliedAt]");
        });
        builder.HasKey(record => record.Id);
        builder.HasIndex(record => new { record.StudentProfileId, record.InternshipPostingId }).IsUnique()
            .HasFilter("[Status] <> 'Withdrawn'");
        builder.HasIndex(record => new { record.InternshipPostingId, record.Status });
        builder.Property(record => record.CoverLetter).HasMaxLength(6000);
        builder.Property(record => record.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(20);
        builder.Property(record => record.AppliedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        builder.Property(record => record.RowVersion).IsRowVersion();
        builder.HasOne(record => record.StudentProfile).WithMany(record => record.Applications)
            .HasForeignKey(record => record.StudentProfileId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(record => record.InternshipPosting).WithMany(record => record.Applications)
            .HasForeignKey(record => record.InternshipPostingId).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(record => record.Resume).WithMany(record => record.Applications)
            .HasForeignKey(record => new { record.ResumeId, record.StudentProfileId })
            .HasPrincipalKey(record => new { record.Id, record.StudentProfileId }).OnDelete(DeleteBehavior.NoAction);
    }
}

