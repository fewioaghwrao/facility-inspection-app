using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class InspectionScheduleConfiguration
    : IEntityTypeConfiguration<InspectionSchedule>
{
    public void Configure(
        EntityTypeBuilder<InspectionSchedule> builder)
    {
        builder.ToTable("InspectionSchedules");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.ScheduledDate)
            .IsRequired();

        builder.Property(x => x.Notes)
            .HasMaxLength(500);

        builder.Property(x => x.IsCancelled)
            .IsRequired();

        builder.HasOne(x => x.Equipment)
            .WithMany()
            .HasForeignKey(x => x.EquipmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.InspectionTemplate)
            .WithMany()
            .HasForeignKey(x => x.InspectionTemplateId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.AssignedOperator)
            .WithMany()
            .HasForeignKey(x => x.AssignedOperatorId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Inspection)
            .WithOne(x => x.InspectionSchedule)
            .HasForeignKey<Inspection>(
                x => x.InspectionScheduleId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(x => x.ScheduledDate);

        builder.HasIndex(x => new
        {
            x.EquipmentId,
            x.ScheduledDate
        });
    }
}
