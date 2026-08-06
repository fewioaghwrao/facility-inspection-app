using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace FacilityInspection.ViewModels;

public sealed class InspectionTemplateListItemViewModel
{
    public InspectionTemplateListItemViewModel(
        Guid id,
        string name,
        string equipmentTypeName,
        int version,
        bool isActive,
        IEnumerable<InspectionTemplateItemRowViewModel> items)
    {
        Id = id;
        Name = name;
        EquipmentTypeName = equipmentTypeName;
        Version = version;
        IsActive = isActive;

        Items =
            new ObservableCollection<
                InspectionTemplateItemRowViewModel>(
                    items);
    }

    public Guid Id { get; }

    public string Name { get; }

    public string EquipmentTypeName { get; }

    public int Version { get; }

    public bool IsActive { get; }

    public string StatusText =>
        IsActive ? "有効" : "無効";

    public string VersionText =>
        $"バージョン {Version}";

    public string ActiveToggleText =>
    IsActive ? "無効化" : "有効化";

    public string ActiveStatusDescription =>
        IsActive
            ? "現在このテンプレートは使用できます。"
            : "現在このテンプレートは使用できません。";

    public ObservableCollection<
        InspectionTemplateItemRowViewModel> Items
    { get; }
}