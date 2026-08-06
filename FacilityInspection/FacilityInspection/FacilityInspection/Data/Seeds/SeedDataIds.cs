using System;

namespace FacilityInspection.Data.Seeds;

internal static class SeedDataIds
{
    // FactorySite
    public static readonly Guid FirstFactorySiteId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000001");

    public static readonly Guid SecondFactorySiteId =
        Guid.Parse(
            "10000000-0000-0000-0000-000000000002");

    // 第1工場のLocation
    public static readonly Guid FirstFactoryCompressorRoomId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000001");

    public static readonly Guid FirstFactoryPumpRoomId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000002");

    public static readonly Guid FirstFactoryVentilationRoomId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000003");

    // 第2工場のLocation
    public static readonly Guid SecondFactoryPumpRoomId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000004");

    public static readonly Guid SecondFactoryDustCollectionRoomId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000005");

    public static readonly Guid SecondFactoryOutdoorAreaId =
        Guid.Parse(
            "20000000-0000-0000-0000-000000000006");

    public static readonly Guid AirCompressorTemplateId =
    Guid.Parse("20000000-0000-0000-0000-000000000001");

    public static readonly Guid CoolingWaterPumpTemplateId =
        Guid.Parse("20000000-0000-0000-0000-000000000002");

    public static readonly Guid VentilationTemplateId =
        Guid.Parse("20000000-0000-0000-0000-000000000003");
}