using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.Operators;
using System;

namespace FacilityInspection.Domain.AuditLogs;

/// <summary>
/// システム内で行われた重要な操作を記録する監査ログ。
///
/// AuditLogは作成後に変更しないことを前提とする。
/// </summary>
public sealed class AuditLog : EntityBase
{
    // ============================================
    // Occurred At
    // ============================================

    /// <summary>
    /// 操作発生日時。
    /// UTCで保存する。
    /// </summary>
    public DateTime OccurredAtUtc
    {
        get;
        private set;
    }


    // ============================================
    // Operator
    // ============================================

    /// <summary>
    /// 操作者ID。
    /// </summary>
    public Guid OperatorId
    {
        get;
        private set;
    }

    /// <summary>
    /// 操作者。
    /// </summary>
    public Operator Operator
    {
        get;
        private set;
    } = null!;


    // ============================================
    // Action
    // ============================================

    /// <summary>
    /// 操作種別。
    /// </summary>
    public AuditActionType ActionType
    {
        get;
        private set;
    }


    // ============================================
    // Target Entity
    // ============================================

    /// <summary>
    /// 操作対象のエンティティ種別。
    /// </summary>
    public AuditEntityType EntityType
    {
        get;
        private set;
    }

    /// <summary>
    /// 操作対象のエンティティID。
    /// </summary>
    public Guid EntityId
    {
        get;
        private set;
    }


    // ============================================
    // Change Values
    // ============================================

    /// <summary>
    /// 変更前の概要。
    ///
    /// 必要に応じてJSON文字列を保存可能。
    /// </summary>
    public string? BeforeValue
    {
        get;
        private set;
    }

    /// <summary>
    /// 変更後の概要。
    ///
    /// 必要に応じてJSON文字列を保存可能。
    /// </summary>
    public string? AfterValue
    {
        get;
        private set;
    }


    // ============================================
    // Reason
    // ============================================

    /// <summary>
    /// 操作理由。
    ///
    /// 差し戻し理由や訂正理由などに使用する。
    /// </summary>
    public string? Reason
    {
        get;
        private set;
    }


    // ============================================
    // EF Core
    // ============================================

    private AuditLog()
    {
    }


    // ============================================
    // Constructor
    // ============================================

    public AuditLog(
        Guid operatorId,
        AuditActionType actionType,
        AuditEntityType entityType,
        Guid entityId,
        string? beforeValue = null,
        string? afterValue = null,
        string? reason = null,
        DateTime? occurredAtUtc = null)
        : base()
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作者IDを指定してください。",
                nameof(operatorId));
        }

        if (entityId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作対象IDを指定してください。",
                nameof(entityId));
        }

        if (!Enum.IsDefined(actionType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(actionType),
                "有効な操作種別を指定してください。");
        }

        if (!Enum.IsDefined(entityType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(entityType),
                "有効な対象種別を指定してください。");
        }

        OperatorId =
            operatorId;

        ActionType =
            actionType;

        EntityType =
            entityType;

        EntityId =
            entityId;

        BeforeValue =
            Normalize(beforeValue);

        AfterValue =
            Normalize(afterValue);

        Reason =
            Normalize(reason);

        OccurredAtUtc =
            occurredAtUtc ??
            DateTime.UtcNow;
    }


    // ============================================
    // Normalize
    // ============================================

    private static string? Normalize(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}