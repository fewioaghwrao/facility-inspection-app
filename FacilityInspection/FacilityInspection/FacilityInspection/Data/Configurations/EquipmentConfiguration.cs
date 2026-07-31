using FacilityInspection.Domain.Equipments;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class EquipmentConfiguration
    : IEntityTypeConfiguration<Equipment>
{
    public void Configure(
        EntityTypeBuilder<Equipment> builder)
    {
        builder.ToTable("Equipments");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.EquipmentCode)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => x.EquipmentCode)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.EquipmentType)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.Manufacturer)
            .HasMaxLength(100);

        builder.Property(x => x.ModelNumber)
            .HasMaxLength(100);

        builder.Property(x => x.SerialNumber)
            .HasMaxLength(100);

        builder.Property(x => x.Notes)
            .HasMaxLength(1000);

        builder.HasIndex(x => x.LocationId);
        builder.HasIndex(x => x.EquipmentType);
        builder.HasIndex(x => x.Status);
    }
}