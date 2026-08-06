using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Equipments;
using System;
using System.Collections.Generic;

namespace FacilityInspection.Domain.InspectionTemplates;

public sealed class InspectionTemplate : EntityBase
{
    public string Name { get; set; } = string.Empty;

    public EquipmentType EquipmentType { get; set; }

    /// <summary>
    /// テンプレートの版番号。
    /// </summary>
    public int Version { get; set; } = 1;

    public bool IsActive { get; set; } = true;

    public DateTime CreatedAt { get; set; }

    public DateTime? UpdatedAt { get; set; }

    public ICollection<InspectionTemplateItem> Items { get; set; }
        = new List<InspectionTemplateItem>();
}