namespace FacilityInspection.Domain.AuditLogs;

/// <summary>
/// 操作履歴の対象となるエンティティ種別。
/// </summary>
public enum AuditEntityType
{
    Inspection = 1,

    InspectionSchedule = 2,

    Equipment = 3,

    InspectionTemplate = 4,

    Operator = 5,

    Database = 90,

    System = 99
}