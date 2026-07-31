using System;

namespace FacilityInspection.Data;

/// <summary>
/// SQLite保存確認用の最小設備エンティティ。
/// 本実装では設備コード、設備種別、設置場所などを追加する。
/// </summary>
public sealed class Equipment
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}