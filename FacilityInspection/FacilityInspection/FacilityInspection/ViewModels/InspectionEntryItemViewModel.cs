using FacilityInspection.Data;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Globalization;

namespace FacilityInspection.ViewModels;

public sealed class InspectionEntryItemViewModel : ViewModelBase
{
    private bool? _checkValue;
    private string _numericText;
    private string _textValue;
    private string _comment;

    public InspectionEntryItemViewModel(
        InspectionEntryItemData data)
    {
        ArgumentNullException.ThrowIfNull(data);

        TemplateItemId = data.TemplateItemId;
        DisplayOrder = data.DisplayOrder;
        ItemName = data.ItemName;
        InputType = data.InputType;
        Unit = data.Unit;
        MinimumValue = data.MinimumValue;
        MaximumValue = data.MaximumValue;
        IsRequired = data.IsRequired;
        Description = data.Description;

        _checkValue = data.CheckValue;

        _numericText =
            data.NumericValue?.ToString(
                CultureInfo.InvariantCulture)
            ?? string.Empty;

        _textValue =
            data.TextValue
            ?? string.Empty;

        _comment =
            data.Comment
            ?? string.Empty;
    }

    public Guid TemplateItemId { get; }

    public int DisplayOrder { get; }

    public string ItemName { get; }

    public InspectionInputType InputType { get; }

    public string? Unit { get; }

    public double? MinimumValue { get; }

    public double? MaximumValue { get; }

    public bool IsRequired { get; }

    public string? Description { get; }

    public string OrderText =>
        $"{DisplayOrder}.";

    public bool HasDescription =>
        !string.IsNullOrWhiteSpace(
            Description);

    public string RequiredText =>
        IsRequired
            ? "必須"
            : "任意";

    public bool IsChoiceInput =>
        InputType is
            InspectionInputType.NormalAbnormal or
            InspectionInputType.DoneNotDone;

    public bool IsNumericInput =>
        InputType ==
            InspectionInputType.Numeric;

    public bool IsTextInput =>
        InputType ==
            InspectionInputType.Text;

    public string PositiveLabel =>
        InputType ==
            InspectionInputType.DoneNotDone
            ? "実施"
            : "正常";

    public string NegativeLabel =>
        InputType ==
            InspectionInputType.DoneNotDone
            ? "未実施"
            : "異常";

    public bool HasUnit =>
        !string.IsNullOrWhiteSpace(
            Unit);

    public bool HasCriteria =>
        MinimumValue.HasValue ||
        MaximumValue.HasValue;

    public string CriteriaText
    {
        get
        {
            if (MinimumValue.HasValue &&
                MaximumValue.HasValue)
            {
                return
                    $"基準: {MinimumValue.Value:g} ～ " +
                    $"{MaximumValue.Value:g}" +
                    GetUnitSuffix();
            }

            if (MinimumValue.HasValue)
            {
                return
                    $"基準: {MinimumValue.Value:g} 以上" +
                    GetUnitSuffix();
            }

            if (MaximumValue.HasValue)
            {
                return
                    $"基準: {MaximumValue.Value:g} 以下" +
                    GetUnitSuffix();
            }

            return string.Empty;
        }
    }

    public bool? CheckValue
    {
        get => _checkValue;

        set
        {
            if (!SetProperty(
                    ref _checkValue,
                    value))
            {
                return;
            }

            OnPropertyChanged(
                nameof(IsPositiveSelected));

            OnPropertyChanged(
                nameof(IsNegativeSelected));
        }
    }

    public bool IsPositiveSelected
    {
        get => CheckValue == true;

        set
        {
            if (value)
            {
                CheckValue = true;
            }
            else if (CheckValue == true)
            {
                CheckValue = null;
            }
        }
    }

    public bool IsNegativeSelected
    {
        get => CheckValue == false;

        set
        {
            if (value)
            {
                CheckValue = false;
            }
            else if (CheckValue == false)
            {
                CheckValue = null;
            }
        }
    }

    public string NumericText
    {
        get => _numericText;

        set => SetProperty(
            ref _numericText,
            value ?? string.Empty);
    }

    public string TextValue
    {
        get => _textValue;

        set => SetProperty(
            ref _textValue,
            value ?? string.Empty);
    }

    public string Comment
    {
        get => _comment;

        set => SetProperty(
            ref _comment,
            value ?? string.Empty);
    }

    private string GetUnitSuffix()
    {
        return HasUnit
            ? $" {Unit}"
            : string.Empty;
    }
}
