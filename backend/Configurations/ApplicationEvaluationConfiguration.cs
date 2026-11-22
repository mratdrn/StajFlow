using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Configurations;

public class ApplicationEvaluationConfiguration : IEntityTypeConfiguration<ApplicationEvaluation>
{
    public void Configure(EntityTypeBuilder<ApplicationEvaluation> builder)
    {
        builder.ToTable("ApplicationEvaluations", table =>
        {
            table.HasCheckConstraint("CK_ApplicationEvaluations_Json", "ISJSON([ResultJson]) = 1");
            table.HasCheckConstraint("CK_ApplicationEvaluations_Versions",
                "LEN(LTRIM(RTRIM([SchemaVersion]))) > 0 AND LEN(LTRIM(RTRIM([ScoringRuleVersion]))) > 0");
            table.HasCheckConstraint("CK_ApplicationEvaluations_Score", "[Score] IS NULL OR [Score] BETWEEN 0 AND 100");
            table.HasCheckConstraint("CK_ApplicationEvaluations_State",
                "([Status] = 'Calculated' AND [Score] IS NOT NULL AND [UnavailableReason] IS NULL AND [ResumeAnalysisId] IS NOT NULL AND [PostingAnalysisId] IS NOT NULL) OR " +
                "([Status] = 'NotCalculated' AND [Score] IS NULL AND [UnavailableReason] IS NOT NULL)");
            table.HasCheckConstraint("CK_ApplicationEvaluations_UnavailableReason",
                "[UnavailableReason] IS NULL OR [UnavailableReason] IN ('AnalysisPending','AnalysisFailed','InsufficientData','NoMeasurableCriteria')");
        });
        builder.HasKey(record => record.Id);
        builder.Property(record => record.SchemaVersion).HasMaxLength(32).IsRequired();
        builder.Property(record => record.ScoringRuleVersion).HasMaxLength(50).IsRequired();
        builder.Property(record => record.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(record => record.Score).HasPrecision(5, 2);
        builder.Property(record => record.UnavailableReason).HasConversion<string>().HasMaxLength(32);
        builder.Property(record => record.ResultJson).HasColumnType("nvarchar(max)").IsRequired();
        builder.Property(record => record.EvaluatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        builder.HasOne(record => record.InternshipApplication).WithOne(record => record.Evaluation)
            .HasForeignKey<ApplicationEvaluation>(record => new { record.InternshipApplicationId, record.ResumeId, record.InternshipPostingId })
            .HasPrincipalKey<InternshipApplication>(record => new { record.Id, record.ResumeId, record.InternshipPostingId })
            .OnDelete(DeleteBehavior.Cascade);
        builder.HasOne(record => record.ResumeAnalysis).WithMany(record => record.Evaluations)
            .HasForeignKey(record => new { record.ResumeAnalysisId, record.ResumeId })
            .HasPrincipalKey(record => new { record.Id, record.ResumeId }).OnDelete(DeleteBehavior.NoAction);
        builder.HasOne(record => record.PostingAnalysis).WithMany(record => record.Evaluations)
            .HasForeignKey(record => new { record.PostingAnalysisId, record.InternshipPostingId })
            .HasPrincipalKey(record => new { record.Id, record.InternshipPostingId }).OnDelete(DeleteBehavior.NoAction);
        foreach (var property in builder.Metadata.GetProperties())
            property.SetAfterSaveBehavior(PropertySaveBehavior.Throw);
    }
}
