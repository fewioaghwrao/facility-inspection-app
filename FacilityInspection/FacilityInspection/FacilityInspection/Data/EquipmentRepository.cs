using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Sites;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using DomainEquipment =
    FacilityInspection.Domain.Equipments.Equipment;

namespace FacilityInspection.Data;

public sealed class EquipmentRepository
{
    public EquipmentRepository(string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(databasePath);
        DatabasePath = databasePath;
    }

    public string DatabasePath { get; }

    public async Task InitializeAsync()
    {
        var directoryPath =
            Path.GetDirectoryName(DatabasePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException(
                "SQLiteデータベースの保存先を取得できませんでした。");
        }

        Directory.CreateDirectory(directoryPath);

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        // FactorySiteConfiguration・LocationConfigurationの
        // HasDataも初回DB作成時に反映される
        await dbContext.Database.EnsureCreatedAsync();
    }

    /// <summary>
    /// 有効な工場を取得する。
    /// </summary>
    public async Task<IReadOnlyList<FactorySite>>
        GetFactorySitesAsync()
    {
        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        return await dbContext.FactorySites
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .ToListAsync();
    }

    /// <summary>
    /// 指定した工場に所属する有効な設置場所を取得する。
    /// </summary>
    public async Task<IReadOnlyList<Location>>
        GetLocationsByFactorySiteIdAsync(
            Guid factorySiteId)
    {
        if (factorySiteId == Guid.Empty)
        {
            throw new ArgumentException(
                "工場IDを指定してください。",
                nameof(factorySiteId));
        }

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        return await dbContext.Locations
            .AsNoTracking()
            .Where(x =>
                x.FactorySiteId == factorySiteId &&
                x.IsActive)
            .OrderBy(x => x.Floor)
            .ThenBy(x => x.Code)
            .ToListAsync();
    }

    /// <summary>
    /// 初期表示などで使用する既定の設置場所IDを取得する。
    /// ComboBox対応完了後は削除可能。
    /// </summary>
    public async Task<Guid> GetDefaultLocationIdAsync()
    {
        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        var locationId = await dbContext.Locations
            .AsNoTracking()
            .Where(x => x.IsActive)
            .OrderBy(x => x.Code)
            .Select(x => x.Id)
            .FirstOrDefaultAsync();

        if (locationId == Guid.Empty)
        {
            throw new InvalidOperationException(
                "有効な設置場所が登録されていません。");
        }

        return locationId;
    }

    public async Task<IReadOnlyList<DomainEquipment>> GetAllAsync()
    {
        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        return await dbContext.Equipments
            .AsNoTracking()
            .Include(x => x.Location)
            .ThenInclude(x => x.FactorySite)
            .OrderBy(x => x.EquipmentCode)
            .ToListAsync();
    }

    public async Task AddAsync(
        Guid locationId,
        string equipmentCode,
        string equipmentName,
        EquipmentType equipmentType)
    {
        if (locationId == Guid.Empty)
        {
            throw new ArgumentException(
                "設置場所IDを指定してください。",
                nameof(locationId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            equipmentCode);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            equipmentName);

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        var locationExists = await dbContext.Locations
            .AnyAsync(x =>
                x.Id == locationId &&
                x.IsActive);

        if (!locationExists)
        {
            throw new InvalidOperationException(
                "指定された設置場所が存在しないか、無効になっています。");
        }

        var normalizedCode =
            equipmentCode.Trim().ToUpperInvariant();

        var duplicateExists = await dbContext.Equipments
            .AnyAsync(x =>
                x.EquipmentCode == normalizedCode);

        if (duplicateExists)
        {
            throw new InvalidOperationException(
                $"設備コード「{normalizedCode}」は既に登録されています。");
        }

        var equipment = new DomainEquipment(
            locationId: locationId,
            equipmentCode: normalizedCode,
            name: equipmentName.Trim(),
            equipmentType: equipmentType);

        dbContext.Equipments.Add(equipment);

        await dbContext.SaveChangesAsync();
    }
}