using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed record InspectionListData(
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

public sealed class InspectionRepository
{
    private readonly InspectionDbContextFactory
        _dbContextFactory;

    public InspectionRepository(
        InspectionDbContextFactory dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(
            dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

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
                        x.Equipment.Location.Name,

                    EquipmentCode =
                        x.Equipment.EquipmentCode,

                    EquipmentName =
                        x.Equipment.Name,

                    TemplateName =
                        x.InspectionTemplate.Name,

                    OperatorName =
                        x.AssignedOperator.DisplayName,

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
}