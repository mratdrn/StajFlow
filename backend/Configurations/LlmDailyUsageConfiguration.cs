using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using StajFlow.Api.Entities;

namespace StajFlow.Api.Configurations;

public class LlmDailyUsageConfiguration : IEntityTypeConfiguration<LlmDailyUsage>
{
    public void Configure(EntityTypeBuilder<LlmDailyUsage> builder)
    {
        builder.ToTable("LlmDailyUsage", table => table.HasCheckConstraint("CK_LlmDailyUsage_Counts",
            "[RequestCount] >= 0 AND [WindowRequestCount] >= 0 AND [InputTokens] >= 0 AND [OutputTokens] >= 0"));
        builder.HasKey(record => record.Date);
        builder.Property(record => record.RowVersion).IsRowVersion();
    }
}
