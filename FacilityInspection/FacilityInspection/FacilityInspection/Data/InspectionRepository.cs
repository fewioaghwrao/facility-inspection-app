using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

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
}