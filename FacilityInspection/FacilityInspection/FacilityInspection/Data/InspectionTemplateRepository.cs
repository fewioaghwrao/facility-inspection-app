using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed record InspectionTemplateItemCreateData(
    int DisplayOrder,
    string ItemName,
    InspectionInputType InputType,
    string? Unit,
    double? MinimumValue,
    double? MaximumValue,
    bool IsRequired,
    bool IsActive);

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

    public async Task<Guid> CreateAsync(
        EquipmentType equipmentType,
        string name,
        IReadOnlyList<InspectionTemplateItemCreateData> items,
        CancellationToken cancellationToken = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentNullException.ThrowIfNull(items);

        if (items.Count == 0)
        {
            throw new InvalidOperationException(
                "点検項目を1件以上登録してください。");
        }

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var currentMaxVersion =
            await dbContext.InspectionTemplates
                .Where(x => x.EquipmentType == equipmentType)
                .Select(x => (int?)x.Version)
                .MaxAsync(cancellationToken)
            ?? 0;

        var template = new InspectionTemplate
        {
            Name = name.Trim(),
            EquipmentType = equipmentType,
            Version = currentMaxVersion + 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        foreach (var createItem in
                 items.OrderBy(x => x.DisplayOrder))
        {
            template.Items.Add(
                new InspectionTemplateItem
                {
                    DisplayOrder =
                        createItem.DisplayOrder,

                    ItemName =
                        createItem.ItemName.Trim(),

                    InputType =
                        createItem.InputType,

                    Unit =
                        NormalizeText(createItem.Unit),

                    MinimumValue =
                        createItem.MinimumValue,

                    MaximumValue =
                        createItem.MaximumValue,

                    IsRequired =
                        createItem.IsRequired,

                    IsActive =
                        createItem.IsActive
                });
        }

        dbContext.InspectionTemplates.Add(template);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return template.Id;
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
                NormalizeText(updateItem.Unit);

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

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}