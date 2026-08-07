using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Domain.Inspections;
using System;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class ScheduleListItemViewModel
{
    private readonly
        Func<ScheduleListItemViewModel, Task>
        _editRequested;

    private readonly
        Action<ScheduleListItemViewModel>
        _cancelRequested;

    public ScheduleListItemViewModel(
        Guid id,
        DateOnly scheduledDate,
        Guid factorySiteId,
        Guid locationId,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        string factorySiteName,
        string locationName,
        string equipmentCode,
        string equipmentName,
        string templateName,
        string operatorName,
        string? notes,
        InspectionStatus status,
        bool isCancelled,
        Func<ScheduleListItemViewModel, Task>
            editRequested,
        Action<ScheduleListItemViewModel>
            cancelRequested)
    {
        ArgumentNullException.ThrowIfNull(editRequested);
        ArgumentNullException.ThrowIfNull(cancelRequested);

        Id = id;
        ScheduledDate = scheduledDate;
        FactorySiteId = factorySiteId;
        LocationId = locationId;
        EquipmentId = equipmentId;
        InspectionTemplateId = inspectionTemplateId;
        AssignedOperatorId = assignedOperatorId;
        FactorySiteName = factorySiteName;
        LocationName = locationName;
        EquipmentCode = equipmentCode;
        EquipmentName = equipmentName;
        TemplateName = templateName;
        OperatorName = operatorName;
        Notes = notes;
        Status = status;
        IsCancelled = isCancelled;

        _editRequested = editRequested;
        _cancelRequested = cancelRequested;
    }

    public Guid Id { get; }

    public DateOnly ScheduledDate { get; }

    public Guid FactorySiteId { get; }

    public Guid LocationId { get; }

    public Guid EquipmentId { get; }

    public Guid InspectionTemplateId { get; }

    public Guid AssignedOperatorId { get; }

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string OperatorName { get; }

    public string? Notes { get; }

    public InspectionStatus Status { get; }

    public bool IsCancelled { get; }

    public string EquipmentDisplayName =>
        $"{EquipmentCode}  {EquipmentName}";

    public string LocationDisplayName =>
        $"{FactorySiteName} / {LocationName}";

    public string TemplateDisplayText =>
        $"点検票：{TemplateName}";

    public string OperatorDisplayText =>
        $"担当：{OperatorName}";

    public string NotesText =>
        string.IsNullOrWhiteSpace(Notes)
            ? "備考なし"
            : Notes;

    public bool IsOverdue =>
        !IsCancelled &&
        Status == InspectionStatus.NotStarted &&
        ScheduledDate <
            DateOnly.FromDateTime(DateTime.Today);

    public bool CanEdit =>
        !IsCancelled &&
        Status == InspectionStatus.NotStarted;

    public bool CanCancel => CanEdit;

    public string StatusText
    {
        get
        {
            if (IsCancelled)
            {
                return "取消";
            }

            if (IsOverdue)
            {
                return "期限超過";
            }

            return Status switch
            {
                InspectionStatus.NotStarted =>
                    "未実施",

                InspectionStatus.InProgress =>
                    "実施中",

                InspectionStatus.Completed =>
                    "完了・承認待ち",

                InspectionStatus.Returned =>
                    "差し戻し",

                InspectionStatus.Approved =>
                    "承認済み",

                _ => Status.ToString()
            };
        }
    }

    public string StatusBackground =>
        StatusText switch
        {
            "未実施" => "#F1F5F9",
            "実施中" => "#DBEAFE",
            "完了・承認待ち" => "#FFEDD5",
            "承認済み" => "#DCFCE7",
            "差し戻し" => "#FEE2E2",
            "期限超過" => "#FEE2E2",
            "取消" => "#E2E8F0",
            _ => "#F1F5F9"
        };

    public string StatusForeground =>
        StatusText switch
        {
            "未実施" => "#475569",
            "実施中" => "#1D4ED8",
            "完了・承認待ち" => "#C2410C",
            "承認済み" => "#15803D",
            "差し戻し" => "#B91C1C",
            "期限超過" => "#B91C1C",
            "取消" => "#64748B",
            _ => "#475569"
        };

    [RelayCommand]
    private async Task EditAsync()
    {
        await _editRequested(this);
    }

    [RelayCommand]
    private void CancelSchedule()
    {
        _cancelRequested(this);
    }
}
