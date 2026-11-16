using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class StudentProfileConfiguration : IEntityTypeConfiguration<StudentProfile>
{
    public void Configure(EntityTypeBuilder<StudentProfile> builder)
    {
        builder.HasKey(record => record.Id);
        builder.Property(record => record.IdentityUserId).HasMaxLength(450);
        builder.HasIndex(record => record.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
        builder.Property(record => record.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(record => record.LastName).HasMaxLength(100).IsRequired();
        builder.Property(record => record.Email).HasMaxLength(254).IsRequired();
        builder.Property(record => record.PhoneNumber).HasMaxLength(32);
        builder.Property(record => record.University).HasMaxLength(200);
        builder.Property(record => record.Department).HasMaxLength(200);
        builder.Property(record => record.Biography).HasMaxLength(4000);
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
    }
}

