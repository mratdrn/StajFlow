using StajFlow.Api.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace StajFlow.Api.Configurations;

public class InternshipRequirementConfiguration : IEntityTypeConfiguration<InternshipRequirement>
{
    public void Configure(EntityTypeBuilder<InternshipRequirement> builder)
    {
        builder.ToTable("InternshipRequirements", table =>
            table.HasCheckConstraint("CK_InternshipRequirements_Type", "[Type] IN ('Required','Preferred')"));
        builder.HasKey(record => record.Id);
        builder.Property(record => record.Description).HasMaxLength(1000).IsRequired();
        builder.Property(record => record.Type).HasColumnName("Type").HasConversion<string>().HasMaxLength(20);
        builder.HasOne(record => record.InternshipPosting).WithMany(record => record.Requirements)
            .HasForeignKey(record => record.InternshipPostingId).OnDelete(DeleteBehavior.Cascade);
    }
}

