using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class ResumeConfiguration : IEntityTypeConfiguration<Resume>
{
    public void Configure(EntityTypeBuilder<Resume> builder)
    {
        builder.ToTable("Resumes", table =>
            table.HasCheckConstraint("CK_Resumes_FileSizeBytes", "[FileSizeBytes] > 0"));
        builder.HasKey(record => record.Id);
        builder.HasAlternateKey(record => new { record.Id, record.StudentProfileId });
        builder.Property(record => record.OriginalFileName).HasMaxLength(255).IsRequired();
        builder.Property(record => record.StorageKey).HasMaxLength(450).IsRequired();
        builder.HasIndex(record => record.StorageKey).IsUnique();
        builder.Property(record => record.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(record => record.UploadedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        builder.Property(record => record.IsFavorite).HasDefaultValue(false);
        builder.HasIndex(record => record.StudentProfileId);
        builder.HasIndex(record => record.StudentProfileId, "IX_Resumes_StudentProfileId_Favorite")
            .IsUnique().HasFilter("[IsFavorite] = 1");
        builder.HasOne(record => record.StudentProfile).WithMany(record => record.Resumes)
            .HasForeignKey(record => record.StudentProfileId).OnDelete(DeleteBehavior.NoAction);
    }
}

