using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Locations;
using System;

namespace FacilityInspection.Domain.Equipments;

public sealed class Equipment : EntityBase
{
    public Guid LocationId { get; private set; }

    public Location Location { get; private set; } = null!;

    public string EquipmentCode { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;

    public EquipmentType EquipmentType { get; private set; }

    public string? Manufacturer { get; private set; }

    public string? ModelNumber { get; private set; }

    public string? SerialNumber { get; private set; }

    public DateOnly? InstalledOn { get; private set; }

    public EquipmentStatus Status { get; private set; }

    public string? Notes { get; private set; }

    // EF Core用
    private Equipment()
    {
    }

    public Equipment(
        Guid locationId,
        string equipmentCode,
        string name,
        EquipmentType equipmentType,
        string? manufacturer = null,
        string? modelNumber = null,
        string? serialNumber = null,
        DateOnly? installedOn = null,
        string? notes = null)
        : base()
    {
        SetLocation(locationId);
        SetEquipmentCode(equipmentCode);
        SetName(name);

        EquipmentType = equipmentType;
        Manufacturer = NormalizeOptional(manufacturer, 100);
        ModelNumber = NormalizeOptional(modelNumber, 100);
        SerialNumber = NormalizeOptional(serialNumber, 100);
        InstalledOn = installedOn;
        Notes = NormalizeOptional(notes, 1000);

        Status = EquipmentStatus.InService;
    }

    public void Update(
        string equipmentCode,
        string name,
        EquipmentType equipmentType,
        string? manufacturer,
        string? modelNumber,
        string? serialNumber,
        DateOnly? installedOn,
        string? notes)
    {
        SetEquipmentCode(equipmentCode);
        SetName(name);

        EquipmentType = equipmentType;
        Manufacturer = NormalizeOptional(manufacturer, 100);
        ModelNumber = NormalizeOptional(modelNumber, 100);
        SerialNumber = NormalizeOptional(serialNumber, 100);
        InstalledOn = installedOn;
        Notes = NormalizeOptional(notes, 1000);

        MarkUpdated();
    }

    public void ChangeLocation(Guid locationId)
    {
        SetLocation(locationId);
        MarkUpdated();
    }

    public void StartMaintenance()
    {
        ChangeStatus(EquipmentStatus.UnderMaintenance);
    }

    public void ResumeOperation()
    {
        ChangeStatus(EquipmentStatus.InService);
    }

    public void Suspend()
    {
        ChangeStatus(EquipmentStatus.Suspended);
    }

    public void Retire()
    {
        ChangeStatus(EquipmentStatus.Retired);
    }

    private void ChangeStatus(EquipmentStatus status)
    {
        if (Status == status)
        {
            return;
        }

        if (Status == EquipmentStatus.Retired &&
            status != EquipmentStatus.Retired)
        {
            throw new InvalidOperationException(
                "廃止済み設備を直接稼働状態へ戻すことはできません。");
        }

        Status = status;
        MarkUpdated();
    }

    private void SetLocation(Guid locationId)
    {
        if (locationId == Guid.Empty)
        {
            throw new ArgumentException(
                "設置場所IDを指定してください。",
                nameof(locationId));
        }

        LocationId = locationId;
    }

    private void SetEquipmentCode(string equipmentCode)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(equipmentCode);

        var normalized = equipmentCode.Trim().ToUpperInvariant();

        if (normalized.Length > 30)
        {
            throw new ArgumentOutOfRangeException(
                nameof(equipmentCode),
                "設備コードは30文字以内で入力してください。");
        }

        EquipmentCode = normalized;
    }

    private void SetName(string name)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        var normalized = name.Trim();

        if (normalized.Length > 100)
        {
            throw new ArgumentOutOfRangeException(
                nameof(name),
                "設備名は100文字以内で入力してください。");
        }

        Name = normalized;
    }

    private static string? NormalizeOptional(
        string? value,
        int maximumLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var normalized = value.Trim();

        if (normalized.Length > maximumLength)
        {
            throw new ArgumentOutOfRangeException(
                nameof(value),
                $"{maximumLength}文字以内で入力してください。");
        }

        return normalized;
    }
}