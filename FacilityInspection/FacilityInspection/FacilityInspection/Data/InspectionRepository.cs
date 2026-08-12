using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using FacilityInspection.Domain.AuditLogs;

namespace FacilityInspection.Data;


// ============================================
// 点検実施状況 一覧
// ============================================

public sealed record InspectionListData(
    Guid ScheduleId,
    Guid? InspectionId,
    DateOnly ScheduledDate,
    string FactorySiteName,
    string LocationName,
    string EquipmentCode,
    string EquipmentName,
    string TemplateName,
    string OperatorName,
    InspectionStatus Status,
    int ResultCount,
    int AbnormalCount,
    int PhotoCount);


// ============================================
// 点検実施詳細
// ============================================

public sealed record InspectionDetailData(
    Guid ScheduleId,
    Guid? InspectionId,
    DateOnly ScheduledDate,
    string FactorySiteName,
    string LocationName,
    string EquipmentCode,
    string EquipmentName,
    string TemplateName,
    string OperatorName,
    InspectionStatus Status,
    IReadOnlyList<InspectionResultDetailData> Results,
    IReadOnlyList<InspectionPhotoDetailData> Photos);


// ============================================
// 点検結果詳細
// ============================================

public sealed record InspectionResultDetailData(
    Guid ResultId,
    int DisplayOrder,
    string ItemName,
    InspectionInputType InputType,
    bool? CheckValue,
    decimal? NumericValue,
    string? TextValue,
    string? Unit,
    bool IsAbnormal,
    string? Comment);


// ============================================
// 点検写真詳細
// ============================================

public sealed record InspectionPhotoDetailData(
    Guid PhotoId,
    Guid? InspectionResultId,
    string RelativePath,
    string? Caption,
    int DisplayOrder,
    DateTime CapturedAtUtc);


// ============================================
// 異常一覧
//
// Inspection単位ではなく、
// InspectionResult単位で1行を返す。
// ============================================

public sealed record AbnormalResultListData(
    Guid ScheduleId,
    Guid InspectionId,
    Guid ResultId,
    DateOnly ScheduledDate,
    string FactorySiteName,
    string LocationName,
    string EquipmentCode,
    string EquipmentName,
    string TemplateName,
    string OperatorName,
    InspectionStatus InspectionStatus,
    int DisplayOrder,
    string ItemName,
    InspectionInputType InputType,
    bool? CheckValue,
    decimal? NumericValue,
    string? TextValue,
    string? Unit,
    string? Comment,
    int PhotoCount);




// ============================================
// 点検担当者向け 点検入力
// ============================================

public sealed record InspectionEntryData(
    Guid ScheduleId,
    Guid InspectionId,
    DateOnly ScheduledDate,
    string FactorySiteName,
    string LocationName,
    string EquipmentCode,
    string EquipmentName,
    string TemplateName,
    InspectionStatus Status,
    IReadOnlyList<InspectionEntryItemData> Items);

public sealed record InspectionEntryItemData(
    Guid TemplateItemId,
    int DisplayOrder,
    string ItemName,
    InspectionInputType InputType,
    string? Unit,
    double? MinimumValue,
    double? MaximumValue,
    bool IsRequired,
    string? Description,
    bool? CheckValue,
    decimal? NumericValue,
    string? TextValue,
    string? Comment);


public sealed class InspectionRepository
{
    private readonly InspectionDbContextFactory
        _dbContextFactory;


    // ============================================
    // Constructor
    // ============================================

    public InspectionRepository(
        InspectionDbContextFactory dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(
            dbContextFactory);

        _dbContextFactory =
            dbContextFactory;
    }


    // ============================================
    // 点検担当者向け 点検開始 / 再開
    // ============================================

    public async Task<InspectionEntryData>
        StartOrResumeAsync(
            Guid scheduleId,
            Guid operatorId,
            CancellationToken cancellationToken = default)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var schedule =
            await dbContext.InspectionSchedules
                .Include(x =>
                    x.Equipment)
                    .ThenInclude(x =>
                        x.Location)
                        .ThenInclude(x =>
                            x.FactorySite)
                .Include(x =>
                    x.InspectionTemplate)
                    .ThenInclude(x =>
                        x.Items)
                .Include(x =>
                    x.Inspection)
                    .ThenInclude(x =>
                        x!.Results)
                .AsSplitQuery()
                .SingleOrDefaultAsync(
                    x =>
                        x.Id == scheduleId &&
                        !x.IsCancelled,
                    cancellationToken);

        if (schedule is null)
        {
            throw new InvalidOperationException(
                "点検予定が見つからないか、取消済みです。");
        }

        if (schedule.AssignedOperatorId !=
            operatorId)
        {
            throw new InvalidOperationException(
                "この点検予定は現在の点検担当者には" +
                "割り当てられていません。");
        }

        var inspection =
            schedule.Inspection;

        var stateChanged =
            false;

        if (inspection is null)
        {
            inspection =
                new Inspection(
                    schedule.Id);

            schedule.AttachInspection(
                inspection);

            dbContext.Inspections.Add(
                inspection);

            inspection.Start(
                operatorId,
                DateTime.UtcNow);

            stateChanged =
                true;
        }
        else
        {
            switch (inspection.Status)
            {
                case InspectionStatus.NotStarted:
                case InspectionStatus.Returned:
                    inspection.Start(
                        operatorId,
                        DateTime.UtcNow);

                    stateChanged =
                        true;
                    break;

                case InspectionStatus.InProgress:
                    if (inspection.PerformedByOperatorId !=
                            operatorId)
                    {
                        throw new InvalidOperationException(
                            "この点検は別の担当者が実施中です。");
                    }

                    break;

                case InspectionStatus.Completed:
                    throw new InvalidOperationException(
                        "この点検は完了済みです。");

                case InspectionStatus.Approved:
                    throw new InvalidOperationException(
                        "この点検は承認済みです。");

                default:
                    throw new InvalidOperationException(
                        $"未対応の点検状態です: " +
                        $"{inspection.Status}");
            }
        }

        if (stateChanged)
        {
            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var existingResults =
            inspection.Results
                .ToDictionary(
                    x =>
                        x.InspectionTemplateItemId);

        var items =
            schedule.InspectionTemplate
                .Items
                .Where(x =>
                    x.IsActive)
                .OrderBy(x =>
                    x.DisplayOrder)
                .Select(item =>
                {
                    existingResults.TryGetValue(
                        item.Id,
                        out var existingResult);

                    return new InspectionEntryItemData(
                        item.Id,
                        item.DisplayOrder,
                        item.ItemName,
                        item.InputType,
                        item.Unit,
                        item.MinimumValue,
                        item.MaximumValue,
                        item.IsRequired,
                        item.Description,
                        existingResult?.CheckValue,
                        existingResult?.NumericValue,
                        existingResult?.TextValue,
                        existingResult?.Comment);
                })
                .ToList();

        return new InspectionEntryData(
            schedule.Id,
            inspection.Id,
            schedule.ScheduledDate,
            schedule.Equipment
                .Location
                .FactorySite
                .Name,
            schedule.Equipment
                .Location
                .Name,
            schedule.Equipment
                .EquipmentCode,
            schedule.Equipment
                .Name,
            schedule.InspectionTemplate
                .Name,
            inspection.Status,
            items);
    }


    // ============================================
    // 点検実施状況 一覧
    // ============================================

    public async Task<IReadOnlyList<InspectionListData>>
        GetAllAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var rows =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .Where(x =>
                    !x.IsCancelled)
                .OrderByDescending(x =>
                    x.ScheduledDate)
                .ThenBy(x =>
                    x.Equipment.EquipmentCode)
                .Select(x => new
                {
                    ScheduleId =
                        x.Id,

                    InspectionId =
                        x.Inspection == null
                            ? (Guid?)null
                            : x.Inspection.Id,

                    x.ScheduledDate,

                    FactorySiteName =
                        x.Equipment
                            .Location
                            .FactorySite
                            .Name,

                    LocationName =
                        x.Equipment
                            .Location
                            .Name,

                    EquipmentCode =
                        x.Equipment
                            .EquipmentCode,

                    EquipmentName =
                        x.Equipment
                            .Name,

                    TemplateName =
                        x.InspectionTemplate
                            .Name,

                    OperatorName =
                        x.AssignedOperator
                            .DisplayName,

                    Status =
                        x.Inspection == null
                            ? InspectionStatus.NotStarted
                            : x.Inspection.Status,

                    ResultCount =
                        x.Inspection == null
                            ? 0
                            : x.Inspection.Results.Count,

                    AbnormalCount =
                        x.Inspection == null
                            ? 0
                            : x.Inspection.Results.Count(
                                result =>
                                    result.IsAbnormal),

                    PhotoCount =
                        x.Inspection == null
                            ? 0
                            : x.Inspection.Photos.Count
                })
                .ToListAsync(
                    cancellationToken);

        return rows
            .Select(x =>
                new InspectionListData(
                    x.ScheduleId,
                    x.InspectionId,
                    x.ScheduledDate,
                    x.FactorySiteName,
                    x.LocationName,
                    x.EquipmentCode,
                    x.EquipmentName,
                    x.TemplateName,
                    x.OperatorName,
                    x.Status,
                    x.ResultCount,
                    x.AbnormalCount,
                    x.PhotoCount))
            .ToList();
    }


    // ============================================
    // 点検実施詳細
    // ============================================

    public async Task<InspectionDetailData?>
        GetDetailAsync(
            Guid scheduleId,
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        /*
         * まず予定を取得する。
         *
         * 未実施の場合はInspectionが存在しないため、
         * InspectionScheduleを起点にする。
         */
        var schedule =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .Where(x =>
                    x.Id == scheduleId &&
                    !x.IsCancelled)
                .Select(x => new
                {
                    ScheduleId =
                        x.Id,

                    InspectionId =
                        x.Inspection == null
                            ? (Guid?)null
                            : x.Inspection.Id,

                    x.ScheduledDate,

                    FactorySiteName =
                        x.Equipment
                            .Location
                            .FactorySite
                            .Name,

                    LocationName =
                        x.Equipment
                            .Location
                            .Name,

                    EquipmentCode =
                        x.Equipment
                            .EquipmentCode,

                    EquipmentName =
                        x.Equipment
                            .Name,

                    TemplateName =
                        x.InspectionTemplate
                            .Name,

                    OperatorName =
                        x.AssignedOperator
                            .DisplayName,

                    Status =
                        x.Inspection == null
                            ? InspectionStatus.NotStarted
                            : x.Inspection.Status
                })
                .SingleOrDefaultAsync(
                    cancellationToken);

        if (schedule is null)
        {
            return null;
        }

        /*
         * 未実施。
         *
         * Inspectionがまだ存在しないため、
         * 基本情報だけを返す。
         */
        if (schedule.InspectionId is not Guid inspectionId)
        {
            return new InspectionDetailData(
                schedule.ScheduleId,
                null,
                schedule.ScheduledDate,
                schedule.FactorySiteName,
                schedule.LocationName,
                schedule.EquipmentCode,
                schedule.EquipmentName,
                schedule.TemplateName,
                schedule.OperatorName,
                schedule.Status,
                [],
                []);
        }

        /*
         * 点検結果
         */
        var resultRows =
            await dbContext
                .Set<InspectionResult>()
                .AsNoTracking()
                .Where(x =>
                    x.InspectionId == inspectionId)
                .OrderBy(x =>
                    x.DisplayOrder)
                .Select(x => new
                {
                    ResultId =
                        x.Id,

                    x.DisplayOrder,
                    x.ItemName,
                    x.InputType,
                    x.CheckValue,
                    x.NumericValue,
                    x.TextValue,
                    x.Unit,
                    x.IsAbnormal,
                    x.Comment
                })
                .ToListAsync(
                    cancellationToken);

        var results =
            resultRows
                .Select(x =>
                    new InspectionResultDetailData(
                        x.ResultId,
                        x.DisplayOrder,
                        x.ItemName,
                        x.InputType,
                        x.CheckValue,
                        x.NumericValue,
                        x.TextValue,
                        x.Unit,
                        x.IsAbnormal,
                        x.Comment))
                .ToList();


        /*
         * 点検写真
         */
        var photoRows =
            await dbContext
                .Set<InspectionPhoto>()
                .AsNoTracking()
                .Where(x =>
                    x.InspectionId == inspectionId)
                .OrderBy(x =>
                    x.DisplayOrder)
                .ThenBy(x =>
                    x.CapturedAtUtc)
                .Select(x => new
                {
                    PhotoId =
                        x.Id,

                    x.InspectionResultId,
                    x.RelativePath,
                    x.Caption,
                    x.DisplayOrder,
                    x.CapturedAtUtc
                })
                .ToListAsync(
                    cancellationToken);

        var photos =
            photoRows
                .Select(x =>
                    new InspectionPhotoDetailData(
                        x.PhotoId,
                        x.InspectionResultId,
                        x.RelativePath,
                        x.Caption,
                        x.DisplayOrder,
                        x.CapturedAtUtc))
                .ToList();


        return new InspectionDetailData(
            schedule.ScheduleId,
            schedule.InspectionId,
            schedule.ScheduledDate,
            schedule.FactorySiteName,
            schedule.LocationName,
            schedule.EquipmentCode,
            schedule.EquipmentName,
            schedule.TemplateName,
            schedule.OperatorName,
            schedule.Status,
            results,
            photos);
    }


    // ============================================
    // 異常一覧
    //
    // InspectionResult.IsAbnormal == true の
    // 点検項目だけを取得する。
    //
    // 例:
    // 1件の点検に異常項目が2件あれば
    // このメソッドでは2件返す。
    // ============================================

    public async Task<
        IReadOnlyList<AbnormalResultListData>>
        GetAbnormalResultsAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        /*
         * SQLiteでは、
         *
         * InspectionSchedule
         *   → Inspection
         *     → InspectionResult
         *
         * をSelectManyするとSQL APPLYへ変換される場合がある。
         *
         * SQLiteはAPPLYをサポートしないため、
         * 明示的なJOINで取得する。
         */

        var rows =
            await (
                from schedule
                    in dbContext.InspectionSchedules
                        .AsNoTracking()

                join inspection
                    in dbContext.Set<Inspection>()
                        .AsNoTracking()
                    on schedule.Id
                    equals inspection.InspectionScheduleId

                join result
                    in dbContext.Set<InspectionResult>()
                        .AsNoTracking()
                    on inspection.Id
                    equals result.InspectionId

                where
                    !schedule.IsCancelled &&
                    result.IsAbnormal

                orderby
                    schedule.ScheduledDate descending,
                    schedule.Equipment.EquipmentCode,
                    result.DisplayOrder

                select new
                {
                    ScheduleId =
                        schedule.Id,

                    InspectionId =
                        inspection.Id,

                    ResultId =
                        result.Id,

                    schedule.ScheduledDate,

                    FactorySiteName =
                        schedule.Equipment
                            .Location
                            .FactorySite
                            .Name,

                    LocationName =
                        schedule.Equipment
                            .Location
                            .Name,

                    EquipmentCode =
                        schedule.Equipment
                            .EquipmentCode,

                    EquipmentName =
                        schedule.Equipment
                            .Name,

                    TemplateName =
                        schedule.InspectionTemplate
                            .Name,

                    OperatorName =
                        schedule.AssignedOperator
                            .DisplayName,

                    InspectionStatus =
                        inspection.Status,

                    result.DisplayOrder,
                    result.ItemName,
                    result.InputType,
                    result.CheckValue,
                    result.NumericValue,
                    result.TextValue,
                    result.Unit,
                    result.Comment,

                    PhotoCount =
                        result.Photos.Count
                })
                .ToListAsync(
                    cancellationToken);


        return rows
            .Select(x =>
                new AbnormalResultListData(
                    x.ScheduleId,
                    x.InspectionId,
                    x.ResultId,
                    x.ScheduledDate,
                    x.FactorySiteName,
                    x.LocationName,
                    x.EquipmentCode,
                    x.EquipmentName,
                    x.TemplateName,
                    x.OperatorName,
                    x.InspectionStatus,
                    x.DisplayOrder,
                    x.ItemName,
                    x.InputType,
                    x.CheckValue,
                    x.NumericValue,
                    x.TextValue,
                    x.Unit,
                    x.Comment,
                    x.PhotoCount))
            .ToList();
    }

    // ============================================
    // 未実施一覧
    // ============================================

    public async Task<IReadOnlyList<InspectionListData>>
        GetNotStartedAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var rows =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .Where(x =>
                    !x.IsCancelled &&
                    x.Inspection == null)
                .OrderBy(x =>
                    x.ScheduledDate)
                .ThenBy(x =>
                    x.Equipment.EquipmentCode)
                .Select(x => new InspectionListData(
                    x.Id,
                    null,
                    x.ScheduledDate,

                    x.Equipment
                        .Location
                        .FactorySite
                        .Name,

                    x.Equipment
                        .Location
                        .Name,

                    x.Equipment
                        .EquipmentCode,

                    x.Equipment
                        .Name,

                    x.InspectionTemplate
                        .Name,

                    x.AssignedOperator
                        .DisplayName,

                    InspectionStatus.NotStarted,

                    0,
                    0,
                    0))
                .ToListAsync(
                    cancellationToken);

        return rows;
    }

    // ============================================
    // 完了・承認待ち一覧
    // ============================================

    public async Task<IReadOnlyList<InspectionListData>>
        GetApprovalPendingAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var rows =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .Where(x =>
                    !x.IsCancelled &&
                    x.Inspection != null &&
                    x.Inspection.Status ==
                        InspectionStatus.Completed)
                .OrderBy(x =>
                    x.ScheduledDate)
                .ThenBy(x =>
                    x.Equipment.EquipmentCode)
                .Select(x => new InspectionListData(
                    x.Id,
                    x.Inspection!.Id,
                    x.ScheduledDate,

                    x.Equipment
                        .Location
                        .FactorySite
                        .Name,

                    x.Equipment
                        .Location
                        .Name,

                    x.Equipment
                        .EquipmentCode,

                    x.Equipment
                        .Name,

                    x.InspectionTemplate
                        .Name,

                    x.AssignedOperator
                        .DisplayName,

                    x.Inspection.Status,

                    x.Inspection.Results.Count,

                    x.Inspection.Results.Count(
                        result =>
                            result.IsAbnormal),

                    x.Inspection.Photos.Count))
                .ToListAsync(
                    cancellationToken);

        return rows;
    }

    // ============================================
    // 点検承認
    // ============================================

    public async Task ApproveAsync(
        Guid scheduleId,
        Guid operatorId,
        CancellationToken cancellationToken = default)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var inspection =
            await dbContext
                .Set<Inspection>()
                .SingleOrDefaultAsync(
                    x =>
                        x.InspectionScheduleId ==
                            scheduleId,
                    cancellationToken);

        if (inspection is null)
        {
            throw new InvalidOperationException(
                "点検実績が見つかりません。");
        }

        if (inspection.Status !=
            InspectionStatus.Completed)
        {
            throw new InvalidOperationException(
                "承認待ちの点検のみ承認できます。");
        }

        var beforeStatus =
            inspection.Status;

        inspection.Approve(
            DateTime.UtcNow);

        var auditLog =
            new AuditLog(
                operatorId,
                AuditActionType.Approve,
                AuditEntityType.Inspection,
                inspection.Id,
                beforeStatus.ToString(),
                inspection.Status.ToString(),
                null);

        dbContext.AuditLogs.Add(
            auditLog);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    // ============================================
    // 点検差し戻し
    // ============================================
    public async Task ReturnAsync(
        Guid scheduleId,
        string reason,
        Guid operatorId,
        CancellationToken cancellationToken = default)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException(
                "差し戻し理由を入力してください。",
                nameof(reason));
        }

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var inspection =
            await dbContext
                .Set<Inspection>()
                .SingleOrDefaultAsync(
                    x =>
                        x.InspectionScheduleId ==
                            scheduleId,
                    cancellationToken);

        if (inspection is null)
        {
            throw new InvalidOperationException(
                "点検実績が見つかりません。");
        }

        if (inspection.Status !=
            InspectionStatus.Completed)
        {
            throw new InvalidOperationException(
                "承認待ちの点検のみ差し戻しできます。");
        }

        var normalizedReason =
            reason.Trim();

        var beforeStatus =
            inspection.Status;

        inspection.Return(
            normalizedReason,
            DateTime.UtcNow);

        var auditLog =
            new AuditLog(
                operatorId,
                AuditActionType.ReturnForCorrection,
                AuditEntityType.Inspection,
                inspection.Id,
                beforeStatus.ToString(),
                inspection.Status.ToString(),
                normalizedReason);

        dbContext.AuditLogs.Add(
            auditLog);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}