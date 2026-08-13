using FacilityInspection.Data;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Globalization;
using System.Collections.ObjectModel;
using System.Linq;

namespace FacilityInspection.ViewModels;

public sealed class InspectionEntryItemViewModel : ViewModelBase
{
    private bool? _checkValue;
    private string _numericText;
    private string _textValue;
    private string _comment;
    private string? _validationMessage;
    private string? _photoErrorMessage;

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

    public ObservableCollection<InspectionEntryPhotoViewModel>
        Photos
    { get; } = [];

    public bool HasPhotos =>
        Photos.Count > 0;

    public string PhotoCountText =>
        $"{Photos.Count} 枚";

    public string? PhotoErrorMessage
    {
        get => _photoErrorMessage;

        private set
        {
            if (SetProperty(
                    ref _photoErrorMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasPhotoError));
            }
        }
    }

    public bool HasPhotoError =>
        !string.IsNullOrWhiteSpace(
            PhotoErrorMessage);

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

            ClearValidationError();
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

        set
        {
            if (SetProperty(
                    ref _numericText,
                    value ?? string.Empty))
            {
                ClearValidationError();
            }
        }
    }

    public string TextValue
    {
        get => _textValue;

        set
        {
            if (SetProperty(
                    ref _textValue,
                    value ?? string.Empty))
            {
                ClearValidationError();
            }
        }
    }

    public string Comment
    {
        get => _comment;

        set => SetProperty(
            ref _comment,
            value ?? string.Empty);
    }

    public string? ValidationMessage
    {
        get => _validationMessage;

        private set
        {
            if (SetProperty(
                    ref _validationMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasValidationError));
            }
        }
    }

    public bool HasValidationError =>
        !string.IsNullOrWhiteSpace(
            ValidationMessage);

    public bool TryCreateCompletionData(
        out InspectionCompletionItemData data)
    {
        ValidationMessage = null;

        bool? checkValue = null;
        decimal? numericValue = null;
        string? textValue = null;

        switch (InputType)
        {
            case InspectionInputType.NormalAbnormal:
            case InspectionInputType.DoneNotDone:
                checkValue = CheckValue;

                if (IsRequired &&
                    !checkValue.HasValue)
                {
                    return FailValidation(
                        "選択してください。",
                        out data);
                }

                break;

            case InspectionInputType.Numeric:
                var numericText =
                    NumericText.Trim();

                if (string.IsNullOrWhiteSpace(
                        numericText))
                {
                    if (IsRequired)
                    {
                        return FailValidation(
                            "数値を入力してください。",
                            out data);
                    }

                    break;
                }

                if (!TryParseDecimal(
                        numericText,
                        out var parsedValue))
                {
                    return FailValidation(
                        "数値として正しく入力してください。",
                        out data);
                }

                numericValue =
                    parsedValue;

                break;

            case InspectionInputType.Text:
                var normalizedText =
                    TextValue.Trim();

                if (IsRequired &&
                    string.IsNullOrWhiteSpace(
                        normalizedText))
                {
                    return FailValidation(
                        "内容を入力してください。",
                        out data);
                }

                textValue =
                    string.IsNullOrWhiteSpace(
                        normalizedText)
                        ? null
                        : normalizedText;

                break;

            default:
                return FailValidation(
                    "未対応の入力形式です。",
                    out data);
        }

        var photos =
            Photos
                .Select(photo =>
                    photo.ToCompletionData())
                .ToList();

        data =
            new InspectionCompletionItemData(
                TemplateItemId,
                checkValue,
                numericValue,
                textValue,
                NormalizeOptionalText(
                    Comment),
                photos);

        return true;
    }

    public void AddPhoto(
        string fileName,
        string relativePath,
        DateTime capturedAtUtc)
    {
        var photo =
            new InspectionEntryPhotoViewModel(
                fileName,
                relativePath,
                capturedAtUtc,
                RemovePhoto);

        Photos.Add(
            photo);

        OnPropertyChanged(
            nameof(HasPhotos));

        OnPropertyChanged(
            nameof(PhotoCountText));

        ClearPhotoError();
    }

    public void CleanupUnsavedPhotos()
    {
        foreach (var photo in
                 Photos.ToList())
        {
            InspectionPhotoStorage
                .DeleteIfExists(
                    photo.RelativePath);
        }

        Photos.Clear();

        OnPropertyChanged(
            nameof(HasPhotos));

        OnPropertyChanged(
            nameof(PhotoCountText));
    }

    public void SetPhotoError(
        string message)
    {
        PhotoErrorMessage =
            message;
    }

    public void ClearPhotoError()
    {
        PhotoErrorMessage = null;
    }

    public void ClearValidationError()
    {
        ValidationMessage = null;
    }

    private void RemovePhoto(
        InspectionEntryPhotoViewModel photo)
    {
        ArgumentNullException.ThrowIfNull(
            photo);

        InspectionPhotoStorage
            .DeleteIfExists(
                photo.RelativePath);

        Photos.Remove(
            photo);

        OnPropertyChanged(
            nameof(HasPhotos));

        OnPropertyChanged(
            nameof(PhotoCountText));
    }

    private bool FailValidation(
        string message,
        out InspectionCompletionItemData data)
    {
        ValidationMessage =
            message;

        data = null!;

        return false;
    }

    private static bool TryParseDecimal(
        string text,
        out decimal value)
    {
        if (decimal.TryParse(
                text,
                NumberStyles.Number,
                CultureInfo.CurrentCulture,
                out value))
        {
            return true;
        }

        return decimal.TryParse(
            text,
            NumberStyles.Number,
            CultureInfo.InvariantCulture,
            out value);
    }

    private static string? NormalizeOptionalText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
            value)
            ? null
            : value.Trim();
    }

    private string GetUnitSuffix()
    {
        return HasUnit
            ? $" {Unit}"
            : string.Empty;
    }
}
