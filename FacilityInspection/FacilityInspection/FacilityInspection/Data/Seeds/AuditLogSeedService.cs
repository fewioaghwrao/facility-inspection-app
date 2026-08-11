using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.Operators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class AuditLogSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        // ========================================
        // 二重登録防止
        // ========================================

        if (await dbContext.AuditLogs
                .AnyAsync(cancellationToken))
        {
            return;
        }

        var today =
            DateOnly.FromDateTime(
                DateTime.Today);


        // ========================================
        // 保全責任者
        // ========================================

        var manager =
            await dbContext.Operators
                .FirstOrDefaultAsync(
                    x =>
                        x.Role ==
                        OperatorRole.MaintenanceManager,
                    cancellationToken)
            ?? throw new InvalidOperationException(
                "保全責任者が見つかりませんでした。");


        // ========================================
        // InspectionSeedService と同じ対象設備
        // ========================================

        var targetEquipmentCodes = new[]
        {
            "AC-001",
            "WP-001",
            "VE-001"
        };

        var schedules =
            await dbContext.InspectionSchedules
                .Include(x => x.Equipment)
                .Include(x => x.Inspection)
                .Where(x =>
                    !x.IsCancelled &&
                    x.ScheduledDate < today &&
                    x.Inspection != null &&
                    targetEquipmentCodes.Contains(
                        x.Equipment.EquipmentCode))
                .OrderByDescending(x =>
                    x.ScheduledDate)
                .ToListAsync(
                    cancellationToken);


        var compressorSchedule =
            GetRequiredSchedule(
                schedules,
                "AC-001");

        var pumpSchedule =
            GetRequiredSchedule(
                schedules,
                "WP-001");

        var ventilationSchedule =
            GetRequiredSchedule(
                schedules,
                "VE-001");


        // ========================================
        // AC-001
        //
        // 09:00 開始
        // 10:00 完了
        // 11:00 承認
        // ========================================

        AddInspectionStartLog(
            dbContext,
            compressorSchedule);

        AddInspectionCompleteLog(
            dbContext,
            compressorSchedule);

        AddApproveLog(
            dbContext,
            compressorSchedule,
            manager.Id);


        // ========================================
        // WP-001
        //
        // 09:00 開始
        // 10:00 完了
        // 現在：承認待ち
        // ========================================

        AddInspectionStartLog(
            dbContext,
            pumpSchedule);

        AddInspectionCompleteLog(
            dbContext,
            pumpSchedule);


        // ========================================
        // VE-001
        //
        // 09:00 開始
        // 10:00 完了
        // 11:00 差し戻し
        // ========================================

        AddInspectionStartLog(
            dbContext,
            ventilationSchedule);

        AddInspectionCompleteLog(
            dbContext,
            ventilationSchedule);

        AddReturnLog(
            dbContext,
            ventilationSchedule,
            manager.Id);


        await dbContext.SaveChangesAsync(
            cancellationToken);
    }


    // ============================================
    // Schedule
    // ============================================

    private static InspectionSchedule
        GetRequiredSchedule(
            IEnumerable<InspectionSchedule> schedules,
            string equipmentCode)
    {
        return schedules
                   .Where(x =>
                       x.Equipment.EquipmentCode ==
                       equipmentCode)
                   .OrderByDescending(x =>
                       x.ScheduledDate)
                   .FirstOrDefault()

               ?? throw new InvalidOperationException(
                   $"{equipmentCode}の過去の点検予定が" +
                   "見つかりませんでした。");
    }


    // ============================================
    // 点検開始
    // ============================================

    private static void AddInspectionStartLog(
        InspectionDbContext dbContext,
        InspectionSchedule schedule)
    {
        var inspection =
            GetRequiredInspection(
                schedule);

        var auditLog =
            new AuditLog(
                operatorId:
                    schedule.AssignedOperatorId,

                actionType:
                    AuditActionType.InspectionStart,

                entityType:
                    AuditEntityType.Inspection,

                entityId:
                    inspection.Id,

                beforeValue:
                    InspectionStatus
                        .NotStarted
                        .ToString(),

                afterValue:
                    InspectionStatus
                        .InProgress
                        .ToString(),

                reason:
                    "点検を開始しました。",

                occurredAtUtc:
                    CreateSeedUtc(
                        schedule.ScheduledDate,
                        9));

        dbContext.AuditLogs.Add(
            auditLog);
    }


    // ============================================
    // 点検完了
    // ============================================

    private static void AddInspectionCompleteLog(
        InspectionDbContext dbContext,
        InspectionSchedule schedule)
    {
        var inspection =
            GetRequiredInspection(
                schedule);

        var auditLog =
            new AuditLog(
                operatorId:
                    schedule.AssignedOperatorId,

                actionType:
                    AuditActionType.InspectionComplete,

                entityType:
                    AuditEntityType.Inspection,

                entityId:
                    inspection.Id,

                beforeValue:
                    InspectionStatus
                        .InProgress
                        .ToString(),

                afterValue:
                    InspectionStatus
                        .Completed
                        .ToString(),

                reason:
                    "点検項目の入力を完了しました。",

                occurredAtUtc:
                    CreateSeedUtc(
                        schedule.ScheduledDate,
                        10));

        dbContext.AuditLogs.Add(
            auditLog);
    }


    // ============================================
    // 承認
    // ============================================

    private static void AddApproveLog(
        InspectionDbContext dbContext,
        InspectionSchedule schedule,
        Guid managerId)
    {
        var inspection =
            GetRequiredInspection(
                schedule);

        var auditLog =
            new AuditLog(
                operatorId:
                    managerId,

                actionType:
                    AuditActionType.Approve,

                entityType:
                    AuditEntityType.Inspection,

                entityId:
                    inspection.Id,

                beforeValue:
                    InspectionStatus
                        .Completed
                        .ToString(),

                afterValue:
                    InspectionStatus
                        .Approved
                        .ToString(),

                reason:
                    "点検結果を確認し、承認しました。",

                occurredAtUtc:
                    CreateSeedUtc(
                        schedule.ScheduledDate,
                        11));

        dbContext.AuditLogs.Add(
            auditLog);
    }


    // ============================================
    // 差し戻し
    // ============================================

    private static void AddReturnLog(
        InspectionDbContext dbContext,
        InspectionSchedule schedule,
        Guid managerId)
    {
        var inspection =
            GetRequiredInspection(
                schedule);

        /*
         * InspectionSeedService の差し戻し理由と
         * 完全に同一にする。
         */
        const string returnReason =
            "異常箇所の状況を再確認し、" +
            "追加写真を登録してください。";

        var auditLog =
            new AuditLog(
                operatorId:
                    managerId,

                actionType:
                    AuditActionType.ReturnForCorrection,

                entityType:
                    AuditEntityType.Inspection,

                entityId:
                    inspection.Id,

                beforeValue:
                    InspectionStatus
                        .Completed
                        .ToString(),

                afterValue:
                    InspectionStatus
                        .Returned
                        .ToString(),

                reason:
                    returnReason,

                occurredAtUtc:
                    CreateSeedUtc(
                        schedule.ScheduledDate,
                        11));

        dbContext.AuditLogs.Add(
            auditLog);
    }


    // ============================================
    // Inspection
    // ============================================

    private static Inspection
        GetRequiredInspection(
            InspectionSchedule schedule)
    {
        return schedule.Inspection
               ?? throw new InvalidOperationException(
                   $"{schedule.Equipment.EquipmentCode}の" +
                   "点検実績が存在しません。");
    }


    // ============================================
    // DateTime
    // ============================================

    private static DateTime CreateSeedUtc(
        DateOnly date,
        int hour)
    {
        return DateTime.SpecifyKind(
            date.ToDateTime(
                new TimeOnly(
                    hour,
                    0)),
            DateTimeKind.Utc);
    }
}