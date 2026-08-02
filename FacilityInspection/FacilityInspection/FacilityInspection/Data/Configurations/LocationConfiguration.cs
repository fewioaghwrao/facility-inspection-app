using FacilityInspection.Data.Seeds;
using FacilityInspection.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System;

namespace FacilityInspection.Data.Configurations;

public sealed class LocationConfiguration
    : IEntityTypeConfiguration<Location>
{
    public void Configure(
        EntityTypeBuilder<Location> builder)
    {
        var seededAtUtc =
            new DateTime(
                2026,
                8,
                1,
                0,
                0,
                0,
                DateTimeKind.Utc);

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

        builder.HasData(
            new
            {
                Id = SeedDataIds.FirstFactoryCompressorRoomId,
                FactorySiteId = SeedDataIds.FirstFactorySiteId,
                Code = "COMPRESSOR-ROOM",
                Name = "コンプレッサー室",
                Floor = "1F",
                Description = "エアコンプレッサーを設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.FirstFactoryPumpRoomId,
                FactorySiteId = SeedDataIds.FirstFactorySiteId,
                Code = "PUMP-ROOM",
                Name = "ポンプ室",
                Floor = "1F",
                Description = "冷却水ポンプを設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.FirstFactoryVentilationRoomId,
                FactorySiteId = SeedDataIds.FirstFactorySiteId,
                Code = "VENTILATION-ROOM",
                Name = "換気設備室",
                Floor = "2F",
                Description = "換気設備を設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.SecondFactoryPumpRoomId,
                FactorySiteId = SeedDataIds.SecondFactorySiteId,
                Code = "PUMP-ROOM",
                Name = "ポンプ室",
                Floor = "1F",
                Description = "冷却水ポンプを設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.SecondFactoryDustCollectionRoomId,
                FactorySiteId = SeedDataIds.SecondFactorySiteId,
                Code = "DUST-COLLECTION-ROOM",
                Name = "集塵設備室",
                Floor = "2F",
                Description = "集塵設備を設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            },
            new
            {
                Id = SeedDataIds.SecondFactoryOutdoorAreaId,
                FactorySiteId = SeedDataIds.SecondFactorySiteId,
                Code = "OUTDOOR-AREA",
                Name = "屋外設備エリア",
                Floor = (string?)null,
                Description = "屋外設備を設置する区域",
                IsActive = true,
                CreatedAtUtc = seededAtUtc,
                UpdatedAtUtc = seededAtUtc
            });
    }
}