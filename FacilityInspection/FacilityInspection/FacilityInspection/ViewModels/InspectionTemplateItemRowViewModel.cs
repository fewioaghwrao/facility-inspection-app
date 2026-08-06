using FacilityInspection.Domain.InspectionTemplates;
using System;

namespace FacilityInspection.ViewModels;

public sealed class InspectionTemplateItemRowViewModel
{
    public InspectionTemplateItemRowViewModel(
        Guid id,
        int displayOrder,
        string itemName,
        InspectionInputType inputType,
        string inputTypeName,
        string? unit,
        double? minimumValue,
        double? maximumValue,
        bool isRequired,
        bool isActive)
    {
        Id = id;
        DisplayOrder = displayOrder;
        ItemName = itemName;
        InputType = inputType;
        InputTypeName = inputTypeName;
        Unit = unit;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        IsRequired = isRequired;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public int DisplayOrder { get; }

    public string ItemName { get; }

    public InspectionInputType InputType { get; }

    public string InputTypeName { get; }

    public string? Unit { get; }

    public double? MinimumValue { get; }

    public double? MaximumValue { get; }

    public bool IsRequired { get; }

    public bool IsActive { get; }

    public string UnitText =>
        string.IsNullOrWhiteSpace(Unit)
            ? "－"
            : Unit;

    public string RequiredText =>
        IsRequired ? "必須" : "任意";

    public string StatusText =>
        IsActive ? "有効" : "無効";

    public string StandardRangeText
    {
        get
        {
            if (!MinimumValue.HasValue &&
                !MaximumValue.HasValue)
            {
                return "－";
            }

            if (MinimumValue.HasValue &&
                MaximumValue.HasValue)
            {
                return $"{MinimumValue} ～ {MaximumValue}";
            }

            if (MinimumValue.HasValue)
            {
                return $"{MinimumValue}以上";
            }

            return $"{MaximumValue}以下";
        }
    }
}