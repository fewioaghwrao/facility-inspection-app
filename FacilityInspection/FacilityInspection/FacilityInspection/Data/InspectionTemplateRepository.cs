using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed record InspectionTemplateItemUpdateData(
    Guid Id,
    int DisplayOrder,
    string ItemName,
    InspectionInputType InputType,
    string? Unit,
    double? MinimumValue,
    double? MaximumValue,
    bool IsRequired,
    bool IsActive);

public sealed class InspectionTemplateRepository
{
    private readonly InspectionDbContextFactory _dbContextFactory;

    public InspectionTemplateRepository(
        InspectionDbContextFactory dbContextFactory)
    {
        ArgumentNullException.ThrowIfNull(dbContextFactory);

        _dbContextFactory = dbContextFactory;
    }

    public async Task<IReadOnlyList<InspectionTemplate>> GetAllAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        return await dbContext.InspectionTemplates
            .AsNoTracking()
            .Include(x => x.Items)
            .OrderBy(x => x.EquipmentType)
            .ThenByDescending(x => x.Version)
            .ToListAsync(cancellationToken);
    }

    public async Task UpdateAsync(
        Guid templateId,
        string name,
        IReadOnlyList<InspectionTemplateItemUpdateData> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(items);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var template =
            await dbContext.InspectionTemplates
                .Include(x => x.Items)
                .SingleOrDefaultAsync(
                    x => x.Id == templateId,
                    cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException(
                "編集対象の点検票テンプレートが見つかりません。");
        }

        template.Name = name.Trim();
        template.UpdatedAt = DateTime.UtcNow;

        var existingItems =
            template.Items.ToDictionary(x => x.Id);

        foreach (var updateItem in items)
        {
            if (!existingItems.TryGetValue(
                    updateItem.Id,
                    out var entity))
            {
                throw new InvalidOperationException(
                    $"点検項目が見つかりません: {updateItem.ItemName}");
            }

            entity.DisplayOrder =
                updateItem.DisplayOrder;

            entity.ItemName =
                updateItem.ItemName.Trim();

            entity.InputType =
                updateItem.InputType;

            entity.Unit =
                string.IsNullOrWhiteSpace(updateItem.Unit)
                    ? null
                    : updateItem.Unit.Trim();

            entity.MinimumValue =
                updateItem.MinimumValue;

            entity.MaximumValue =
                updateItem.MaximumValue;

            entity.IsRequired =
                updateItem.IsRequired;

            entity.IsActive =
                updateItem.IsActive;
        }

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    public async Task SetActiveAsync(
        Guid templateId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var template =
            await dbContext.InspectionTemplates
                .SingleOrDefaultAsync(
                    x => x.Id == templateId,
                    cancellationToken);

        if (template is null)
        {
            throw new InvalidOperationException(
                "対象の点検票テンプレートが見つかりません。");
        }

        template.IsActive = isActive;
        template.UpdatedAt = DateTime.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }
}