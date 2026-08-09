using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionResultConfiguration
    : IEntityTypeConfiguration<InspectionResult>
{
    public void Configure(
        EntityTypeBuilder<InspectionResult> builder)
    {
        builder.ToTable("InspectionResults");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InspectionId)
            .IsRequired();

        builder.Property(x => x.InspectionTemplateItemId)
            .IsRequired();

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.ItemName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.InputType)
            .IsRequired();

        builder.Property(x => x.CheckValue);

        builder.Property(x => x.NumericValue)
            .HasPrecision(18, 4);

        builder.Property(x => x.TextValue)
            .HasMaxLength(1000);

        builder.Property(x => x.Unit)
            .HasMaxLength(50);

        builder.Property(x => x.IsAbnormal)
            .IsRequired();

        builder.Property(x => x.Comment)
            .HasMaxLength(1000);

        // 1回の点検につき、同じテンプレート項目の結果は1件だけ
        builder.HasIndex(x => new
        {
            x.InspectionId,
            x.InspectionTemplateItemId
        })
            .IsUnique();

        builder.HasIndex(x => new
        {
            x.InspectionId,
            x.DisplayOrder
        });

        builder.HasOne(x => x.Inspection)
            .WithMany(x => x.Results)
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.InspectionTemplateItem)
            .WithMany()
            .HasForeignKey(x => x.InspectionTemplateItemId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}