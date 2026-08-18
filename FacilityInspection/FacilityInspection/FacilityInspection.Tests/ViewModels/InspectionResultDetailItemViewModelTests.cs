using FacilityInspection.Data;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionResultDetailItemViewModelTests
{
    private static readonly Guid
        ResultId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new InspectionResultDetailItemViewModel(
                        null!));


        // Assert
        Assert.Equal(
            "source",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var source =
            CreateData(
                resultId:
                    ResultId,
                displayOrder:
                    2,
                itemName:
                    "吐出圧力",
                inputType:
                    InspectionInputType.Numeric,
                checkValue:
                    null,
                numericValue:
                    0.75m,
                textValue:
                    null,
                unit:
                    "MPa",
                isAbnormal:
                    true,
                comment:
                    "基準値を超過");


        // Act
        var sut =
            new InspectionResultDetailItemViewModel(
                source);


        // Assert
        Assert.Equal(
            ResultId,
            sut.ResultId);

        Assert.Equal(
            2,
            sut.DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            sut.ItemName);

        Assert.Equal(
            InspectionInputType.Numeric,
            sut.InputType);

        Assert.Null(
            sut.CheckValue);

        Assert.Equal(
            0.75m,
            sut.NumericValue);

        Assert.Null(
            sut.TextValue);

        Assert.Equal(
            "MPa",
            sut.Unit);

        Assert.True(
            sut.IsAbnormal);

        Assert.Equal(
            "基準値を超過",
            sut.Comment);
    }


    // ============================================
    // Comment
    // ============================================

    [Fact]
    public void Constructor_WhenCommentIsNull_UsesEmptyString()
    {
        // Arrange
        var source =
            CreateData(
                comment:
                    null);


        // Act
        var sut =
            new InspectionResultDetailItemViewModel(
                source);


        // Assert
        Assert.Equal(
            string.Empty,
            sut.Comment);

        Assert.False(
            sut.HasComment);
    }


    [Theory]
    [InlineData(
        "",
        false)]
    [InlineData(
        "   ",
        false)]
    [InlineData(
        "異音あり",
        true)]
    public void HasComment_ReturnsExpectedValue(
        string comment,
        bool expected)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    comment:
                        comment));


        // Assert
        Assert.Equal(
            expected,
            sut.HasComment);
    }


    // ============================================
    // Input Type Text
    // ============================================

    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        "正常・異常")]
    [InlineData(
        InspectionInputType.DoneNotDone,
        "実施・未実施")]
    [InlineData(
        InspectionInputType.Numeric,
        "数値")]
    [InlineData(
        InspectionInputType.Text,
        "文字")]
    public void InputTypeText_ReturnsExpectedText(
        InspectionInputType inputType,
        string expected)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        inputType));


        // Assert
        Assert.Equal(
            expected,
            sut.InputTypeText);
    }


    // ============================================
    // Normal / Abnormal
    // ============================================

    [Theory]
    [InlineData(
        true,
        "正常")]
    [InlineData(
        false,
        "異常")]
    [InlineData(
        null,
        "未入力")]
    public void ValueText_NormalAbnormal_ReturnsExpectedText(
        bool? checkValue,
        string expected)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType
                            .NormalAbnormal,
                    checkValue:
                        checkValue));


        // Assert
        Assert.Equal(
            expected,
            sut.ValueText);
    }


    // ============================================
    // Done / Not Done
    // ============================================

    [Theory]
    [InlineData(
        true,
        "実施")]
    [InlineData(
        false,
        "未実施")]
    [InlineData(
        null,
        "未入力")]
    public void ValueText_DoneNotDone_ReturnsExpectedText(
        bool? checkValue,
        string expected)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType
                            .DoneNotDone,
                    checkValue:
                        checkValue));


        // Assert
        Assert.Equal(
            expected,
            sut.ValueText);
    }


    // ============================================
    // Numeric
    // ============================================

    [Fact]
    public void ValueText_NumericWithUnit_ReturnsValueAndUnit()
    {
        // Arrange
        var value =
            12.5m;

        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType.Numeric,
                    numericValue:
                        value,
                    unit:
                        "MPa"));


        // Assert
        Assert.Equal(
            $"{value} MPa",
            sut.ValueText);
    }


    [Fact]
    public void ValueText_NumericWithoutUnit_ReturnsValueOnly()
    {
        // Arrange
        var value =
            12.5m;

        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType.Numeric,
                    numericValue:
                        value,
                    unit:
                        null));


        // Assert
        Assert.Equal(
            $"{value}",
            sut.ValueText);
    }


    [Fact]
    public void ValueText_NumericWithoutValue_ReturnsNotEntered()
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType.Numeric,
                    numericValue:
                        null,
                    unit:
                        "MPa"));


        // Assert
        Assert.Equal(
            "未入力",
            sut.ValueText);
    }


    // ============================================
    // Text
    // ============================================

    [Theory]
    [InlineData(
        "異音を確認",
        "異音を確認")]
    [InlineData(
        "",
        "未入力")]
    [InlineData(
        "   ",
        "未入力")]
    public void ValueText_Text_ReturnsExpectedText(
        string textValue,
        string expected)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        InspectionInputType.Text,
                    textValue:
                        textValue));


        // Assert
        Assert.Equal(
            expected,
            sut.ValueText);
    }


    // ============================================
    // Result Status
    // ============================================

    [Theory]
    [InlineData(
        false,
        "正常",
        "#DCFCE7",
        "#15803D")]
    [InlineData(
        true,
        "異常",
        "#FEE2E2",
        "#B91C1C")]
    public void StatusDisplay_ReturnsExpectedValues(
        bool isAbnormal,
        string expectedText,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    isAbnormal:
                        isAbnormal));


        // Assert
        Assert.Equal(
            expectedText,
            sut.ResultStatusText);

        Assert.Equal(
            expectedBackground,
            sut.StatusBackground);

        Assert.Equal(
            expectedForeground,
            sut.StatusForeground);
    }


    // ============================================
    // Unsupported Input Type
    // ============================================

    [Fact]
    public void UnsupportedInputType_UsesFallbackValues()
    {
        // Arrange
        var inputType =
            (InspectionInputType)999;

        var sut =
            new InspectionResultDetailItemViewModel(
                CreateData(
                    inputType:
                        inputType));


        // Assert
        Assert.Equal(
            inputType.ToString(),
            sut.InputTypeText);

        Assert.Equal(
            "-",
            sut.ValueText);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionResultDetailData
        CreateData(
            Guid? resultId = null,
            int displayOrder = 1,
            string itemName =
                "点検項目",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            bool? checkValue = true,
            decimal? numericValue = null,
            string? textValue = null,
            string? unit = null,
            bool isAbnormal = false,
            string? comment = null)
    {
        return new InspectionResultDetailData(
            ResultId:
                resultId ??
                ResultId,

            DisplayOrder:
                displayOrder,

            ItemName:
                itemName,

            InputType:
                inputType,

            CheckValue:
                checkValue,

            NumericValue:
                numericValue,

            TextValue:
                textValue,

            Unit:
                unit,

            IsAbnormal:
                isAbnormal,

            Comment:
                comment);
    }
}