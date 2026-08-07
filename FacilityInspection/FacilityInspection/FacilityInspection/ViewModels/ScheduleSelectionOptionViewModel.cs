using FacilityInspection.Domain.Equipments;
using System;

namespace FacilityInspection.ViewModels;

public sealed class ScheduleSelectionOptionViewModel
{
    public ScheduleSelectionOptionViewModel(
        Guid id,
        string displayName,
        EquipmentType? equipmentType = null)
    {
        Id = id;
        DisplayName = displayName;
        EquipmentType = equipmentType;
    }

    public Guid Id { get; }

    public string DisplayName { get; }

    public EquipmentType? EquipmentType { get; }

    public override string ToString()
    {
        return DisplayName;
    }
}
