using FacilityInspection.Data;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionEntryItemViewModelTests
{
    private static readonly Guid
        TemplateItemId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullData_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new InspectionEntryItemViewModel(
                        null!));

        // Assert
        Assert.Equal(
            "data",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var data =
            CreateData(
                displayOrder:
                    2,
                itemName:
                    "吐出圧力",
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "MPa",
                minimumValue:
                    0.5,
                maximumValue:
                    1.0,
                isRequired:
                    true,
                description:
                    "圧力計を確認してください。",
                numericValue:
                    0.75m,
                comment:
                    "正常範囲");


        // Act
        var sut =
            new InspectionEntryItemViewModel(
                data);


        // Assert
        Assert.Equal(
            TemplateItemId,
            sut.TemplateItemId);

        Assert.Equal(
            2,
            sut.DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            sut.ItemName);

        Assert.Equal(
            InspectionInputType.Numeric,
            sut.InputType);

        Assert.Equal(
            "MPa",
            sut.Unit);

        Assert.Equal(
            0.5,
            sut.MinimumValue);

        Assert.Equal(
            1.0,
            sut.MaximumValue);

        Assert.True(
            sut.IsRequired);

        Assert.Equal(
            "圧力計を確認してください。",
            sut.Description);

        Assert.Equal(
            "0.75",
            sut.NumericText);

        Assert.Equal(
            "正常範囲",
            sut.Comment);
    }


    [Fact]
    public void Constructor_WhenNullableTextsAreNull_UsesEmptyStrings()
    {
        // Arrange
        var data =
            CreateData(
                numericValue:
                    null,
                textValue:
                    null,
                comment:
                    null);


        // Act
        var sut =
            new InspectionEntryItemViewModel(
                data);


        // Assert
        Assert.Equal(
            string.Empty,
            sut.NumericText);

        Assert.Equal(
            string.Empty,
            sut.TextValue);

        Assert.Equal(
            string.Empty,
            sut.Comment);
    }


    // ============================================
    // Basic Display
    // ============================================

    [Fact]
    public void DisplayProperties_ReturnExpectedValues()
    {
        // Arrange
        var sut =
            CreateViewModel(
                displayOrder:
                    3,
                isRequired:
                    true,
                description:
                    "確認してください。");


        // Assert
        Assert.Equal(
            "3.",
            sut.OrderText);

        Assert.True(
            sut.HasDescription);

        Assert.Equal(
            "必須",
            sut.RequiredText);
    }


    [Fact]
    public void RequiredText_WhenNotRequired_ReturnsOptional()
    {
        // Arrange
        var sut =
            CreateViewModel(
                isRequired:
                    false);


        // Assert
        Assert.Equal(
            "任意",
            sut.RequiredText);
    }


    // ============================================
    // Input Type
    // ============================================

    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        true,
        false,
        false)]
    [InlineData(
        InspectionInputType.DoneNotDone,
        true,
        false,
        false)]
    [InlineData(
        InspectionInputType.Numeric,
        false,
        true,
        false)]
    [InlineData(
        InspectionInputType.Text,
        false,
        false,
        true)]
    public void InputTypeProperties_ReturnExpectedValues(
        InspectionInputType inputType,
        bool expectedChoice,
        bool expectedNumeric,
        bool expectedText)
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    inputType);


        // Assert
        Assert.Equal(
            expectedChoice,
            sut.IsChoiceInput);

        Assert.Equal(
            expectedNumeric,
            sut.IsNumericInput);

        Assert.Equal(
            expectedText,
            sut.IsTextInput);
    }


    // ============================================
    // Choice Labels
    // ============================================

    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        "正常",
        "異常")]
    [InlineData(
        InspectionInputType.DoneNotDone,
        "実施",
        "未実施")]
    public void ChoiceLabels_ReturnExpectedValues(
        InspectionInputType inputType,
        string expectedPositive,
        string expectedNegative)
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    inputType);


        // Assert
        Assert.Equal(
            expectedPositive,
            sut.PositiveLabel);

        Assert.Equal(
            expectedNegative,
            sut.NegativeLabel);
    }


    // ============================================
    // Unit / Criteria
    // ============================================

    [Fact]
    public void CriteriaText_WithMinimumAndMaximum_ReturnsRange()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "MPa",
                minimumValue:
                    0.5,
                maximumValue:
                    1.0);


        // Assert
        Assert.True(
            sut.HasUnit);

        Assert.True(
            sut.HasCriteria);

        Assert.Equal(
            "基準: 0.5 ～ 1 MPa",
            sut.CriteriaText);
    }


    [Fact]
    public void CriteriaText_WithMinimumOnly_ReturnsMinimumText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "℃",
                minimumValue:
                    10,
                maximumValue:
                    null);


        // Assert
        Assert.True(
            sut.HasCriteria);

        Assert.Equal(
            "基準: 10 以上 ℃",
            sut.CriteriaText);
    }


    [Fact]
    public void CriteriaText_WithMaximumOnly_ReturnsMaximumText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "℃",
                minimumValue:
                    null,
                maximumValue:
                    80);


        // Assert
        Assert.True(
            sut.HasCriteria);

        Assert.Equal(
            "基準: 80 以下 ℃",
            sut.CriteriaText);
    }


    [Fact]
    public void CriteriaText_WithoutCriteria_ReturnsEmptyString()
    {
        // Arrange
        var sut =
            CreateViewModel(
                minimumValue:
                    null,
                maximumValue:
                    null);


        // Assert
        Assert.False(
            sut.HasCriteria);

        Assert.Equal(
            string.Empty,
            sut.CriteriaText);
    }


    // ============================================
    // Choice Selection
    // ============================================

    [Fact]
    public void IsPositiveSelected_WhenSetTrue_SetsCheckValueTrue()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    null);


        // Act
        sut.IsPositiveSelected =
            true;


        // Assert
        Assert.True(
            sut.CheckValue);

        Assert.True(
            sut.IsPositiveSelected);

        Assert.False(
            sut.IsNegativeSelected);
    }


    [Fact]
    public void IsPositiveSelected_WhenSetFalse_ClearsPositiveSelection()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    true);


        // Act
        sut.IsPositiveSelected =
            false;


        // Assert
        Assert.Null(
            sut.CheckValue);

        Assert.False(
            sut.IsPositiveSelected);

        Assert.False(
            sut.IsNegativeSelected);
    }


    [Fact]
    public void IsNegativeSelected_WhenSetTrue_SetsCheckValueFalse()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    null);


        // Act
        sut.IsNegativeSelected =
            true;


        // Assert
        Assert.False(
            sut.CheckValue);

        Assert.False(
            sut.IsPositiveSelected);

        Assert.True(
            sut.IsNegativeSelected);
    }


    [Fact]
    public void IsNegativeSelected_WhenSetFalse_ClearsNegativeSelection()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    false);


        // Act
        sut.IsNegativeSelected =
            false;


        // Assert
        Assert.Null(
            sut.CheckValue);

        Assert.False(
            sut.IsPositiveSelected);

        Assert.False(
            sut.IsNegativeSelected);
    }


    // ============================================
    // Choice Validation
    // ============================================

    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal)]
    [InlineData(
        InspectionInputType.DoneNotDone)]
    public void TryCreateCompletionData_RequiredChoiceWithoutSelection_Fails(
        InspectionInputType inputType)
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    inputType,
                isRequired:
                    true,
                checkValue:
                    null);


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.False(
            result);

        Assert.Null(
            data);

        Assert.True(
            sut.HasValidationError);

        Assert.Equal(
            "選択してください。",
            sut.ValidationMessage);
    }


    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        true)]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        false)]
    [InlineData(
        InspectionInputType.DoneNotDone,
        true)]
    [InlineData(
        InspectionInputType.DoneNotDone,
        false)]
    public void TryCreateCompletionData_ChoiceInput_ReturnsCheckValue(
        InspectionInputType inputType,
        bool checkValue)
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    inputType,
                isRequired:
                    true,
                checkValue:
                    checkValue);


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Equal(
            TemplateItemId,
            data.TemplateItemId);

        Assert.Equal(
            checkValue,
            data.CheckValue);

        Assert.Null(
            data.NumericValue);

        Assert.Null(
            data.TextValue);

        Assert.False(
            sut.HasValidationError);
    }


    [Fact]
    public void TryCreateCompletionData_OptionalChoiceWithoutSelection_Succeeds()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                isRequired:
                    false,
                checkValue:
                    null);


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Null(
            data.CheckValue);

        Assert.Null(
            data.NumericValue);

        Assert.Null(
            data.TextValue);
    }


    // ============================================
    // Numeric Validation
    // ============================================

    [Fact]
    public void TryCreateCompletionData_RequiredNumericWithoutValue_Fails()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    true);

        sut.NumericText =
            "   ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.False(
            result);

        Assert.Null(
            data);

        Assert.True(
            sut.HasValidationError);

        Assert.Equal(
            "数値を入力してください。",
            sut.ValidationMessage);
    }


    [Fact]
    public void TryCreateCompletionData_InvalidNumericValue_Fails()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    true);

        sut.NumericText =
            "ABC";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.False(
            result);

        Assert.Null(
            data);

        Assert.Equal(
            "数値として正しく入力してください。",
            sut.ValidationMessage);

        Assert.True(
            sut.HasValidationError);
    }


    [Fact]
    public void TryCreateCompletionData_ValidNumericValue_ReturnsDecimal()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    true);

        sut.NumericText =
            "12.5";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Equal(
            12.5m,
            data.NumericValue);

        Assert.Null(
            data.CheckValue);

        Assert.Null(
            data.TextValue);

        Assert.False(
            sut.HasValidationError);
    }


    [Fact]
    public void TryCreateCompletionData_OptionalNumericWithoutValue_SucceedsWithNull()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    false);

        sut.NumericText =
            string.Empty;


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Null(
            data.NumericValue);
    }


    // ============================================
    // Text Validation
    // ============================================

    [Fact]
    public void TryCreateCompletionData_RequiredTextWithoutValue_Fails()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Text,
                isRequired:
                    true);

        sut.TextValue =
            "   ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.False(
            result);

        Assert.Null(
            data);

        Assert.True(
            sut.HasValidationError);

        Assert.Equal(
            "内容を入力してください。",
            sut.ValidationMessage);
    }


    [Fact]
    public void TryCreateCompletionData_TextValue_IsTrimmed()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Text,
                isRequired:
                    true);

        sut.TextValue =
            "  異音なし  ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Equal(
            "異音なし",
            data.TextValue);

        Assert.Null(
            data.CheckValue);

        Assert.Null(
            data.NumericValue);
    }


    [Fact]
    public void TryCreateCompletionData_OptionalEmptyText_ReturnsNull()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Text,
                isRequired:
                    false);

        sut.TextValue =
            "   ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Null(
            data.TextValue);
    }


    // ============================================
    // Comment
    // ============================================

    [Fact]
    public void TryCreateCompletionData_Comment_IsTrimmed()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    true);

        sut.Comment =
            "  点検時に清掃実施  ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.Equal(
            "点検時に清掃実施",
            data.Comment);
    }


    [Fact]
    public void TryCreateCompletionData_WhitespaceComment_ReturnsNull()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    true);

        sut.Comment =
            "   ";


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.Null(
            data.Comment);
    }


    // ============================================
    // Unsupported Input Type
    // ============================================

    [Fact]
    public void TryCreateCompletionData_UnsupportedInputType_Fails()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    (InspectionInputType)999);


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.False(
            result);

        Assert.Null(
            data);

        Assert.True(
            sut.HasValidationError);

        Assert.Equal(
            "未対応の入力形式です。",
            sut.ValidationMessage);
    }


    // ============================================
    // Validation Error Clear
    // ============================================

    [Fact]
    public void ClearValidationError_ClearsValidationMessage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    true);

        sut.NumericText =
            string.Empty;

        sut.TryCreateCompletionData(
            out _);

        Assert.True(
            sut.HasValidationError);


        // Act
        sut.ClearValidationError();


        // Assert
        Assert.Null(
            sut.ValidationMessage);

        Assert.False(
            sut.HasValidationError);
    }


    [Fact]
    public void NumericText_WhenChanged_ClearsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Numeric,
                isRequired:
                    true);

        sut.NumericText =
            string.Empty;

        sut.TryCreateCompletionData(
            out _);

        Assert.True(
            sut.HasValidationError);


        // Act
        sut.NumericText =
            "10";


        // Assert
        Assert.False(
            sut.HasValidationError);

        Assert.Null(
            sut.ValidationMessage);
    }


    [Fact]
    public void TextValue_WhenChanged_ClearsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.Text,
                isRequired:
                    true);

        sut.TextValue =
            string.Empty;

        sut.TryCreateCompletionData(
            out _);

        Assert.True(
            sut.HasValidationError);


        // Act
        sut.TextValue =
            "確認済み";


        // Assert
        Assert.False(
            sut.HasValidationError);

        Assert.Null(
            sut.ValidationMessage);
    }


    [Fact]
    public void CheckValue_WhenChanged_ClearsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                isRequired:
                    true,
                checkValue:
                    null);

        sut.TryCreateCompletionData(
            out _);

        Assert.True(
            sut.HasValidationError);


        // Act
        sut.CheckValue =
            true;


        // Assert
        Assert.False(
            sut.HasValidationError);

        Assert.Null(
            sut.ValidationMessage);
    }


    // ============================================
    // Photo
    // ============================================

    [Fact]
    public void AddPhoto_AddsPhotoAndUpdatesPhotoProperties()
    {
        // Arrange
        var capturedAtUtc =
            new DateTime(
                2026,
                8,
                19,
                1,
                30,
                0,
                DateTimeKind.Utc);

        var sut =
            CreateViewModel();


        Assert.Empty(
            sut.Photos);

        Assert.False(
            sut.HasPhotos);

        Assert.Equal(
            "0 枚",
            sut.PhotoCountText);


        // Act
        sut.AddPhoto(
            "pressure.jpg",
            "photos/pressure.jpg",
            capturedAtUtc);


        // Assert
        Assert.Single(
            sut.Photos);

        Assert.True(
            sut.HasPhotos);

        Assert.Equal(
            "1 枚",
            sut.PhotoCountText);


        var photo =
            sut.Photos[0];

        Assert.Equal(
            "pressure.jpg",
            photo.FileName);

        Assert.Equal(
            "photos/pressure.jpg",
            photo.RelativePath);

        Assert.Equal(
            capturedAtUtc,
            photo.CapturedAtUtc);
    }


    [Fact]
    public void AddPhoto_ClearsPreviousPhotoError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.SetPhotoError(
            "写真を追加できませんでした。");

        Assert.True(
            sut.HasPhotoError);


        // Act
        sut.AddPhoto(
            "photo.jpg",
            "photos/photo.jpg",
            DateTime.UtcNow);


        // Assert
        Assert.False(
            sut.HasPhotoError);

        Assert.Null(
            sut.PhotoErrorMessage);
    }


    [Fact]
    public void TryCreateCompletionData_WithPhoto_IncludesPhotoData()
    {
        // Arrange
        var capturedAtUtc =
            new DateTime(
                2026,
                8,
                19,
                2,
                0,
                0,
                DateTimeKind.Utc);


        var sut =
            CreateViewModel(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    true);


        sut.AddPhoto(
            "photo.jpg",
            "photos/photo.jpg",
            capturedAtUtc);


        // Act
        var result =
            sut.TryCreateCompletionData(
                out var data);


        // Assert
        Assert.True(
            result);

        Assert.NotNull(
            data);

        Assert.Single(
            data.Photos);

        Assert.Equal(
            "photos/photo.jpg",
            data.Photos[0].RelativePath);

        Assert.Equal(
            capturedAtUtc,
            data.Photos[0].CapturedAtUtc);
    }


    // ============================================
    // Photo Error
    // ============================================

    [Fact]
    public void SetPhotoError_SetsPhotoError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.SetPhotoError(
            "写真ファイルを保存できませんでした。");


        // Assert
        Assert.True(
            sut.HasPhotoError);

        Assert.Equal(
            "写真ファイルを保存できませんでした。",
            sut.PhotoErrorMessage);
    }


    [Fact]
    public void ClearPhotoError_ClearsPhotoError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.SetPhotoError(
            "写真エラー");

        Assert.True(
            sut.HasPhotoError);


        // Act
        sut.ClearPhotoError();


        // Assert
        Assert.False(
            sut.HasPhotoError);

        Assert.Null(
            sut.PhotoErrorMessage);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionEntryItemViewModel
        CreateViewModel(
            int displayOrder = 1,
            string itemName =
                "点検項目",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = false,
            string? description = null,
            bool? checkValue = null,
            decimal? numericValue = null,
            string? textValue = null,
            string? comment = null)
    {
        return new InspectionEntryItemViewModel(
            CreateData(
                displayOrder:
                    displayOrder,
                itemName:
                    itemName,
                inputType:
                    inputType,
                unit:
                    unit,
                minimumValue:
                    minimumValue,
                maximumValue:
                    maximumValue,
                isRequired:
                    isRequired,
                description:
                    description,
                checkValue:
                    checkValue,
                numericValue:
                    numericValue,
                textValue:
                    textValue,
                comment:
                    comment));
    }


    private static InspectionEntryItemData
        CreateData(
            Guid? templateItemId = null,
            int displayOrder = 1,
            string itemName =
                "点検項目",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = false,
            string? description = null,
            bool? checkValue = null,
            decimal? numericValue = null,
            string? textValue = null,
            string? comment = null)
    {
        return new InspectionEntryItemData(
            TemplateItemId:
                templateItemId ??
                TemplateItemId,

            DisplayOrder:
                displayOrder,

            ItemName:
                itemName,

            InputType:
                inputType,

            Unit:
                unit,

            MinimumValue:
                minimumValue,

            MaximumValue:
                maximumValue,

            IsRequired:
                isRequired,

            Description:
                description,

            CheckValue:
                checkValue,

            NumericValue:
                numericValue,

            TextValue:
                textValue,

            Comment:
                comment);
    }
}
