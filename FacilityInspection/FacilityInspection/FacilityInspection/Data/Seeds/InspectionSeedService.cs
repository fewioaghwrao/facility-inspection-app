using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class InspectionSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var today =
            DateOnly.FromDateTime(DateTime.Today);

        var targetEquipmentCodes = new[]
        {
        "AC-001",
        "WP-001",
        "VE-001"
    };

        var schedules =
            await dbContext.InspectionSchedules
                .Include(x => x.Equipment)
                .Include(x => x.InspectionTemplate)
                    .ThenInclude(x => x.Items)
                .Include(x => x.Inspection)
                    .ThenInclude(x => x.Results)
                .Include(x => x.Inspection)
                    .ThenInclude(x => x.Photos)
                .Where(x =>
                    !x.IsCancelled &&
                    x.ScheduledDate < today &&
                    targetEquipmentCodes.Contains(
                        x.Equipment.EquipmentCode))
                .OrderByDescending(x => x.ScheduledDate)
                .ToListAsync(cancellationToken);

        SeedSchedule(
            dbContext,
            GetRequiredSchedule(
                schedules,
                "AC-001"),
            SeedInspectionState.Approved);

        SeedSchedule(
            dbContext,
            GetRequiredSchedule(
                schedules,
                "WP-001"),
            SeedInspectionState.Completed);

        SeedSchedule(
            dbContext,
            GetRequiredSchedule(
                schedules,
                "VE-001"),
            SeedInspectionState.Returned);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static InspectionSchedule GetRequiredSchedule(
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

    private static void AddResults(
        Inspection inspection,
        IEnumerable<InspectionTemplateItem> templateItems,
        bool hasAbnormalResult)
    {
        var activeItems =
            templateItems
                .Where(x => x.IsActive)
                .OrderBy(x => x.DisplayOrder)
                .ToList();

        for (var index = 0;
             index < activeItems.Count;
             index++)
        {
            var item = activeItems[index];

            // 差し戻しデータだけ先頭項目を異常とする
            var isAbnormal =
                hasAbnormalResult &&
                index == 0;

            var result =
                new InspectionResult(
                    inspectionId: inspection.Id,
                    inspectionTemplateItemId: item.Id,
                    displayOrder: item.DisplayOrder,
                    itemName: item.ItemName,
                    inputType: item.InputType,
                    unit: item.Unit);

            var values =
                CreateResultValues(
                    item,
                    isAbnormal);

            result.UpdateResult(
                checkValue: values.CheckValue,
                numericValue: values.NumericValue,
                textValue: values.TextValue,
                isAbnormal: isAbnormal,
                comment: isAbnormal
                    ? "異常を確認しました。再点検が必要です。"
                    : "異常ありません。");

            inspection.Results.Add(result);
        }
    }

    private static ResultSeedValues CreateResultValues(
        InspectionTemplateItem item,
        bool isAbnormal)
    {
        return item.InputType switch
        {
            InspectionInputType.NormalAbnormal =>
                new ResultSeedValues(
                    CheckValue: !isAbnormal,
                    NumericValue: null,
                    TextValue: null),

            InspectionInputType.DoneNotDone =>
                new ResultSeedValues(
                    CheckValue: !isAbnormal,
                    NumericValue: null,
                    TextValue: null),

            InspectionInputType.Numeric =>
                new ResultSeedValues(
                    CheckValue: null,
                    NumericValue:
                        CreateNumericValue(
                            item,
                            isAbnormal),
                    TextValue: null),

            InspectionInputType.Text =>
                new ResultSeedValues(
                    CheckValue: null,
                    NumericValue: null,
                    TextValue: isAbnormal
                        ? "異常箇所を確認"
                        : "良好"),

            _ =>
                new ResultSeedValues(
                    CheckValue: null,
                    NumericValue: null,
                    TextValue: isAbnormal
                        ? "異常あり"
                        : "異常なし")
        };
    }

    private static decimal CreateNumericValue(
        InspectionTemplateItem item,
        bool isAbnormal)
    {
        if (isAbnormal)
        {
            if (item.MaximumValue.HasValue)
            {
                var maximum =
                    item.MaximumValue.Value;

                var additionalValue =
                    Math.Max(
                        Math.Abs(maximum) * 0.1,
                        0.1);

                return Convert.ToDecimal(
                    maximum + additionalValue);
            }

            if (item.MinimumValue.HasValue)
            {
                var minimum =
                    item.MinimumValue.Value;

                var deductionValue =
                    Math.Max(
                        Math.Abs(minimum) * 0.1,
                        0.1);

                return Convert.ToDecimal(
                    minimum - deductionValue);
            }

            return 999m;
        }

        if (item.MinimumValue.HasValue &&
            item.MaximumValue.HasValue)
        {
            return Convert.ToDecimal(
                (item.MinimumValue.Value +
                 item.MaximumValue.Value) / 2);
        }

        if (item.MinimumValue.HasValue)
        {
            return Convert.ToDecimal(
                item.MinimumValue.Value);
        }

        if (item.MaximumValue.HasValue)
        {
            return Convert.ToDecimal(
                item.MaximumValue.Value);
        }

        return 1m;
    }

    private static void AddPhotos(
        Inspection inspection,
        EquipmentType equipmentType,
        DateTime startedAtUtc)
    {
        var photoData =
            GetPhotoData(equipmentType);

        var firstResult =
            inspection.Results
                .OrderBy(x => x.DisplayOrder)
                .FirstOrDefault();

        var overallPhoto =
            new InspectionPhoto(
                inspectionId: inspection.Id,
                relativePath: photoData.FirstPath,
                capturedAtUtc:
                    startedAtUtc.AddMinutes(20),
                displayOrder: 1,
                caption: photoData.FirstCaption);

        var detailPhoto =
            new InspectionPhoto(
                inspectionId: inspection.Id,
                relativePath: photoData.SecondPath,
                capturedAtUtc:
                    startedAtUtc.AddMinutes(30),
                displayOrder: 2,
                inspectionResultId:
                    firstResult?.Id,
                caption: photoData.SecondCaption);

        inspection.Photos.Add(overallPhoto);
        inspection.Photos.Add(detailPhoto);
    }

    private static PhotoSeedData GetPhotoData(
        EquipmentType equipmentType)
    {
        return equipmentType switch
        {
            EquipmentType.AirCompressor =>
                new PhotoSeedData(
                    FirstPath:
                        "sample/images/resultcomp1.jpg",
                    SecondPath:
                        "sample/images/resultcomp2.jpg",
                    FirstCaption:
                        "コンプレッサー外観",
                    SecondCaption:
                        "コンプレッサー点検箇所"),

            EquipmentType.CoolingWaterPump =>
                new PhotoSeedData(
                    FirstPath:
                        "sample/images/resultponp1.jpg",
                    SecondPath:
                        "sample/images/resultponp2.jpg",
                    FirstCaption:
                        "冷却水ポンプ外観",
                    SecondCaption:
                        "冷却水ポンプ点検箇所"),

            EquipmentType.Ventilation =>
                new PhotoSeedData(
                    FirstPath:
                        "sample/images/resultkanki1.jpg",
                    SecondPath:
                        "sample/images/resultkanki2.jpg",
                    FirstCaption:
                        "換気設備外観",
                    SecondCaption:
                        "再確認が必要な点検箇所"),

            _ =>
                throw new InvalidOperationException(
                    $"{equipmentType}用の結果写真が" +
                    "設定されていません。")
        };
    }

    private static void SeedSchedule(
    InspectionDbContext dbContext,
    InspectionSchedule schedule,
    SeedInspectionState state)
    {
        var inspection =
            schedule.Inspection;

        // すでに完了・承認・差し戻し済みなら変更しない
        if (inspection is not null &&
            inspection.Status !=
                InspectionStatus.NotStarted)
        {
            return;
        }

        if (inspection is null)
        {
            inspection =
                new Inspection(schedule.Id);

            schedule.AttachInspection(
                inspection);

            dbContext.Inspections.Add(
                inspection);
        }

        var startedAtUtc =
            DateTime.SpecifyKind(
                schedule.ScheduledDate.ToDateTime(
                    new TimeOnly(9, 0)),
                DateTimeKind.Utc);

        var completedAtUtc =
            startedAtUtc.AddHours(1);

        var reviewedAtUtc =
            completedAtUtc.AddHours(1);

        inspection.Start(
            schedule.AssignedOperatorId,
            startedAtUtc);

        var hasAbnormalResult =
            state ==
            SeedInspectionState.Returned;

        if (inspection.Results.Count == 0)
        {
            AddResults(
                inspection,
                schedule.InspectionTemplate.Items,
                hasAbnormalResult);
        }

        if (inspection.Photos.Count == 0)
        {
            AddPhotos(
                inspection,
                schedule.InspectionTemplate
                    .EquipmentType,
                startedAtUtc);
        }

        inspection.Complete(
            completedAtUtc);

        switch (state)
        {
            case SeedInspectionState.Completed:
                // 完了・承認待ちのまま
                break;

            case SeedInspectionState.Approved:
                inspection.Approve(
                    reviewedAtUtc);
                break;

            case SeedInspectionState.Returned:
                inspection.Return(
                    "異常箇所の状況を再確認し、" +
                    "追加写真を登録してください。",
                    reviewedAtUtc);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(state),
                    state,
                    "未対応の点検状態です。");
        }
    }

    private enum SeedInspectionState
    {
        Completed,
        Approved,
        Returned
    }

    private sealed record ResultSeedValues(
        bool? CheckValue,
        decimal? NumericValue,
        string? TextValue);

    private sealed record PhotoSeedData(
        string FirstPath,
        string SecondPath,
        string FirstCaption,
        string SecondCaption);
}
