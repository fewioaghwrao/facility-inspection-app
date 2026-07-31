using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

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
        var directoryPath = Path.GetDirectoryName(DatabasePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            throw new InvalidOperationException(
                "SQLiteデータベースの保存先を取得できませんでした。");
        }

        Directory.CreateDirectory(directoryPath);

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        await dbContext.Database.EnsureCreatedAsync();
    }

    public async Task<IReadOnlyList<Equipment>> GetAllAsync()
    {
        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        return await dbContext.Equipments
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync();
    }

    public async Task AddAsync(string equipmentName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(equipmentName);

        await using var dbContext =
            new InspectionDbContext(DatabasePath);

        var equipment = new Equipment
        {
            Name = equipmentName.Trim(),
            CreatedAtUtc = DateTime.UtcNow
        };

        dbContext.Equipments.Add(equipment);

        await dbContext.SaveChangesAsync();
    }
}