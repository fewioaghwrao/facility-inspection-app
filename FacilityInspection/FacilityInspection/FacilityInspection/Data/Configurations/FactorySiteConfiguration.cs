using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class FactorySiteConfiguration
    : IEntityTypeConfiguration<FactorySite>
{
    public void Configure(
        EntityTypeBuilder<FactorySite> builder)
    {
        builder.ToTable("FactorySites");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Code)
            .IsRequired()
            .HasMaxLength(20);

        builder.HasIndex(x => x.Code)
            .IsUnique();

        builder.Property(x => x.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(x => x.Description)
            .HasMaxLength(500);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.HasMany(x => x.Locations)
            .WithOne(x => x.FactorySite)
            .HasForeignKey(x => x.FactorySiteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}