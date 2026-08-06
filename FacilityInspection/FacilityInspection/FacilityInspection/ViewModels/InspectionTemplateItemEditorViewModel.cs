using CommunityToolkit.Mvvm.ComponentModel;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Collections.Generic;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionTemplateItemEditorViewModel
    : ObservableObject
{
    public InspectionTemplateItemEditorViewModel(
        Guid id,
        int displayOrder,
        string itemName,
        InspectionInputType inputType,
        string? unit,
        double? minimumValue,
        double? maximumValue,
        bool isRequired,
        bool isActive)
    {
        Id = id;
        DisplayOrder = displayOrder;
        ItemName = itemName;
        InputTypeName =
            ConvertInputTypeToName(inputType);
        Unit = unit;
        MinimumValue = minimumValue;
        MaximumValue = maximumValue;
        IsRequired = isRequired;
        IsActive = isActive;
    }

    public Guid Id { get; }

    public IReadOnlyList<string> InputTypeChoices { get; } =
    [
        "正常・異常",
        "実施・未実施",
        "数値",
        "文字入力"
    ];

    [ObservableProperty]
    private int displayOrder;

    [ObservableProperty]
    private string itemName = string.Empty;

    [ObservableProperty]
    private string inputTypeName = string.Empty;

    [ObservableProperty]
    private string? unit;

    [ObservableProperty]
    private double? minimumValue;

    [ObservableProperty]
    private double? maximumValue;

    [ObservableProperty]
    private bool isRequired;

    [ObservableProperty]
    private bool isActive;

    public InspectionInputType GetInputType()
    {
        return InputTypeName switch
        {
            "正常・異常" =>
                InspectionInputType.NormalAbnormal,

            "実施・未実施" =>
                InspectionInputType.DoneNotDone,

            "数値" =>
                InspectionInputType.Numeric,

            "文字入力" =>
                InspectionInputType.Text,

            _ => throw new InvalidOperationException(
                $"未対応の入力方式です: {InputTypeName}")
        };
    }

    private static string ConvertInputTypeToName(
        InspectionInputType inputType)
    {
        return inputType switch
        {
            InspectionInputType.NormalAbnormal =>
                "正常・異常",

            InspectionInputType.DoneNotDone =>
                "実施・未実施",

            InspectionInputType.Numeric =>
                "数値",

            InspectionInputType.Text =>
                "文字入力",

            _ => inputType.ToString()
        };
    }
}