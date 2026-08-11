using FacilityInspection.Domain.AuditLogs;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FacilityInspection.Data.Configurations;

public sealed class AuditLogConfiguration
    : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(
        EntityTypeBuilder<AuditLog> builder)
    {
        // ============================================
        // Table
        // ============================================

        builder.ToTable(
            "AuditLogs");


        // ============================================
        // Primary Key
        // ============================================

        builder.HasKey(x =>
            x.Id);


        // ============================================
        // OccurredAtUtc
        // ============================================

        builder.Property(x =>
                x.OccurredAtUtc)
            .IsRequired();


        // ============================================
        // Operator
        // ============================================

        builder.Property(x =>
                x.OperatorId)
            .IsRequired();

        builder.HasOne(x =>
                x.Operator)
            .WithMany()
            .HasForeignKey(x =>
                x.OperatorId)
            .OnDelete(
                DeleteBehavior.Restrict);


        // ============================================
        // ActionType
        // ============================================

        builder.Property(x =>
                x.ActionType)
            .HasConversion<int>()
            .IsRequired();


        // ============================================
        // EntityType
        // ============================================

        builder.Property(x =>
                x.EntityType)
            .HasConversion<int>()
            .IsRequired();


        // ============================================
        // EntityId
        // ============================================

        builder.Property(x =>
                x.EntityId)
            .IsRequired();


        // ============================================
        // BeforeValue
        // ============================================

        builder.Property(x =>
                x.BeforeValue)
            .HasMaxLength(
                4000);


        // ============================================
        // AfterValue
        // ============================================

        builder.Property(x =>
                x.AfterValue)
            .HasMaxLength(
                4000);


        // ============================================
        // Reason
        // ============================================

        builder.Property(x =>
                x.Reason)
            .HasMaxLength(
                1000);


        // ============================================
        // Index
        // ============================================

        // 新しい履歴から表示するケースが多いため
        builder.HasIndex(x =>
            x.OccurredAtUtc);

        // 操作者別検索
        builder.HasIndex(x =>
            x.OperatorId);

        // 操作種別検索
        builder.HasIndex(x =>
            x.ActionType);

        // 対象エンティティの履歴検索
        builder.HasIndex(x => new
        {
            x.EntityType,
            x.EntityId
        });
    }
}