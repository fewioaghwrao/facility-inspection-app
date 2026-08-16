using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using Xunit;

namespace FacilityInspection.Tests.Domain.Inspections;

public sealed class InspectionResultTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidValues_CreatesInspectionResult()
    {
        // Arrange
        var inspectionId =
            Guid.NewGuid();

        var inspectionTemplateItemId =
            Guid.NewGuid();

        // Act
        var result =
            new InspectionResult(
                inspectionId,
                inspectionTemplateItemId,
                1,
                "吐出圧力",
                InspectionInputType.Numeric,
                "MPa");

        // Assert
        Assert.Equal(
            inspectionId,
            result.InspectionId);

        Assert.Equal(
            inspectionTemplateItemId,
            result.InspectionTemplateItemId);

        Assert.Equal(
            1,
            result.DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            result.ItemName);

        Assert.Equal(
            InspectionInputType.Numeric,
            result.InputType);

        Assert.Equal(
            "MPa",
            result.Unit);

        Assert.Null(
            result.CheckValue);

        Assert.Null(
            result.NumericValue);

        Assert.Null(
            result.TextValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);

        Assert.Empty(
            result.Photos);
    }


    [Fact]
    public void Constructor_WithEmptyInspectionId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionResult(
                    Guid.Empty,
                    Guid.NewGuid(),
                    0,
                    "目視確認",
                    InspectionInputType.NormalAbnormal));

        // Assert
        Assert.Equal(
            "inspectionId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyInspectionTemplateItemId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionResult(
                    Guid.NewGuid(),
                    Guid.Empty,
                    0,
                    "目視確認",
                    InspectionInputType.NormalAbnormal));

        // Assert
        Assert.Equal(
            "inspectionTemplateItemId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNegativeDisplayOrder_ThrowsArgumentOutOfRangeException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentOutOfRangeException>(
                () => new InspectionResult(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    -1,
                    "目視確認",
                    InspectionInputType.NormalAbnormal));

        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithZeroDisplayOrder_Succeeds()
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "目視確認",
                InspectionInputType.NormalAbnormal);

        // Assert
        Assert.Equal(
            0,
            result.DisplayOrder);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceItemName_ThrowsArgumentException(
        string itemName)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionResult(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    0,
                    itemName,
                    InspectionInputType.NormalAbnormal));

        // Assert
        Assert.Equal(
            "itemName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullItemName_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new InspectionResult(
                    Guid.NewGuid(),
                    Guid.NewGuid(),
                    0,
                    null!,
                    InspectionInputType.NormalAbnormal));

        // Assert
        Assert.Equal(
            "itemName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithSpacesAroundItemName_TrimsItemName()
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "  吐出圧力  ",
                InspectionInputType.Numeric);

        // Assert
        Assert.Equal(
            "吐出圧力",
            result.ItemName);
    }


    [Theory]
    [InlineData(InspectionInputType.NormalAbnormal)]
    [InlineData(InspectionInputType.DoneNotDone)]
    [InlineData(InspectionInputType.Numeric)]
    [InlineData(InspectionInputType.Text)]
    public void Constructor_WithInputType_SetsInputType(
        InspectionInputType inputType)
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "テスト項目",
                inputType);

        // Assert
        Assert.Equal(
            inputType,
            result.InputType);
    }


    // ============================================
    // Unit
    // ============================================

    [Fact]
    public void Constructor_WithUnit_SetsUnit()
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "吐出圧力",
                InspectionInputType.Numeric,
                "MPa");

        // Assert
        Assert.Equal(
            "MPa",
            result.Unit);
    }


    [Fact]
    public void Constructor_WithSpacesAroundUnit_TrimsUnit()
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "吐出圧力",
                InspectionInputType.Numeric,
                "  MPa  ");

        // Assert
        Assert.Equal(
            "MPa",
            result.Unit);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyUnit_NormalizesToNull(
        string? unit)
    {
        // Act
        var result =
            new InspectionResult(
                Guid.NewGuid(),
                Guid.NewGuid(),
                0,
                "吐出圧力",
                InspectionInputType.Numeric,
                unit);

        // Assert
        Assert.Null(
            result.Unit);
    }


    // ============================================
    // UpdateResult
    // NormalAbnormal / DoneNotDone
    // ============================================

    [Theory]
    [InlineData(InspectionInputType.NormalAbnormal)]
    [InlineData(InspectionInputType.DoneNotDone)]
    public void UpdateResult_WithBooleanInput_UpdatesCheckValue(
        InspectionInputType inputType)
    {
        // Arrange
        var result =
            CreateResult(
                inputType);

        // Act
        result.UpdateResult(
            checkValue: true,
            numericValue: null,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.True(
            result.CheckValue);

        Assert.Null(
            result.NumericValue);

        Assert.Null(
            result.TextValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    [Theory]
    [InlineData(InspectionInputType.NormalAbnormal)]
    [InlineData(InspectionInputType.DoneNotDone)]
    public void UpdateResult_WithFalseBooleanInput_UpdatesCheckValue(
        InspectionInputType inputType)
    {
        // Arrange
        var result =
            CreateResult(
                inputType);

        // Act
        result.UpdateResult(
            checkValue: false,
            numericValue: null,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.False(
            result.CheckValue);
    }


    // ============================================
    // UpdateResult
    // Numeric
    // ============================================

    [Fact]
    public void UpdateResult_WithNumericValue_UpdatesNumericValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Numeric);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: 0.75m,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            0.75m,
            result.NumericValue);

        Assert.Null(
            result.CheckValue);

        Assert.Null(
            result.TextValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    [Fact]
    public void UpdateResult_WithZeroNumericValue_UpdatesNumericValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Numeric);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: 0m,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            0m,
            result.NumericValue);
    }


    [Fact]
    public void UpdateResult_WithNegativeNumericValue_UpdatesNumericValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Numeric);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: -5.5m,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            -5.5m,
            result.NumericValue);
    }


    // ============================================
    // UpdateResult
    // Text
    // ============================================

    [Fact]
    public void UpdateResult_WithTextValue_UpdatesTextValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Text);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: "異常なし",
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            "異常なし",
            result.TextValue);

        Assert.Null(
            result.CheckValue);

        Assert.Null(
            result.NumericValue);
    }


    [Fact]
    public void UpdateResult_WithSpacesAroundTextValue_TrimsTextValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Text);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: "  異常なし  ",
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            "異常なし",
            result.TextValue);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateResult_WithEmptyTextValue_NormalizesToNull(
        string? textValue)
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Text);

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: textValue,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Null(
            result.TextValue);
    }


    // ============================================
    // IsAbnormal
    // ============================================

    [Fact]
    public void UpdateResult_WithAbnormalTrue_SetsIsAbnormal()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.NormalAbnormal);

        // Act
        result.UpdateResult(
            checkValue: false,
            numericValue: null,
            textValue: null,
            isAbnormal: true,
            comment: "異音あり");

        // Assert
        Assert.True(
            result.IsAbnormal);

        Assert.Equal(
            "異音あり",
            result.Comment);
    }


    [Fact]
    public void UpdateResult_FromAbnormalToNormal_ChangesIsAbnormalToFalse()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.NormalAbnormal);

        result.UpdateResult(
            checkValue: false,
            numericValue: null,
            textValue: null,
            isAbnormal: true,
            comment: "異音あり");

        // Act
        result.UpdateResult(
            checkValue: true,
            numericValue: null,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    // ============================================
    // Comment
    // ============================================

    [Fact]
    public void UpdateResult_WithComment_SetsComment()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.NormalAbnormal);

        // Act
        result.UpdateResult(
            checkValue: false,
            numericValue: null,
            textValue: null,
            isAbnormal: true,
            comment: "ベルト付近から異音あり");

        // Assert
        Assert.Equal(
            "ベルト付近から異音あり",
            result.Comment);
    }


    [Fact]
    public void UpdateResult_WithSpacesAroundComment_TrimsComment()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.NormalAbnormal);

        // Act
        result.UpdateResult(
            checkValue: false,
            numericValue: null,
            textValue: null,
            isAbnormal: true,
            comment: "  ベルト付近から異音あり  ");

        // Assert
        Assert.Equal(
            "ベルト付近から異音あり",
            result.Comment);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void UpdateResult_WithEmptyComment_NormalizesToNull(
        string? comment)
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.NormalAbnormal);

        // Act
        result.UpdateResult(
            checkValue: true,
            numericValue: null,
            textValue: null,
            isAbnormal: false,
            comment: comment);

        // Assert
        Assert.Null(
            result.Comment);
    }


    // ============================================
    // Re-update
    // ============================================

    [Fact]
    public void UpdateResult_WhenCalledAgain_ReplacesPreviousTextValues()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Text);

        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: "異常あり",
            isAbnormal: true,
            comment: "確認してください");

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: "異常なし",
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            "異常なし",
            result.TextValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    [Fact]
    public void UpdateResult_WhenCalledAgain_ReplacesPreviousNumericValue()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Numeric);

        result.UpdateResult(
            checkValue: null,
            numericValue: 10.5m,
            textValue: null,
            isAbnormal: true,
            comment: "上限超過");

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: 8.0m,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Equal(
            8.0m,
            result.NumericValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    [Fact]
    public void UpdateResult_WhenValuesAreCleared_SetsValuesToNull()
    {
        // Arrange
        var result =
            CreateResult(
                InspectionInputType.Text);

        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: "入力済み",
            isAbnormal: true,
            comment: "コメントあり");

        // Act
        result.UpdateResult(
            checkValue: null,
            numericValue: null,
            textValue: null,
            isAbnormal: false,
            comment: null);

        // Assert
        Assert.Null(
            result.CheckValue);

        Assert.Null(
            result.NumericValue);

        Assert.Null(
            result.TextValue);

        Assert.False(
            result.IsAbnormal);

        Assert.Null(
            result.Comment);
    }


    // ============================================
    // Test Helper
    // ============================================

    private static InspectionResult CreateResult(
        InspectionInputType inputType)
    {
        return new InspectionResult(
            Guid.NewGuid(),
            Guid.NewGuid(),
            0,
            "テスト項目",
            inputType);
    }
}