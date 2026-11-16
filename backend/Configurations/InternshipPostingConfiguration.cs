using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class InternshipPostingConfiguration : IEntityTypeConfiguration<InternshipPosting>
{
    public void Configure(EntityTypeBuilder<InternshipPosting> builder)
    {
        builder.ToTable("InternshipPostings", table =>
        {
            table.HasCheckConstraint("CK_InternshipPostings_Status", "[Status] IN ('Draft','Published','Closed')");
            table.HasCheckConstraint("CK_InternshipPostings_Dates", "[EndDate] IS NULL OR [StartDate] IS NULL OR [EndDate] >= [StartDate]");
        });
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Title).HasMaxLength(200).IsRequired();
        builder.Property(record => record.ShortDescription).HasMaxLength(1000);
        builder.Property(record => record.Description).HasMaxLength(10000).IsRequired();
        builder.Property(record => record.Location).HasMaxLength(250);
        builder.Property(record => record.Status).HasColumnName("Status").HasConversion<string>().HasMaxLength(20);
        builder.Property(record => record.CreatedAt).HasDefaultValueSql("SYSDATETIMEOFFSET()");
        builder.Property(record => record.RowVersion).IsRowVersion();
        builder.HasIndex(record => new { record.Status, record.ApplicationDeadline });
        builder.HasOne(record => record.CompanyProfile).WithMany(record => record.InternshipPostings)
            .HasForeignKey(record => record.CompanyProfileId).OnDelete(DeleteBehavior.NoAction);
    }
}

