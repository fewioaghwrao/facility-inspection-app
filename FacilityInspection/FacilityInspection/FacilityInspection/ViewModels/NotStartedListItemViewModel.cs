using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class NotStartedListItemViewModel
{
    private readonly Action<Guid>
        _openDetailRequested;


    // ============================================
    // Constructor
    // ============================================

    public NotStartedListItemViewModel(
        InspectionListData source,
        Action<Guid> openDetailRequested)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            openDetailRequested);

        _openDetailRequested =
            openDetailRequested;

        ScheduleId =
            source.ScheduleId;

        InspectionId =
            source.InspectionId;

        ScheduledDate =
            source.ScheduledDate;

        FactorySiteName =
            source.FactorySiteName;

        LocationName =
            source.LocationName;

        EquipmentCode =
            source.EquipmentCode;

        EquipmentName =
            source.EquipmentName;

        TemplateName =
            source.TemplateName;

        OperatorName =
            source.OperatorName;
    }


    // ============================================
    // Data
    // ============================================

    public Guid ScheduleId { get; }

    public Guid? InspectionId { get; }

    public DateOnly ScheduledDate { get; }

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string OperatorName { get; }


    // ============================================
    // Display Properties
    // ============================================

    public string ScheduledDateText =>
        ScheduledDate.ToString(
            "yyyy/MM/dd");

    public string LocationDisplayName =>
        $"{FactorySiteName} / {LocationName}";

    public string EquipmentDisplayName =>
        $"{EquipmentCode}  {EquipmentName}";

    public string StatusText =>
        "未実施";


    // ============================================
    // Detail
    // ============================================

    [RelayCommand]
    private void OpenDetail()
    {
        if (ScheduleId == Guid.Empty)
        {
            return;
        }

        _openDetailRequested(
            ScheduleId);
    }
}