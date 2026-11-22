using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Configurations;

public class ResumeAnalysisConfiguration : IEntityTypeConfiguration<ResumeAnalysis>
{
    public void Configure(EntityTypeBuilder<ResumeAnalysis> builder)
    {
        builder.ToTable("ResumeAnalyses", table =>
        {
            table.HasCheckConstraint("CK_ResumeAnalyses_Version", "[Version] > 0 AND LEN(LTRIM(RTRIM([SchemaVersion]))) > 0");
            table.HasCheckConstraint("CK_ResumeAnalyses_Json", "[DataJson] IS NULL OR ISJSON([DataJson]) = 1");
            table.HasCheckConstraint("CK_ResumeAnalyses_SourceContentHash", "[SourceContentHash] IS NULL OR LEN([SourceContentHash]) = 64");
            table.HasCheckConstraint("CK_ResumeAnalyses_ErrorCode",
                "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled','AuthenticationFailed','ModelUnavailable','QuotaExceeded','InputTooLong','ConfigurationError','ContentBlocked')");
            table.HasCheckConstraint("CK_ResumeAnalyses_State",
                "([Status] = 'Pending' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Processing' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Completed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NOT NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Failed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NULL AND [ErrorCode] IS NOT NULL)");
            table.HasCheckConstraint("CK_ResumeAnalyses_Timestamps",
                "([StartedAt] IS NULL OR [StartedAt] >= [CreatedAt]) AND ([CompletedAt] IS NULL OR [CompletedAt] >= [StartedAt])");
        });
        builder.HasKey(record => record.Id);
        builder.HasAlternateKey(record => new { record.Id, record.ResumeId });
        builder.HasIndex(record => new { record.ResumeId, record.Version }).IsUnique();
        builder.HasIndex(record => record.ResumeId).IsUnique()
            .HasDatabaseName("IX_ResumeAnalyses_Active")
            .HasFilter("[Status] IN ('Pending','Processing')");
        builder.Property(record => record.Version).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.SchemaVersion).HasMaxLength(32).IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.SourceContentHash).HasMaxLength(64).IsUnicode(false);
        builder.Property(record => record.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(record => record.ErrorCode).HasConversion<string>().HasMaxLength(32);
        builder.Property(record => record.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.RowVersion).IsRowVersion();
        builder.HasOne(record => record.Resume).WithMany(record => record.Analyses)
            .HasForeignKey(record => record.ResumeId).OnDelete(DeleteBehavior.Cascade);
    }
}
