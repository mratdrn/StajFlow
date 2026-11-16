using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.HasKey(record => record.Id);
        builder.Property(record => record.IdentityUserId).HasMaxLength(450);
        builder.HasIndex(record => record.IdentityUserId).IsUnique().HasFilter("[IdentityUserId] IS NOT NULL");
        builder.Property(record => record.Name).HasMaxLength(200).IsRequired();
        builder.Property(record => record.ContactEmail).HasMaxLength(254).IsRequired();
        builder.Property(record => record.Description).HasMaxLength(6000);
        builder.Property(record => record.WebsiteUrl).HasMaxLength(2048);
        builder.Property(record => record.Industry).HasMaxLength(150);
        builder.Property(record => record.Location).HasMaxLength(250);
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
    }
}

