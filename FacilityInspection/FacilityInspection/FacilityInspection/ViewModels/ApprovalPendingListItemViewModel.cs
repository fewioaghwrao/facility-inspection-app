using CommunityToolkit.Mvvm.Input;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class ApprovalPendingListItemViewModel
{
    private readonly Action<Guid>
        _detailRequested;

    public ApprovalPendingListItemViewModel(
        Guid scheduleId,
        Guid inspectionId,
        DateOnly scheduledDate,
        string factorySiteName,
        string locationName,
        string equipmentCode,
        string equipmentName,
        string templateName,
        string operatorName,
        int resultCount,
        int abnormalCount,
        int photoCount,
        Action<Guid> detailRequested)
    {
        ScheduleId = scheduleId;
        InspectionId = inspectionId;
        ScheduledDate = scheduledDate;
        FactorySiteName = factorySiteName;
        LocationName = locationName;
        EquipmentCode = equipmentCode;
        EquipmentName = equipmentName;
        TemplateName = templateName;
        OperatorName = operatorName;
        ResultCount = resultCount;
        AbnormalCount = abnormalCount;
        PhotoCount = photoCount;

        _detailRequested =
            detailRequested;
    }

    public Guid ScheduleId { get; }

    public Guid InspectionId { get; }

    public DateOnly ScheduledDate { get; }

    public string ScheduledDateText =>
        ScheduledDate.ToString(
            "yyyy/MM/dd");

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string OperatorName { get; }

    public int ResultCount { get; }

    public int AbnormalCount { get; }

    public int PhotoCount { get; }

    [RelayCommand]
    private void OpenDetail()
    {
        _detailRequested(
            ScheduleId);
    }
}
