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

        await dbContext.Database.EnsureCreatedAsync();

        await SeedInitialDataAsync(dbContext);
    }

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

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        var locationExists = await dbContext.Locations
            .AnyAsync(x =>
                x.Id == locationId &&
                x.IsActive);

        if (!locationExists)
        {
            throw new InvalidOperationException(
                "指定された設置場所が存在しません。");
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
            name: equipmentName,
            equipmentType: equipmentType);

        dbContext.Equipments.Add(equipment);

        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedInitialDataAsync(
        InspectionDbContext dbContext)
    {
        if (await dbContext.FactorySites.AnyAsync())
        {
            return;
        }

        var factorySite = new FactorySite(
            code: "SITE-01",
            name: "第1工場",
            description: "設備点検アプリの初期データ");

        dbContext.FactorySites.Add(factorySite);

        await dbContext.SaveChangesAsync();

        var location = new Location(
            factorySiteId: factorySite.Id,
            code: "LOC-01",
            name: "コンプレッサー室",
            floor: "1F",
            description: "初期設備登録用の設置場所");

        dbContext.Locations.Add(location);

        await dbContext.SaveChangesAsync();
    }
}