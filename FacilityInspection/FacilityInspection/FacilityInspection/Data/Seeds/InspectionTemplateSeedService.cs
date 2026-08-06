using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data.Seeds;

public sealed class InspectionTemplateSeedService
{
    private readonly InspectionDbContext _dbContext;

    public InspectionTemplateSeedService(
        InspectionDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task SeedAsync(
        CancellationToken cancellationToken = default)
    {
        if (await _dbContext.InspectionTemplates.AnyAsync(
                cancellationToken))
        {
            return;
        }

        var templates = new[]
        {
            CreateAirCompressorTemplate(),
            CreateCoolingWaterPumpTemplate(),
            CreateVentilationTemplate()
        };

        await _dbContext.InspectionTemplates.AddRangeAsync(
            templates,
            cancellationToken);

        await _dbContext.SaveChangesAsync(
            cancellationToken);
    }

    private static InspectionTemplate CreateAirCompressorTemplate()
    {
        var template = new InspectionTemplate
        {
            Name = "エアコンプレッサー標準点検票",
            EquipmentType = EquipmentType.AirCompressor,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        template.Items =
        [
            CreateItem(
            template.Id,
            "運転音に異常がないか",
            InspectionInputType.NormalAbnormal,
            1),

        CreateItem(
            template.Id,
            "異常振動がないか",
            InspectionInputType.NormalAbnormal,
            2),

        CreateItem(
            template.Id,
            "空気圧",
            InspectionInputType.Numeric,
            3,
            "MPa"),

        CreateItem(
            template.Id,
            "油量は基準範囲内か",
            InspectionInputType.NormalAbnormal,
            4),

        CreateItem(
            template.Id,
            "漏れがないか",
            InspectionInputType.NormalAbnormal,
            5),

        CreateItem(
            template.Id,
            "ドレン排出を実施したか",
            InspectionInputType.DoneNotDone,
            6)
        ];

        return template;
    }

    private static InspectionTemplate CreateCoolingWaterPumpTemplate()
    {
        var template = new InspectionTemplate
        {
            Name = "冷却水ポンプ標準点検票",
            EquipmentType = EquipmentType.CoolingWaterPump,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        template.Items =
        [
            CreateItem(
            template.Id,
            "異常音がないか",
            InspectionInputType.NormalAbnormal,
            1),

        CreateItem(
            template.Id,
            "異常振動がないか",
            InspectionInputType.NormalAbnormal,
            2),

        CreateItem(
            template.Id,
            "水漏れがないか",
            InspectionInputType.NormalAbnormal,
            3),

        CreateItem(
            template.Id,
            "吐出圧力",
            InspectionInputType.Numeric,
            4,
            "MPa"),

        CreateItem(
            template.Id,
            "電流値",
            InspectionInputType.Numeric,
            5,
            "A"),

        CreateItem(
            template.Id,
            "周辺に障害物がないか",
            InspectionInputType.NormalAbnormal,
            6)
        ];

        return template;
    }

    private static InspectionTemplate CreateVentilationTemplate()
    {
        var template = new InspectionTemplate
        {
            Name = "換気設備標準点検票",
            EquipmentType = EquipmentType.Ventilation,
            Version = 1,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        template.Items =
        [
            CreateItem(
            template.Id,
            "運転状態",
            InspectionInputType.NormalAbnormal,
            1),

        CreateItem(
            template.Id,
            "異常音",
            InspectionInputType.NormalAbnormal,
            2),

        CreateItem(
            template.Id,
            "異常振動",
            InspectionInputType.NormalAbnormal,
            3),

        CreateItem(
            template.Id,
            "フィルター汚れ",
            InspectionInputType.NormalAbnormal,
            4),

        CreateItem(
            template.Id,
            "吸排気口の閉塞",
            InspectionInputType.NormalAbnormal,
            5),

        CreateItem(
            template.Id,
            "外観破損",
            InspectionInputType.NormalAbnormal,
            6)
        ];

        return template;
    }

    private static InspectionTemplateItem CreateItem(
        Guid templateId,
        string itemName,
        InspectionInputType inputType,
        int displayOrder,
        string? unit = null)
    {
        return new InspectionTemplateItem
        {
            InspectionTemplateId = templateId,
            ItemName = itemName,
            InputType = inputType,
            Unit = unit,
            DisplayOrder = displayOrder,
            IsRequired = true,
            IsActive = true
        };
    }
}