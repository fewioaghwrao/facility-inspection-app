using FacilityInspection.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class LocationConfiguration
    : IEntityTypeConfiguration<Location>
{
    public void Configure(
        EntityTypeBuilder<Location> builder)
    {
        builder.ToTable("Locations");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(30);

        builder.HasIndex(x => new
        {
            x.FactorySiteId,
            x.Code
        })
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Floor)
            .HasMaxLength(20);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasMany(x => x.Equipments)
            .WithOne(x => x.Location)
            .HasForeignKey(x => x.LocationId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}