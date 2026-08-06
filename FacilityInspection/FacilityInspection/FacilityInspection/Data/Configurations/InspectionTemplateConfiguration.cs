using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionTemplateConfiguration
    : IEntityTypeConfiguration<InspectionTemplate>
{
    public void Configure(
        EntityTypeBuilder<InspectionTemplate> builder)
    {
        builder.ToTable("InspectionTemplates");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Name)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.EquipmentType)
            .HasConversion<int>()
            .IsRequired();

        builder.Property(x => x.Version)
            .IsRequired();

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.EquipmentType,
            x.Version
        }).IsUnique();

        builder.HasMany(x => x.Items)
            .WithOne(x => x.InspectionTemplate)
            .HasForeignKey(x => x.InspectionTemplateId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}