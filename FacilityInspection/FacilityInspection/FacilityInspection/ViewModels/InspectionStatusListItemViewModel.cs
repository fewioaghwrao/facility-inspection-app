using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionStatusListItemViewModel
{
    private readonly Action<Guid>
        _openDetailRequested;

    public InspectionStatusListItemViewModel(
        InspectionListData source,
        Action<Guid> openDetailRequested)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(
            openDetailRequested);

        _openDetailRequested =
            openDetailRequested;

        ScheduleId = source.ScheduleId;
        InspectionId = source.InspectionId;
        ScheduledDate = source.ScheduledDate;
        FactorySiteName = source.FactorySiteName;
        LocationName = source.LocationName;
        EquipmentCode = source.EquipmentCode;
        EquipmentName = source.EquipmentName;
        TemplateName = source.TemplateName;
        OperatorName = source.OperatorName;
        Status = source.Status;
        ResultCount = source.ResultCount;
        AbnormalCount = source.AbnormalCount;
        PhotoCount = source.PhotoCount;
    }

    public Guid ScheduleId { get; }

    public Guid? InspectionId { get; }

    public DateOnly ScheduledDate { get; }

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string OperatorName { get; }

    public InspectionStatus Status { get; }

    public int ResultCount { get; }

    public int AbnormalCount { get; }

    public int PhotoCount { get; }

    public string ScheduledDateText =>
        $"{ScheduledDate:yyyy/MM/dd}";

    public string LocationDisplayName =>
        $"{FactorySiteName} / {LocationName}";

    public string EquipmentDisplayName =>
        $"{EquipmentCode}  {EquipmentName}";

    public string ResultCountText =>
        $"{ResultCount}項目";

    public string PhotoCountText =>
        $"{PhotoCount}枚";

    public string AbnormalCountText =>
        AbnormalCount == 0
            ? "異常なし"
            : $"異常 {AbnormalCount}件";

    public string AbnormalBackground =>
        AbnormalCount == 0
            ? "#DCFCE7"
            : "#FEE2E2";

    public string AbnormalForeground =>
        AbnormalCount == 0
            ? "#15803D"
            : "#B91C1C";

    public string StatusText =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "未実施",

            InspectionStatus.InProgress =>
                "実施中",

            InspectionStatus.Completed =>
                "完了・承認待ち",

            InspectionStatus.Approved =>
                "承認済み",

            InspectionStatus.Returned =>
                "差し戻し",

            _ =>
                Status.ToString()
        };

    public string StatusBackground =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "#F1F5F9",

            InspectionStatus.InProgress =>
                "#DBEAFE",

            InspectionStatus.Completed =>
                "#FFEDD5",

            InspectionStatus.Approved =>
                "#DCFCE7",

            InspectionStatus.Returned =>
                "#FEE2E2",

            _ =>
                "#F1F5F9"
        };

    public string StatusForeground =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "#475569",

            InspectionStatus.InProgress =>
                "#1D4ED8",

            InspectionStatus.Completed =>
                "#C2410C",

            InspectionStatus.Approved =>
                "#15803D",

            InspectionStatus.Returned =>
                "#B91C1C",

            _ =>
                "#475569"
        };

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