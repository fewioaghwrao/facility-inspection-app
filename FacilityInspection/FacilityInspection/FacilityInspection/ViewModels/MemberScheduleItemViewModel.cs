using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Domain.Inspections;
using System;

namespace FacilityInspection.ViewModels;

public sealed class MemberScheduleItemViewModel
{
    private readonly Action<Guid> _openInspection;

    public MemberScheduleItemViewModel(
        Guid scheduleId,
        DateOnly scheduledDate,
        string factorySiteName,
        string locationName,
        string equipmentCode,
        string equipmentName,
        string templateName,
        string? notes,
        InspectionStatus status,
        Action<Guid> openInspection)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            factorySiteName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            locationName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            equipmentCode);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            equipmentName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            templateName);

        ArgumentNullException.ThrowIfNull(
            openInspection);

        ScheduleId = scheduleId;
        ScheduledDate = scheduledDate;
        FactorySiteName = factorySiteName;
        LocationName = locationName;
        EquipmentCode = equipmentCode;
        EquipmentName = equipmentName;
        TemplateName = templateName;
        Notes = notes;
        Status = status;

        _openInspection = openInspection;

        StartInspectionCommand =
            new RelayCommand(
                StartInspection,
                () => CanStartInspection);
    }

    public Guid ScheduleId { get; }

    public DateOnly ScheduledDate { get; }

    public string FactorySiteName { get; }

    public string LocationName { get; }

    public string EquipmentCode { get; }

    public string EquipmentName { get; }

    public string TemplateName { get; }

    public string? Notes { get; }

    public InspectionStatus Status { get; }

    public string LocationDisplay =>
        $"{FactorySiteName} / {LocationName}";

    public string TemplateDisplay =>
        $"点検票: {TemplateName}";

    public string NotesDisplay =>
        string.IsNullOrWhiteSpace(Notes)
            ? "備考: なし"
            : $"備考: {Notes.Trim()}";

    public string StatusText =>
        Status switch
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


            _ =>
                Status.ToString()
        };

    public string StatusBackground =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "#F1F5F9",

            InspectionStatus.InProgress =>
                "#EFF6FF",

            InspectionStatus.Completed =>
                "#FFF7ED",

            InspectionStatus.Returned =>
                "#FEF2F2",

            InspectionStatus.Approved =>
                "#F0FDF4",

            _ =>
                "#F8FAFC"
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

            InspectionStatus.Returned =>
                "#B91C1C",

            InspectionStatus.Approved =>
                "#15803D",

            _ =>
                "#475569"
        };

    public bool CanStartInspection =>
        Status is
            InspectionStatus.NotStarted or
            InspectionStatus.InProgress or
            InspectionStatus.Returned;

    public string ActionButtonText =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "点検する",

            InspectionStatus.Returned =>
                "修正する",

            InspectionStatus.InProgress =>
                "点検を再開",

            _ =>
                "確認"
        };

    public IRelayCommand StartInspectionCommand { get; }

    private void StartInspection()
    {
        _openInspection(ScheduleId);
    }
}
