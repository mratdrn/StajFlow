using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class CompanyProfileConfiguration : IEntityTypeConfiguration<CompanyProfile>
{
    public void Configure(EntityTypeBuilder<CompanyProfile> builder)
    {
        builder.ToTable("CompanyProfiles", table => table.HasCheckConstraint("CK_CompanyProfiles_Logo",
            "([LogoData] IS NULL AND [LogoContentType] IS NULL AND [LogoVersion] IS NULL) OR " +
            "([LogoData] IS NOT NULL AND DATALENGTH([LogoData]) BETWEEN 1 AND 2097152 AND " +
            "[LogoContentType] IS NOT NULL AND [LogoContentType] IN ('image/png','image/jpeg') AND [LogoVersion] IS NOT NULL)"));
        builder.HasKey(record => record.Id);
        builder.Property(record => record.LogoContentType).HasMaxLength(20);
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

