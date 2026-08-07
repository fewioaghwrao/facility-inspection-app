using FacilityInspection.Domain.Equipments;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class EquipmentSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        // 第1工場のコンプレッサー室を取得する
        var compressorRoom =
            await dbContext.Locations
                .Include(x => x.FactorySite)
                .SingleOrDefaultAsync(
                    x =>
                        x.FactorySite.Name == "第1工場" &&
                        x.Name == "コンプレッサー室" &&
                        x.IsActive,
                    cancellationToken);

        if (compressorRoom is null)
        {
            throw new InvalidOperationException(
                "シード対象の設置場所「第1工場 / コンプレッサー室」が見つかりません。");
        }

        await AddEquipmentIfMissingAsync(
            dbContext,
            compressorRoom.Id,
            equipmentCode: "AC-001",
            equipmentName: "エアコンプレッサー1号機",
            equipmentType: EquipmentType.AirCompressor,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-AC-22A",
            serialNumber: "DEMO-AC-0001",
            installedOn: new DateOnly(2021, 4, 1),
            notes: "第1工場コンプレッサー室の主設備",
            cancellationToken);

        await AddEquipmentIfMissingAsync(
            dbContext,
            compressorRoom.Id,
            equipmentCode: "AC-002",
            equipmentName: "エアコンプレッサー2号機",
            equipmentType: EquipmentType.AirCompressor,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-AC-22B",
            serialNumber: "DEMO-AC-0002",
            installedOn: new DateOnly(2022, 7, 15),
            notes: "第1工場コンプレッサー室の予備設備",
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task AddEquipmentIfMissingAsync(
        InspectionDbContext dbContext,
        Guid locationId,
        string equipmentCode,
        string equipmentName,
        EquipmentType equipmentType,
        string? manufacturer,
        string? modelNumber,
        string? serialNumber,
        DateOnly? installedOn,
        string? notes,
        CancellationToken cancellationToken)
    {
        var normalizedEquipmentCode =
            equipmentCode.Trim().ToUpperInvariant();

        var alreadyExists =
            await dbContext.Equipments.AnyAsync(
                x =>
                    x.EquipmentCode ==
                    normalizedEquipmentCode,
                cancellationToken);

        if (alreadyExists)
        {
            return;
        }

        var equipment =
            new Equipment(
                locationId: locationId,
                equipmentCode: normalizedEquipmentCode,
                name: equipmentName,
                equipmentType: equipmentType,
                manufacturer: manufacturer,
                modelNumber: modelNumber,
                serialNumber: serialNumber,
                installedOn: installedOn,
                notes: notes);

        dbContext.Equipments.Add(
            equipment);
    }
}