using FacilityInspection.Domain.Common;
using System;

namespace FacilityInspection.Domain.Inspections;

/// <summary>
/// 点検時に登録された結果写真。
/// </summary>
public sealed class InspectionPhoto : EntityBase
{
    public Guid InspectionId { get; private set; }

    public Inspection Inspection { get; private set; } = null!;

    /// <summary>
    /// 特定の点検項目に紐づく場合に設定する。
    /// 点検全体の写真の場合はnull。
    /// </summary>
    public Guid? InspectionResultId { get; private set; }

    public InspectionResult? InspectionResult { get; private set; }

    /// <summary>
    /// アプリケーション基準フォルダーからの相対パス。
    /// 例：sample/images/resultcomp1.jpg
    /// </summary>
    public string RelativePath { get; private set; } = string.Empty;

    public string? Caption { get; private set; }

    public int DisplayOrder { get; private set; }

    public DateTime CapturedAtUtc { get; private set; }

    // EF Core用
    private InspectionPhoto()
    {
    }

    public InspectionPhoto(
        Guid inspectionId,
        string relativePath,
        DateTime capturedAtUtc,
        int displayOrder = 0,
        Guid? inspectionResultId = null,
        string? caption = null)
        : base()
    {
        if (inspectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検実施IDを指定してください。",
                nameof(inspectionId));
        }

        if (inspectionResultId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検結果IDには空のGUIDを指定できません。",
                nameof(inspectionResultId));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "表示順は0以上で指定してください。");
        }

        InspectionId = inspectionId;
        InspectionResultId = inspectionResultId;
        RelativePath = NormalizeRelativePath(relativePath);
        CapturedAtUtc = capturedAtUtc;
        DisplayOrder = displayOrder;
        Caption = NormalizeCaption(caption);
    }

    public void ChangeCaption(string? caption)
    {
        Caption = NormalizeCaption(caption);

        MarkUpdated();
    }

    public void ChangeDisplayOrder(int displayOrder)
    {
        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "表示順は0以上で指定してください。");
        }

        DisplayOrder = displayOrder;

        MarkUpdated();
    }

    private static string NormalizeRelativePath(string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(relativePath);

        var normalizedPath = relativePath
            .Trim()
            .Replace('\\', '/');

        if (normalizedPath.Length > 500)
        {
            throw new ArgumentOutOfRangeException(
                nameof(relativePath),
                "写真パスは500文字以内で指定してください。");
        }

        if (normalizedPath.StartsWith('/') ||
            normalizedPath.Contains(":/", StringComparison.Ordinal) ||
            normalizedPath.StartsWith("../", StringComparison.Ordinal) ||
            normalizedPath.Contains("/../", StringComparison.Ordinal) ||
            normalizedPath.EndsWith("/..", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "写真パスにはアプリケーション基準フォルダーからの相対パスを指定してください。",
                nameof(relativePath));
        }

        return normalizedPath;
    }

    private static string? NormalizeCaption(string? caption)
    {
        if (string.IsNullOrWhiteSpace(caption))
        {
            return null;
        }

        var normalizedCaption = caption.Trim();

        if (normalizedCaption.Length > 200)
        {
            throw new ArgumentOutOfRangeException(
                nameof(caption),
                "写真説明は200文字以内で入力してください。");
        }

        return normalizedCaption;
    }
}