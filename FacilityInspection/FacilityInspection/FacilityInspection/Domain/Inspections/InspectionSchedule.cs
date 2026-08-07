using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Operators;
using System;

namespace FacilityInspection.Domain.Inspections;

public sealed class InspectionSchedule : EntityBase
{
    public DateOnly ScheduledDate { get; private set; }

    public Guid EquipmentId { get; private set; }

    public Equipment Equipment { get; private set; } = null!;

    public Guid InspectionTemplateId { get; private set; }

    public InspectionTemplate InspectionTemplate { get; private set; } = null!;

    public Guid AssignedOperatorId { get; private set; }

    public Operator AssignedOperator { get; private set; } = null!;

    public string? Notes { get; private set; }

    public bool IsCancelled { get; private set; }

    public Inspection? Inspection { get; private set; }

    // EF Core用
    private InspectionSchedule()
    {
    }

    public InspectionSchedule(
        DateOnly scheduledDate,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        string? notes = null)
        : base()
    {
        SetScheduledDate(scheduledDate);
        SetEquipment(equipmentId);
        SetInspectionTemplate(inspectionTemplateId);
        SetAssignedOperator(assignedOperatorId);

        Notes = NormalizeNotes(notes);
        IsCancelled = false;
    }

    public void Update(
        DateOnly scheduledDate,
        Guid equipmentId,
        Guid inspectionTemplateId,
        Guid assignedOperatorId,
        string? notes)
    {
        if (IsCancelled)
        {
            throw new InvalidOperationException(
                "取消済みの点検予定は変更できません。");
        }

        SetScheduledDate(scheduledDate);
        SetEquipment(equipmentId);
        SetInspectionTemplate(inspectionTemplateId);
        SetAssignedOperator(assignedOperatorId);

        Notes = NormalizeNotes(notes);

        MarkUpdated();
    }

    public void Cancel()
    {
        if (IsCancelled)
        {
            return;
        }

        IsCancelled = true;
        MarkUpdated();
    }

    public void AttachInspection(Inspection inspection)
    {
        ArgumentNullException.ThrowIfNull(inspection);

        if (inspection.InspectionScheduleId != Id)
        {
            throw new InvalidOperationException(
                "点検予定と点検記録のIDが一致しません。");
        }

        Inspection = inspection;
    }

    private void SetScheduledDate(DateOnly scheduledDate)
    {
        if (scheduledDate == default)
        {
            throw new ArgumentException(
                "点検予定日を指定してください。",
                nameof(scheduledDate));
        }

        ScheduledDate = scheduledDate;
    }

    private void SetEquipment(Guid equipmentId)
    {
        if (equipmentId == Guid.Empty)
        {
            throw new ArgumentException(
                "設備IDを指定してください。",
                nameof(equipmentId));
        }

        EquipmentId = equipmentId;
    }

    private void SetInspectionTemplate(
        Guid inspectionTemplateId)
    {
        if (inspectionTemplateId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検票テンプレートIDを指定してください。",
                nameof(inspectionTemplateId));
        }

        InspectionTemplateId = inspectionTemplateId;
    }

    private void SetAssignedOperator(
        Guid assignedOperatorId)
    {
        if (assignedOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "担当者IDを指定してください。",
                nameof(assignedOperatorId));
        }

        AssignedOperatorId = assignedOperatorId;
    }

    private static string? NormalizeNotes(
        string? notes)
    {
        if (string.IsNullOrWhiteSpace(notes))
        {
            return null;
        }

        var normalized = notes.Trim();

        if (normalized.Length > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(notes),
                "備考は500文字以内で入力してください。");
        }

        return normalized;
    }
}
