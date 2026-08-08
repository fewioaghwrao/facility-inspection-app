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
    private const string FirstFactorySiteName =
        "第1工場";

    private const string SecondFactorySiteName =
        "第2工場";

    private const string CompressorRoomName =
        "コンプレッサー室";

    private const string PumpRoomName =
        "ポンプ室";

    private const string VentilationRoomName =
        "換気設備室";

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        /*
         * 第1工場の設置場所
         */

        var firstFactoryCompressorRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                FirstFactorySiteName,
                CompressorRoomName,
                cancellationToken);

        var firstFactoryPumpRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                FirstFactorySiteName,
                PumpRoomName,
                cancellationToken);

        var firstFactoryVentilationRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                FirstFactorySiteName,
                VentilationRoomName,
                cancellationToken);

        /*
         * 第2工場の設置場所
         */

        var secondFactoryCompressorRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                SecondFactorySiteName,
                CompressorRoomName,
                cancellationToken);

        var secondFactoryPumpRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                SecondFactorySiteName,
                PumpRoomName,
                cancellationToken);

        var secondFactoryVentilationRoomId =
            await GetRequiredLocationIdAsync(
                dbContext,
                SecondFactorySiteName,
                VentilationRoomName,
                cancellationToken);

        /*
         * エアコンプレッサー
         */

        await AddOrUpdateEquipmentAsync(
            dbContext,
            firstFactoryCompressorRoomId,
            equipmentCode: "AC-001",
            equipmentName: "エアコンプレッサー1号機",
            equipmentType: EquipmentType.AirCompressor,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-AC-22A",
            serialNumber: "DEMO-AC-0001",
            installedOn: new DateOnly(2021, 4, 1),
            notes: "第1工場コンプレッサー室の主設備",
            cancellationToken: cancellationToken);

        await AddOrUpdateEquipmentAsync(
            dbContext,
            secondFactoryCompressorRoomId,
            equipmentCode: "AC-002",
            equipmentName: "エアコンプレッサー2号機",
            equipmentType: EquipmentType.AirCompressor,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-AC-22B",
            serialNumber: "DEMO-AC-0002",
            installedOn: new DateOnly(2022, 7, 15),
            notes: "第2工場コンプレッサー室の主設備",
            cancellationToken: cancellationToken);

        /*
         * 冷却水ポンプ
         */

        await AddOrUpdateEquipmentAsync(
            dbContext,
            firstFactoryPumpRoomId,
            equipmentCode: "WP-001",
            equipmentName: "冷却水ポンプ1号機",
            equipmentType: EquipmentType.CoolingWaterPump,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-WP-15A",
            serialNumber: "DEMO-WP-0001",
            installedOn: new DateOnly(2020, 10, 1),
            notes: "第1工場ポンプ室の冷却水循環用主ポンプ",
            cancellationToken: cancellationToken);

        await AddOrUpdateEquipmentAsync(
            dbContext,
            secondFactoryPumpRoomId,
            equipmentCode: "WP-002",
            equipmentName: "冷却水ポンプ2号機",
            equipmentType: EquipmentType.CoolingWaterPump,
            manufacturer: "サンプル機械株式会社",
            modelNumber: "SAMPLE-WP-15B",
            serialNumber: "DEMO-WP-0002",
            installedOn: new DateOnly(2021, 6, 15),
            notes: "第2工場ポンプ室の冷却水循環用主ポンプ",
            cancellationToken: cancellationToken);

        /*
         * 換気設備
         */

        await AddOrUpdateEquipmentAsync(
            dbContext,
            firstFactoryVentilationRoomId,
            equipmentCode: "VE-001",
            equipmentName: "換気設備1号機",
            equipmentType: EquipmentType.Ventilation,
            manufacturer: "サンプル設備株式会社",
            modelNumber: "SAMPLE-VE-30A",
            serialNumber: "DEMO-VE-0001",
            installedOn: new DateOnly(2019, 8, 1),
            notes: "第1工場換気設備室の主換気設備",
            cancellationToken: cancellationToken);

        await AddOrUpdateEquipmentAsync(
            dbContext,
            secondFactoryVentilationRoomId,
            equipmentCode: "VE-002",
            equipmentName: "換気設備2号機",
            equipmentType: EquipmentType.Ventilation,
            manufacturer: "サンプル設備株式会社",
            modelNumber: "SAMPLE-VE-30B",
            serialNumber: "DEMO-VE-0002",
            installedOn: new DateOnly(2022, 3, 10),
            notes: "第2工場換気設備室の主換気設備",
            cancellationToken: cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    /// <summary>
    /// 工場名と設置場所名から、
    /// 有効な設置場所IDを取得する。
    /// </summary>
    private static async Task<Guid>
        GetRequiredLocationIdAsync(
            InspectionDbContext dbContext,
            string factorySiteName,
            string locationName,
            CancellationToken cancellationToken)
    {
        var locations =
            await dbContext.Locations
                .AsNoTracking()
                .Where(x =>
                    x.FactorySite.Name == factorySiteName &&
                    x.Name == locationName &&
                    x.FactorySite.IsActive &&
                    x.IsActive)
                .Select(x => new
                {
                    x.Id,
                    x.Code,
                    x.Name,
                    x.FactorySiteId
                })
                .ToListAsync(cancellationToken);

        foreach (var location in locations)
        {
            System.Diagnostics.Debug.WriteLine(
                $"{factorySiteName} / " +
                $"{location.Code} / " +
                $"{location.Name} / " +
                $"{location.Id}");
        }

        if (locations.Count == 0)
        {
            throw new InvalidOperationException(
                $"シード対象の設置場所" +
                $"「{factorySiteName} / {locationName}」" +
                "が見つかりません。");
        }

        if (locations.Count > 1)
        {
            throw new InvalidOperationException(
                $"シード対象の設置場所" +
                $"「{factorySiteName} / {locationName}」" +
                $"が{locations.Count}件あります。");
        }

        return locations[0].Id;
    }

    /// <summary>
    /// 設備コードが未登録なら追加し、
    /// 登録済みならシード内容で更新する。
    /// </summary>
    private static async Task
        AddOrUpdateEquipmentAsync(
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

        var normalizedEquipmentCode =
            equipmentCode
                .Trim()
                .ToUpperInvariant();

        var existingEquipment =
            await dbContext.Equipments
                .SingleOrDefaultAsync(
                    x =>
                        x.EquipmentCode ==
                        normalizedEquipmentCode,
                    cancellationToken);

        if (existingEquipment is null)
        {
            var equipment =
                new Equipment(
                    locationId: locationId,
                    equipmentCode:
                        normalizedEquipmentCode,
                    name:
                        equipmentName,
                    equipmentType:
                        equipmentType,
                    manufacturer:
                        manufacturer,
                    modelNumber:
                        modelNumber,
                    serialNumber:
                        serialNumber,
                    installedOn:
                        installedOn,
                    notes:
                        notes);

            dbContext.Equipments.Add(
                equipment);

            return;
        }

        /*
         * 既存設備の所属工場・設置場所が異なる場合、
         * 指定されたLocationへ移動する。
         */
        if (existingEquipment.LocationId !=
            locationId)
        {
            existingEquipment.ChangeLocation(
                locationId);
        }

        existingEquipment.Update(
            equipmentCode:
                normalizedEquipmentCode,
            name:
                equipmentName,
            equipmentType:
                equipmentType,
            manufacturer:
                manufacturer,
            modelNumber:
                modelNumber,
            serialNumber:
                serialNumber,
            installedOn:
                installedOn,
            notes:
                notes);
    }
}
