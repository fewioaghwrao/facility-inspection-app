using FacilityInspection.Data.Seeds;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

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

        var seededAtUtc =
            new DateTime(
                2026,
                8,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

        builder.HasData(
            new
            {
                Id = SeedDataIds.FirstFactorySiteId,
                Code = "SITE-001",
                Name = "第1工場",
                Description = "第1工場の設備を管理する工場マスター",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.SecondFactorySiteId,
                Code = "SITE-002",
                Name = "第2工場",
                Description = "第2工場の設備を管理する工場マスター",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            });
    }
}