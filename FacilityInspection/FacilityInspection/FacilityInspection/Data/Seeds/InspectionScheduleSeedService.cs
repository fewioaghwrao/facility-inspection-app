using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.Operators;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

/// <summary>
/// ポートフォリオ表示用の点検予定を登録する。
/// 2026年7月から2027年8月まで、
/// 6設備に対して毎月1件ずつ登録する。
/// </summary>
public sealed class InspectionScheduleSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory)
{
    private static readonly DateOnly StartMonth =
        new(2026, 7, 1);

    private static readonly DateOnly EndMonth =
        new(2027, 8, 1);

    /*
     * 同じ日に予定が集中しないよう、
     * 設備ごとに毎月の実施日を分散する。
     */
    private static readonly EquipmentScheduleDefinition[]
        EquipmentDefinitions =
        [
            new(
                EquipmentCode: "AC-001",
                ScheduledDay: 5),

            new(
                EquipmentCode: "AC-002",
                ScheduledDay: 10),

            new(
                EquipmentCode: "WP-001",
                ScheduledDay: 15),

            new(
                EquipmentCode: "WP-002",
                ScheduledDay: 20),

            new(
                EquipmentCode: "VE-001",
                ScheduledDay: 25),

            new(
                EquipmentCode: "VE-002",
                ScheduledDay: 28)
        ];

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        /*
         * 1. 対象設備を取得
         */

        var equipmentCodes =
            EquipmentDefinitions
                .Select(x => x.EquipmentCode)
                .ToArray();

        var equipments =
            await dbContext.Equipments
                .AsNoTracking()
                .Where(x =>
                    equipmentCodes.Contains(
                        x.EquipmentCode))
                .ToListAsync(cancellationToken);

        ValidateEquipments(equipments);

        var equipmentByCode =
            equipments.ToDictionary(
                x => x.EquipmentCode,
                StringComparer.OrdinalIgnoreCase);

        /*
         * 2. 点検担当者を5人取得
         */

        var inspectors =
            await dbContext.Operators
                .AsNoTracking()
                .Where(x =>
                    x.Role == OperatorRole.Inspector &&
                    x.IsActive)
                .OrderBy(x => x.DisplayName)
                .ThenBy(x => x.LoginId)
                .Take(5)
                .ToListAsync(cancellationToken);

        if (inspectors.Count < 5)
        {
            throw new InvalidOperationException(
                "点検予定シードの作成には、" +
                "有効な点検担当者が5人必要です。" +
                $"現在の登録数は{inspectors.Count}人です。");
        }

        /*
         * 3. 設備種別ごとの有効な最新テンプレートを取得
         */

        var requiredEquipmentTypes =
            equipments
                .Select(x => x.EquipmentType)
                .Distinct()
                .ToArray();

        var templateCandidates =
            await dbContext.InspectionTemplates
                .AsNoTracking()
                .Where(x =>
                    x.IsActive &&
                    requiredEquipmentTypes.Contains(
                        x.EquipmentType))
                .ToListAsync(cancellationToken);

        var templateByEquipmentType =
            templateCandidates
                .GroupBy(x => x.EquipmentType)
                .ToDictionary(
                    x => x.Key,
                    x => x
                        .OrderByDescending(y => y.Version)
                        .ThenByDescending(y => y.CreatedAt)
                        .First());

        ValidateTemplates(
            requiredEquipmentTypes,
            templateByEquipmentType);

        /*
         * 4. 既存予定を取得
         *
         * 設備IDと予定日が同じ予定は追加しない。
         */

        var rangeStart =
            StartMonth;

        var rangeEnd =
            new DateOnly(
                EndMonth.Year,
                EndMonth.Month,
                DateTime.DaysInMonth(
                    EndMonth.Year,
                    EndMonth.Month));

        var existingScheduleRows =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .Where(x =>
                    x.ScheduledDate >= rangeStart &&
                    x.ScheduledDate <= rangeEnd)
                .Select(x => new
                {
                    x.EquipmentId,
                    x.ScheduledDate
                })
                .ToListAsync(cancellationToken);

        var existingScheduleKeys =
            existingScheduleRows
                .Select(x =>
                    new ScheduleKey(
                        x.EquipmentId,
                        x.ScheduledDate))
                .ToHashSet();

        /*
         * 5. 2026年7月～2027年8月の予定を作成
         */

        var currentMonth =
            StartMonth;

        var monthIndex = 0;
        var addedCount = 0;

        while (currentMonth <= EndMonth)
        {
            for (var equipmentIndex = 0;
                 equipmentIndex <
                 EquipmentDefinitions.Length;
                 equipmentIndex++)
            {
                var definition =
                    EquipmentDefinitions[
                        equipmentIndex];

                var equipment =
                    equipmentByCode[
                        definition.EquipmentCode];

                var scheduledDate =
                    CreateScheduledDate(
                        currentMonth,
                        definition.ScheduledDay);

                var scheduleKey =
                    new ScheduleKey(
                        equipment.Id,
                        scheduledDate);

                /*
                 * 再起動時に同じ予定を重複登録しない。
                 */
                if (existingScheduleKeys.Contains(
                        scheduleKey))
                {
                    continue;
                }

                /*
                 * 5人の担当者を順番に割り当てる。
                 *
                 * 月をまたいでも割当順が循環するため、
                 * 特定担当者への偏りを抑えられる。
                 */
                var operatorIndex =
                    (
                        monthIndex *
                        EquipmentDefinitions.Length +
                        equipmentIndex
                    ) % inspectors.Count;

                var assignedOperator =
                    inspectors[operatorIndex];

                var inspectionTemplate =
                    templateByEquipmentType[
                        equipment.EquipmentType];

                var schedule =
                    new InspectionSchedule(
                        scheduledDate:
                            scheduledDate,
                        equipmentId:
                            equipment.Id,
                        inspectionTemplateId:
                            inspectionTemplate.Id,
                        assignedOperatorId:
                            assignedOperator.Id,
                        notes:
                            CreateNotes(
                                scheduledDate,
                                equipment));

                dbContext.InspectionSchedules.Add(
                    schedule);

                existingScheduleKeys.Add(
                    scheduleKey);

                addedCount++;
            }

            currentMonth =
                currentMonth.AddMonths(1);

            monthIndex++;
        }

        if (addedCount == 0)
        {
            return;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    /// <summary>
    /// 指定月と実施日から予定日を生成する。
    /// 月末を超える日が指定された場合は月末へ補正する。
    /// </summary>
    private static DateOnly CreateScheduledDate(
        DateOnly targetMonth,
        int scheduledDay)
    {
        var lastDay =
            DateTime.DaysInMonth(
                targetMonth.Year,
                targetMonth.Month);

        var actualDay =
            Math.Min(
                scheduledDay,
                lastDay);

        return new DateOnly(
            targetMonth.Year,
            targetMonth.Month,
            actualDay);
    }

    private static string CreateNotes(
        DateOnly scheduledDate,
        Equipment equipment)
    {
        return
            $"{scheduledDate.Year}年" +
            $"{scheduledDate.Month}月 " +
            $"{equipment.Name}の月次定期点検" +
            "（ポートフォリオ用デモデータ）";
    }

    private static void ValidateEquipments(
        IReadOnlyCollection<Equipment> equipments)
    {
        var registeredCodes =
            equipments
                .Select(x => x.EquipmentCode)
                .ToHashSet(
                    StringComparer.OrdinalIgnoreCase);

        var missingCodes =
            EquipmentDefinitions
                .Select(x => x.EquipmentCode)
                .Where(x =>
                    !registeredCodes.Contains(x))
                .ToArray();

        if (missingCodes.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "点検予定シードに必要な設備が" +
            "登録されていません: " +
            string.Join(
                ", ",
                missingCodes));
    }

    private static void ValidateTemplates(
        IReadOnlyCollection<EquipmentType>
            requiredEquipmentTypes,
        IReadOnlyDictionary<
            EquipmentType,
            Domain.InspectionTemplates
                .InspectionTemplate>
            templateByEquipmentType)
    {
        var missingTypes =
            requiredEquipmentTypes
                .Where(x =>
                    !templateByEquipmentType
                        .ContainsKey(x))
                .ToArray();

        if (missingTypes.Length == 0)
        {
            return;
        }

        throw new InvalidOperationException(
            "点検予定シードに必要な有効テンプレートが" +
            "登録されていません: " +
            string.Join(
                ", ",
                missingTypes));
    }

    private sealed record
        EquipmentScheduleDefinition(
            string EquipmentCode,
            int ScheduledDay);

    private readonly record struct ScheduleKey(
        Guid EquipmentId,
        DateOnly ScheduledDate);
}