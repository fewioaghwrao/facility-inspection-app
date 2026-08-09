using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Operators;
using System;
using System.Collections.Generic;

namespace FacilityInspection.Domain.Inspections;

public sealed class Inspection : EntityBase
{
    public Guid InspectionScheduleId { get; private set; }

    public InspectionSchedule InspectionSchedule { get; private set; } = null!;

    public InspectionStatus Status { get; private set; }

    public Guid? PerformedByOperatorId { get; private set; }

    public Operator? PerformedByOperator { get; private set; }

    public DateTime? StartedAtUtc { get; private set; }

    public DateTime? CompletedAtUtc { get; private set; }

    public DateTime? ReviewedAtUtc { get; private set; }

    public string? ReturnReason { get; private set; }

    public ICollection<InspectionResult> Results { get; private set; }
    = new List<InspectionResult>();

    public ICollection<InspectionPhoto> Photos { get; private set; }
    = new List<InspectionPhoto>();

    // EF Core用
    private Inspection()
    {
    }

    public Inspection(Guid inspectionScheduleId)
        : base()
    {
        if (inspectionScheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(inspectionScheduleId));
        }

        InspectionScheduleId = inspectionScheduleId;
        Status = InspectionStatus.NotStarted;
    }

    public void Start(
        Guid performedByOperatorId,
        DateTime startedAtUtc)
    {
        if (Status != InspectionStatus.NotStarted &&
            Status != InspectionStatus.Returned)
        {
            throw new InvalidOperationException(
                "未実施または差し戻し状態の点検だけ開始できます。");
        }

        if (performedByOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "実施担当者IDを指定してください。",
                nameof(performedByOperatorId));
        }

        PerformedByOperatorId = performedByOperatorId;
        StartedAtUtc = startedAtUtc;
        CompletedAtUtc = null;
        ReviewedAtUtc = null;
        ReturnReason = null;
        Status = InspectionStatus.InProgress;

        MarkUpdated();
    }

    public void Complete(DateTime completedAtUtc)
    {
        if (Status != InspectionStatus.InProgress)
        {
            throw new InvalidOperationException(
                "実施中の点検だけ完了できます。");
        }

        CompletedAtUtc = completedAtUtc;
        Status = InspectionStatus.Completed;

        MarkUpdated();
    }

    public void Return(
        string reason,
        DateTime reviewedAtUtc)
    {
        if (Status != InspectionStatus.Completed)
        {
            throw new InvalidOperationException(
                "承認待ちの点検だけ差し戻しできます。");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(reason);

        var normalizedReason = reason.Trim();

        if (normalizedReason.Length > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(reason),
                "差し戻し理由は500文字以内で入力してください。");
        }

        ReturnReason = normalizedReason;
        ReviewedAtUtc = reviewedAtUtc;
        Status = InspectionStatus.Returned;

        MarkUpdated();
    }

    public void Approve(DateTime reviewedAtUtc)
    {
        if (Status != InspectionStatus.Completed)
        {
            throw new InvalidOperationException(
                "承認待ちの点検だけ承認できます。");
        }

        ReturnReason = null;
        ReviewedAtUtc = reviewedAtUtc;
        Status = InspectionStatus.Approved;

        MarkUpdated();
    }
}
