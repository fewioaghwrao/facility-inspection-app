using FacilityInspection.Data;
using FacilityInspection.Domain.InspectionTemplates;
using System;

namespace FacilityInspection.ViewModels;

public sealed class InspectionResultDetailItemViewModel
{
    public InspectionResultDetailItemViewModel(
        InspectionResultDetailData source)
    {
        ArgumentNullException.ThrowIfNull(source);

        ResultId = source.ResultId;
        DisplayOrder = source.DisplayOrder;
        ItemName = source.ItemName;
        InputType = source.InputType;
        CheckValue = source.CheckValue;
        NumericValue = source.NumericValue;
        TextValue = source.TextValue;
        Unit = source.Unit;
        IsAbnormal = source.IsAbnormal;
        Comment = source.Comment ?? string.Empty;
    }

    public Guid ResultId { get; }

    public int DisplayOrder { get; }

    public string ItemName { get; }

    public InspectionInputType InputType { get; }

    public bool? CheckValue { get; }

    public decimal? NumericValue { get; }

    public string? TextValue { get; }

    public string? Unit { get; }

    public bool IsAbnormal { get; }

    public string Comment { get; }

    public bool HasComment =>
        !string.IsNullOrWhiteSpace(
            Comment);

    public string InputTypeText =>
        InputType switch
        {
            InspectionInputType.NormalAbnormal =>
                "正常・異常",

            InspectionInputType.DoneNotDone =>
                "実施・未実施",

            InspectionInputType.Numeric =>
                "数値",

            InspectionInputType.Text =>
                "文字",

            _ =>
                InputType.ToString()
        };

    public string ValueText =>
        InputType switch
        {
            InspectionInputType.NormalAbnormal =>
                CheckValue switch
                {
                    true => "正常",
                    false => "異常",
                    null => "未入力"
                },

            InspectionInputType.DoneNotDone =>
                CheckValue switch
                {
                    true => "実施",
                    false => "未実施",
                    null => "未入力"
                },

            InspectionInputType.Numeric =>
                NumericValue.HasValue
                    ? string.IsNullOrWhiteSpace(Unit)
                        ? $"{NumericValue.Value}"
                        : $"{NumericValue.Value} {Unit}"
                    : "未入力",

            InspectionInputType.Text =>
                string.IsNullOrWhiteSpace(TextValue)
                    ? "未入力"
                    : TextValue,

            _ =>
                "-"
        };

    public string ResultStatusText =>
        IsAbnormal
            ? "異常"
            : "正常";

    public string StatusBackground =>
        IsAbnormal
            ? "#FEE2E2"
            : "#DCFCE7";

    public string StatusForeground =>
        IsAbnormal
            ? "#B91C1C"
            : "#15803D";
}