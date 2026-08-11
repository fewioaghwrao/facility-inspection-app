using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class AuditLogListItemViewModel
{
    private readonly Action<Guid>
        _openDetailRequested;


    // ============================================
    // Constructor
    // ============================================

    public AuditLogListItemViewModel(
        AuditLogListData source,
        Action<Guid> openDetailRequested)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        ArgumentNullException.ThrowIfNull(
            openDetailRequested);

        _openDetailRequested =
            openDetailRequested;

        AuditLogId =
            source.AuditLogId;

        OccurredAtUtc =
            source.OccurredAtUtc;

        OperatorId =
            source.OperatorId;

        OperatorName =
            source.OperatorName;

        ActionType =
            source.ActionType;

        EntityType =
            source.EntityType;

        EntityId =
            source.EntityId;

        Reason =
            source.Reason ?? string.Empty;
    }


    // ============================================
    // Data
    // ============================================

    public Guid AuditLogId { get; }

    public DateTime OccurredAtUtc { get; }

    public Guid OperatorId { get; }

    public string OperatorName { get; }

    public AuditActionType ActionType { get; }

    public AuditEntityType EntityType { get; }

    public Guid EntityId { get; }

    public string Reason { get; }


    // ============================================
    // Display
    // ============================================

    /// <summary>
    /// UTCで保存した日時を端末のローカル時刻で表示する。
    /// </summary>
    public string OccurredAtText =>
        OccurredAtUtc
            .ToLocalTime()
            .ToString(
                "yyyy/MM/dd HH:mm:ss");


    public string ActionTypeText =>
        GetActionTypeText(
            ActionType);


    public string EntityTypeText =>
        GetEntityTypeText(
            EntityType);


    public string EntityIdText =>
        EntityId.ToString();


    public string ShortEntityIdText =>
        EntityId.ToString("N")[..8];


    public bool HasReason =>
        !string.IsNullOrWhiteSpace(
            Reason);


    // ============================================
    // Action Display
    // ============================================

    public static string GetActionTypeText(
        AuditActionType actionType)
    {
        return actionType switch
        {
            AuditActionType.Create =>
                "登録",

            AuditActionType.Update =>
                "更新",

            AuditActionType.Delete =>
                "削除",

            AuditActionType.Cancel =>
                "取消",

            AuditActionType.InspectionStart =>
                "点検開始",

            AuditActionType.InspectionComplete =>
                "点検完了",

            AuditActionType.Approve =>
                "承認",

            AuditActionType.ReturnForCorrection =>
                "差し戻し",

            AuditActionType.Login =>
                "ログイン",

            AuditActionType.Logout =>
                "ログアウト",

            AuditActionType.Backup =>
                "バックアップ",

            AuditActionType.Restore =>
                "復元",

            _ =>
                actionType.ToString()
        };
    }


    // ============================================
    // Entity Display
    // ============================================

    public static string GetEntityTypeText(
        AuditEntityType entityType)
    {
        return entityType switch
        {
            AuditEntityType.Inspection =>
                "点検",

            AuditEntityType.InspectionSchedule =>
                "点検予定",

            AuditEntityType.Equipment =>
                "設備",

            AuditEntityType.InspectionTemplate =>
                "点検票テンプレート",

            AuditEntityType.Operator =>
                "担当者",

            AuditEntityType.System =>
                "システム",

            _ =>
                entityType.ToString()
        };
    }


    // ============================================
    // Detail
    // ============================================

    [RelayCommand]
    private void OpenDetail()
    {
        if (AuditLogId == Guid.Empty)
        {
            return;
        }

        _openDetailRequested(
            AuditLogId);
    }
}
