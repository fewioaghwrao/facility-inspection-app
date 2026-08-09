using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionPhotoConfiguration
    : IEntityTypeConfiguration<InspectionPhoto>
{
    public void Configure(
        EntityTypeBuilder<InspectionPhoto> builder)
    {
        builder.ToTable("InspectionPhotos");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.InspectionId)
            .IsRequired();

        builder.Property(x => x.InspectionResultId);

        builder.Property(x => x.RelativePath)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(x => x.Caption)
            .HasMaxLength(200);

        builder.Property(x => x.DisplayOrder)
            .IsRequired();

        builder.Property(x => x.CapturedAtUtc)
            .IsRequired();

        builder.HasIndex(x => new
        {
            x.InspectionId,
            x.DisplayOrder
        });

        builder.HasIndex(x => x.InspectionResultId);

        // 点検全体に対する写真
        builder.HasOne(x => x.Inspection)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.InspectionId)
            .OnDelete(DeleteBehavior.Cascade);

        // 特定の点検項目に対する写真
        builder.HasOne(x => x.InspectionResult)
            .WithMany(x => x.Photos)
            .HasForeignKey(x => x.InspectionResultId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}