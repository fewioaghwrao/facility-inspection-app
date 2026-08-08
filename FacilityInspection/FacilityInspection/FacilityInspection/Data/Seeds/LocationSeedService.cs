using FacilityInspection.Domain.Locations;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class LocationSeedService(
    IDbContextFactory<InspectionDbContext> dbContextFactory)
{
    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        await using var dbContext =
            await dbContextFactory.CreateDbContextAsync(
                cancellationToken);

        var firstFactory =
            await dbContext.FactorySites
                .SingleOrDefaultAsync(
                    x =>
                        x.Name == "第1工場" &&
                        x.IsActive,
                    cancellationToken);

        if (firstFactory is null)
        {
            throw new InvalidOperationException(
                "シード対象の工場「第1工場」が見つかりません。");
        }

        var secondFactory =
            await dbContext.FactorySites
                .SingleOrDefaultAsync(
                    x =>
                        x.Name == "第2工場" &&
                        x.IsActive,
                    cancellationToken);

        if (secondFactory is null)
        {
            throw new InvalidOperationException(
                "シード対象の工場「第2工場」が見つかりません。");
        }

        /*
         * 第1工場
         */

        await AddLocationIfMissingAsync(
            dbContext,
            firstFactory.Id,
            code: "COMPRESSOR",
            name: "コンプレッサー室",
            floor: "1F",
            description: "エアコンプレッサー設置場所",
            cancellationToken);

        await AddLocationIfMissingAsync(
            dbContext,
            firstFactory.Id,
            code: "PUMP",
            name: "ポンプ室",
            floor: "1F",
            description: "冷却水ポンプ設置場所",
            cancellationToken);

        await AddLocationIfMissingAsync(
            dbContext,
            firstFactory.Id,
            code: "VENTILATION",
            name: "換気設備室",
            floor: "1F",
            description: "換気設備設置場所",
            cancellationToken);

        /*
         * 第2工場
         */

        await AddLocationIfMissingAsync(
            dbContext,
            secondFactory.Id,
            code: "COMPRESSOR",
            name: "コンプレッサー室",
            floor: "1F",
            description: "エアコンプレッサー設置場所",
            cancellationToken);

        await AddLocationIfMissingAsync(
            dbContext,
            secondFactory.Id,
            code: "PUMP",
            name: "ポンプ室",
            floor: "1F",
            description: "冷却水ポンプ設置場所",
            cancellationToken);

        await AddLocationIfMissingAsync(
            dbContext,
            secondFactory.Id,
            code: "VENTILATION",
            name: "換気設備室",
            floor: "1F",
            description: "換気設備設置場所",
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static async Task AddLocationIfMissingAsync(
        InspectionDbContext dbContext,
        Guid factorySiteId,
        string code,
        string name,
        string? floor,
        string? description,
        CancellationToken cancellationToken)
    {
        var normalizedCode =
            code.Trim().ToUpperInvariant();

        var normalizedName =
            name.Trim();

        var existingLocation =
            await dbContext.Locations
                .SingleOrDefaultAsync(
                    x =>
                        x.FactorySiteId == factorySiteId &&
                        (
                            x.Code == normalizedCode ||
                            x.Name == normalizedName
                        ),
                    cancellationToken);

        if (existingLocation is null)
        {
            var location =
                new FacilityInspection.Domain.Locations.Location(
                    factorySiteId: factorySiteId,
                    code: normalizedCode,
                    name: normalizedName,
                    floor: floor,
                    description: description);

            dbContext.Locations.Add(location);

            return;
        }

        existingLocation.Update(
            code: normalizedCode,
            name: normalizedName,
            floor: floor,
            description: description);

        existingLocation.Activate();
    }
}