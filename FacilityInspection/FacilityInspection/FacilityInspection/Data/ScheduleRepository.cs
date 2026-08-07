using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed class ScheduleRepository
{
    private readonly InspectionDbContextFactory
        _dbContextFactory;

    public ScheduleRepository(
        InspectionDbContextFactory dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(
            dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<InspectionSchedule>>
        GetMonthAsync(
            DateOnly displayedMonth,
            CancellationToken cancellationToken = default)
    {
        var monthStart =
            new DateOnly(
                displayedMonth.Year,
                displayedMonth.Month,
                1);

        var nextMonth =
            monthStart.AddMonths(1);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.InspectionSchedules
            .AsNoTracking()
            .Include(x => x.Equipment)
                .ThenInclude(x => x.Location)
                    .ThenInclude(x => x.FactorySite)
            .Include(x => x.InspectionTemplate)
            .Include(x => x.AssignedOperator)
            .Include(x => x.Inspection)
            .Where(x =>
                x.ScheduledDate >= monthStart &&
                x.ScheduledDate < nextMonth)
            .OrderBy(x => x.ScheduledDate)
            .ThenBy(x => x.Equipment.EquipmentCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<FactorySite>>
        GetFactorySitesAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.FactorySites
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Location>>
        GetLocationsAsync(
            Guid factorySiteId,
            CancellationToken cancellationToken = default)
    {
        if (factorySiteId == Guid.Empty)
        {
            return [];
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.Locations
            .AsNoTracking()
            .Where(x =>
                x.FactorySiteId == factorySiteId &&
                x.IsActive)
            .OrderBy(x => x.Floor)
            .ThenBy(x => x.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Equipment>>
        GetEquipmentsAsync(
            Guid locationId,
            CancellationToken cancellationToken = default)
    {
        if (locationId == Guid.Empty)
        {
            return [];
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.Equipments
            .AsNoTracking()
            .Where(x =>
                x.LocationId == locationId &&
                x.Status == EquipmentStatus.InService)
            .OrderBy(x => x.EquipmentCode)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<InspectionTemplate>>
        GetTemplatesAsync(
            EquipmentType equipmentType,
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.InspectionTemplates
            .AsNoTracking()
            .Where(x =>
                x.EquipmentType == equipmentType &&
                x.IsActive)
            .OrderByDescending(x => x.Version)
            .ThenBy(x => x.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Operator>>
        GetInspectorsAsync(
            CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.Operators
            .AsNoTracking()
            .Where(x =>
                x.IsActive &&
                x.Role == OperatorRole.Inspector)
            .OrderBy(x => x.DisplayName)
            .ThenBy(x => x.LoginId)
            .ToListAsync(cancellationToken);
    }

    public async Task<Guid> CreateAsync(
        DateOnly scheduledDate,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        ValidateScheduledDate(scheduledDate);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        await ValidateReferencesAsync(
            dbContext,
            equipmentId,
            inspectionTemplateId,
            assignedOperatorId,
            cancellationToken);

        await EnsureScheduleIsNotDuplicatedAsync(
            dbContext,
            null,
            equipmentId,
            scheduledDate,
            cancellationToken);

        var schedule =
            new InspectionSchedule(
                scheduledDate,
                equipmentId,
                inspectionTemplateId,
                assignedOperatorId,
                notes);

        var inspection =
            new Inspection(schedule.Id);

        schedule.AttachInspection(inspection);

        dbContext.InspectionSchedules.Add(schedule);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return schedule.Id;
    }

    public async Task UpdateAsync(
        Guid scheduleId,
        DateOnly scheduledDate,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        string? notes,
        CancellationToken cancellationToken = default)
    {
        ValidateScheduledDate(scheduledDate);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var schedule =
            await dbContext.InspectionSchedules
                .Include(x => x.Inspection)
                .SingleOrDefaultAsync(
                    x => x.Id == scheduleId,
                    cancellationToken);

        if (schedule is null)
        {
            throw new InvalidOperationException(
                "編集対象の点検予定が見つかりません。");
        }

        if (schedule.IsCancelled)
        {
            throw new InvalidOperationException(
                "取消済みの点検予定は編集できません。");
        }

        var status =
            schedule.Inspection?.Status ??
            InspectionStatus.NotStarted;

        if (status != InspectionStatus.NotStarted)
        {
            throw new InvalidOperationException(
                "点検開始後の予定は編集できません。");
        }

        await ValidateReferencesAsync(
            dbContext,
            equipmentId,
            inspectionTemplateId,
            assignedOperatorId,
            cancellationToken);

        await EnsureScheduleIsNotDuplicatedAsync(
            dbContext,
            scheduleId,
            equipmentId,
            scheduledDate,
            cancellationToken);

        schedule.Update(
            scheduledDate,
            equipmentId,
            inspectionTemplateId,
            assignedOperatorId,
            notes);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task CancelAsync(
        Guid scheduleId,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var schedule =
            await dbContext.InspectionSchedules
                .Include(x => x.Inspection)
                .SingleOrDefaultAsync(
                    x => x.Id == scheduleId,
                    cancellationToken);

        if (schedule is null)
        {
            throw new InvalidOperationException(
                "取消対象の点検予定が見つかりません。");
        }

        var status =
            schedule.Inspection?.Status ??
            InspectionStatus.NotStarted;

        if (status != InspectionStatus.NotStarted)
        {
            throw new InvalidOperationException(
                "点検開始後の予定は取り消せません。");
        }

        schedule.Cancel();

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task ValidateReferencesAsync(
        InspectionDbContext dbContext,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        CancellationToken cancellationToken)
    {
        var equipment =
            await dbContext.Equipments
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == equipmentId,
                    cancellationToken);

        if (equipment is null ||
            equipment.Status != EquipmentStatus.InService)
        {
            throw new InvalidOperationException(
                "選択した設備が存在しないか、現在使用できません。");
        }

        var template =
            await dbContext.InspectionTemplates
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == inspectionTemplateId,
                    cancellationToken);

        if (template is null ||
            !template.IsActive)
        {
            throw new InvalidOperationException(
                "選択した点検票テンプレートが存在しないか、無効です。");
        }

        if (template.EquipmentType !=
            equipment.EquipmentType)
        {
            throw new InvalidOperationException(
                "設備種別と点検票テンプレートの種別が一致しません。");
        }

        var assignedOperator =
            await dbContext.Operators
                .AsNoTracking()
                .SingleOrDefaultAsync(
                    x => x.Id == assignedOperatorId,
                    cancellationToken);

        if (assignedOperator is null ||
            !assignedOperator.IsActive ||
            assignedOperator.Role !=
                OperatorRole.Inspector)
        {
            throw new InvalidOperationException(
                "選択した点検担当者が存在しないか、現在利用できません。");
        }
    }

    private static async Task
        EnsureScheduleIsNotDuplicatedAsync(
            InspectionDbContext dbContext,
            Guid? excludedScheduleId,
            Guid equipmentId,
            DateOnly scheduledDate,
            CancellationToken cancellationToken)
    {
        var duplicated =
            await dbContext.InspectionSchedules
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        (!excludedScheduleId.HasValue ||
                         x.Id != excludedScheduleId.Value) &&
                        !x.IsCancelled &&
                        x.EquipmentId == equipmentId &&
                        x.ScheduledDate == scheduledDate,
                    cancellationToken);

        if (duplicated)
        {
            throw new InvalidOperationException(
                "同じ設備・同じ日付の点検予定がすでに登録されています。");
        }
    }

    private static void ValidateScheduledDate(
        DateOnly scheduledDate)
    {
        if (scheduledDate == default)
        {
            throw new ArgumentException(
                "点検予定日を指定してください。",
                nameof(scheduledDate));
        }

        var today =
            DateOnly.FromDateTime(DateTime.Today);

        if (scheduledDate < today)
        {
            throw new InvalidOperationException(
                "過去の日付には新しい点検予定を登録できません。");
        }
    }
}
