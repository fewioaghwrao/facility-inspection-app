using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionTemplateItemConfiguration
    : IEntityTypeConfiguration<InspectionTemplateItem>
{
    public void Configure(
        EntityTypeBuilder<InspectionTemplateItem> builder)
    {
        builder.ToTable("InspectionTemplateItems");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ItemName)
            .HasMaxLength(150)
            .IsRequired();

        builder.Property(x => x.InputType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Unit)
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.IsRequired)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.InspectionTemplateId,
            x.DisplayOrder
        });
    }
}