using FacilityInspection.Domain.Common;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Collections.Generic;

namespace FacilityInspection.Domain.Inspections;

/// <summary>
/// 点検項目ごとの実施結果。
/// </summary>
public sealed class InspectionResult : EntityBase
{
    /// <summary>
    /// EF Core用コンストラクター。
    /// </summary>
    private InspectionResult()
    {
    }

    public InspectionResult(
        Guid inspectionId,
        Guid inspectionTemplateItemId,
        int displayOrder,
        string itemName,
        InspectionInputType inputType,
        string? unit = null)
    {
        if (inspectionId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検実施IDは必須です。",
                nameof(inspectionId));
        }

        if (inspectionTemplateItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検テンプレート項目IDは必須です。",
                nameof(inspectionTemplateItemId));
        }

        if (displayOrder < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(displayOrder),
                "表示順は0以上で指定してください。");
        }

        if (string.IsNullOrWhiteSpace(itemName))
        {
            throw new ArgumentException(
                "点検項目名は必須です。",
                nameof(itemName));
        }

        InspectionId = inspectionId;
        InspectionTemplateItemId = inspectionTemplateItemId;
        DisplayOrder = displayOrder;
        ItemName = itemName.Trim();
        InputType = inputType;
        Unit = NormalizeOptionalText(unit);
    }

    /// <summary>
    /// 点検実施ID。
    /// </summary>
    public Guid InspectionId { get; private set; }

    /// <summary>
    /// 元になった点検テンプレート項目ID。
    /// </summary>
    public Guid InspectionTemplateItemId { get; private set; }

    /// <summary>
    /// 点検実施時点の表示順。
    /// </summary>
    public int DisplayOrder { get; private set; }

    /// <summary>
    /// 点検実施時点の項目名。
    /// テンプレート変更後も過去結果を保持するため結果側にも保存する。
    /// </summary>
    public string ItemName { get; private set; } = string.Empty;

    /// <summary>
    /// 点検実施時点の入力形式。
    /// </summary>
    public InspectionInputType InputType { get; private set; }

    /// <summary>
    /// チェック形式の入力値。
    /// </summary>
    public bool? CheckValue { get; private set; }

    /// <summary>
    /// 数値形式の入力値。
    /// </summary>
    public decimal? NumericValue { get; private set; }

    /// <summary>
    /// 文字列形式の入力値。
    /// </summary>
    public string? TextValue { get; private set; }

    /// <summary>
    /// 数値項目の単位。
    /// </summary>
    public string? Unit { get; private set; }

    /// <summary>
    /// 異常の有無。
    /// </summary>
    public bool IsAbnormal { get; private set; }

    /// <summary>
    /// 点検者コメント。
    /// </summary>
    public string? Comment { get; private set; }

    public Inspection Inspection { get; private set; } = null!;

    public InspectionTemplateItem InspectionTemplateItem { get; private set; }
        = null!;

    public ICollection<InspectionPhoto> Photos { get; private set; }
    = new List<InspectionPhoto>();

    /// <summary>
    /// 点検結果を更新する。
    /// InputTypeに対応する値だけを指定し、それ以外はnullとする。
    /// </summary>
    public void UpdateResult(
        bool? checkValue,
        decimal? numericValue,
        string? textValue,
        bool isAbnormal,
        string? comment)
    {
        CheckValue = checkValue;
        NumericValue = numericValue;
        TextValue = NormalizeOptionalText(textValue);
        IsAbnormal = isAbnormal;
        Comment = NormalizeOptionalText(comment);
    }

    private static string? NormalizeOptionalText(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}