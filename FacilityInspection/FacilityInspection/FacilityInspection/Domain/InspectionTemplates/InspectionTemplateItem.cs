using FacilityInspection.Domain.Common;
using System;

namespace FacilityInspection.Domain.InspectionTemplates;

public sealed class InspectionTemplateItem : EntityBase
{
    public Guid InspectionTemplateId { get; set; }

    public InspectionTemplate InspectionTemplate { get; set; } = null!;

    public string ItemName { get; set; } = string.Empty;

    public InspectionInputType InputType { get; set; }

    /// <summary>
    /// MPa、Aなどの単位。
    /// </summary>
    public string? Unit { get; set; }

    /// <summary>
    /// 数値入力時の基準下限。
    /// </summary>
    public double? MinimumValue { get; set; }

    /// <summary>
    /// 数値入力時の基準上限。
    /// </summary>
    public double? MaximumValue { get; set; }

    public int DisplayOrder { get; set; }

    public bool IsRequired { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public string? Description { get; set; }
}