using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Configurations;

public class PostingAnalysisConfiguration : IEntityTypeConfiguration<PostingAnalysis>
{
    public void Configure(EntityTypeBuilder<PostingAnalysis> builder)
    {
        builder.ToTable("PostingAnalyses", table =>
        {
            table.HasCheckConstraint("CK_PostingAnalyses_Version", "[Version] > 0 AND LEN(LTRIM(RTRIM([SchemaVersion]))) > 0");
            table.HasCheckConstraint("CK_PostingAnalyses_Json", "[DataJson] IS NULL OR ISJSON([DataJson]) = 1");
            table.HasCheckConstraint("CK_PostingAnalyses_SourceRowVersion", "DATALENGTH([SourceRowVersion]) = 8");
            table.HasCheckConstraint("CK_PostingAnalyses_ErrorCode",
                "[ErrorCode] IS NULL OR [ErrorCode] IN ('FileNotFound','InvalidPdf','EncryptedPdf','NoText','UnreadableText','FileTooLarge','ReadFailed','ProviderUnavailable','InvalidResponse','ProcessingFailed','Cancelled','AuthenticationFailed','ModelUnavailable','QuotaExceeded','InputTooLong','ConfigurationError','ContentBlocked')");
            table.HasCheckConstraint("CK_PostingAnalyses_State",
                "([Status] = 'Pending' AND [StartedAt] IS NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Processing' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NULL AND [DataJson] IS NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Completed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NOT NULL AND [ErrorCode] IS NULL) OR " +
                "([Status] = 'Failed' AND [StartedAt] IS NOT NULL AND [CompletedAt] IS NOT NULL AND [DataJson] IS NULL AND [ErrorCode] IS NOT NULL)");
            table.HasCheckConstraint("CK_PostingAnalyses_Timestamps",
                "([StartedAt] IS NULL OR [StartedAt] >= [CreatedAt]) AND ([CompletedAt] IS NULL OR [CompletedAt] >= [StartedAt])");
        });
        builder.HasKey(record => record.Id);
        builder.HasAlternateKey(record => new { record.Id, record.InternshipPostingId });
        builder.HasIndex(record => new { record.InternshipPostingId, record.Version }).IsUnique();
        builder.HasIndex(record => new { record.InternshipPostingId, record.SourceRowVersion }).IsUnique()
            .HasDatabaseName("IX_PostingAnalyses_ActiveSource")
            .HasFilter("[Status] IN ('Pending','Processing')");
        builder.Property(record => record.Version).Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.SchemaVersion).HasMaxLength(32).IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.SourceRowVersion).HasColumnType("varbinary(8)").IsRequired()
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.SourceContentHash).HasMaxLength(64).IsUnicode(false)
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(record => record.ErrorCode).HasConversion<string>().HasMaxLength(32);
        builder.Property(record => record.DataJson).HasColumnType("nvarchar(max)");
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()")
            .Metadata.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
        builder.Property(record => record.RowVersion).IsRowVersion();
        builder.HasOne(record => record.InternshipPosting).WithMany(record => record.Analyses)
            .HasForeignKey(record => record.InternshipPostingId).OnDelete(DeleteBehavior.Cascade);
    }
}
