using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionConfiguration
    : IEntityTypeConfiguration<Inspection>
{
    public void Configure(
        EntityTypeBuilder<Inspection> builder)
    {
        builder.ToTable("Inspections");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(x => x.ReturnReason)
            .HasMaxLength(500);

        builder.HasIndex(x => x.InspectionScheduleId)
            .IsUnique();

        builder.HasOne(x => x.PerformedByOperator)
            .WithMany()
            .HasForeignKey(x => x.PerformedByOperatorId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
