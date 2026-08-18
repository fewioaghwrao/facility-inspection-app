using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionTemplateItemEditorViewModelTests
{
    private static readonly Guid
        ItemId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var sut =
            new InspectionTemplateItemEditorViewModel(
                id:
                    ItemId,
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
                isActive:
                    true);


        // Assert
        Assert.Equal(
            ItemId,
            sut.Id);

        Assert.Equal(
            2,
            sut.DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            sut.ItemName);

        Assert.Equal(
            "数値",
            sut.InputTypeName);

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

        Assert.True(
            sut.IsActive);
    }


    // ============================================
    // Input Type Choices
    // ============================================

    [Fact]
    public void InputTypeChoices_ContainsExpectedChoices()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Assert
        Assert.Collection(
            sut.InputTypeChoices,

            item =>
                Assert.Equal(
                    "正常・異常",
                    item),

            item =>
                Assert.Equal(
                    "実施・未実施",
                    item),

            item =>
                Assert.Equal(
                    "数値",
                    item),

            item =>
                Assert.Equal(
                    "文字入力",
                    item));
    }


    // ============================================
    // Input Type Conversion
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
        "文字入力")]
    public void Constructor_ConvertsInputTypeToExpectedName(
        InspectionInputType inputType,
        string expectedName)
    {
        // Act
        var sut =
            CreateViewModel(
                inputType:
                    inputType);


        // Assert
        Assert.Equal(
            expectedName,
            sut.InputTypeName);
    }


    [Theory]
    [InlineData(
        "正常・異常",
        InspectionInputType.NormalAbnormal)]
    [InlineData(
        "実施・未実施",
        InspectionInputType.DoneNotDone)]
    [InlineData(
        "数値",
        InspectionInputType.Numeric)]
    [InlineData(
        "文字入力",
        InspectionInputType.Text)]
    public void GetInputType_ReturnsExpectedType(
        string inputTypeName,
        InspectionInputType expected)
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.InputTypeName =
            inputTypeName;


        // Act
        var result =
            sut.GetInputType();


        // Assert
        Assert.Equal(
            expected,
            result);
    }


    [Fact]
    public void GetInputType_WithUnsupportedName_ThrowsInvalidOperationException()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.InputTypeName =
            "未対応入力";


        // Act
        var exception =
            Assert.Throws<
                InvalidOperationException>(
                () =>
                    sut.GetInputType());


        // Assert
        Assert.Equal(
            "未対応の入力方式です: 未対応入力",
            exception.Message);
    }


    // ============================================
    // Display Order
    // ============================================

    [Fact]
    public void SetDisplayOrder_WithValidValue_UpdatesDisplayOrder()
    {
        // Arrange
        var sut =
            CreateViewModel(
                displayOrder:
                    1);


        // Act
        sut.SetDisplayOrder(
            5);


        // Assert
        Assert.Equal(
            5,
            sut.DisplayOrder);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void SetDisplayOrder_WithLessThanOne_ThrowsArgumentOutOfRangeException(
        int displayOrder)
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        var exception =
            Assert.Throws<
                ArgumentOutOfRangeException>(
                () =>
                    sut.SetDisplayOrder(
                        displayOrder));


        // Assert
        Assert.Equal(
            "displayOrder",
            exception.ParamName);
    }


    // ============================================
    // Remove
    // ============================================

    [Fact]
    public void RemoveCommand_RequestsRemovalWithSelf()
    {
        // Arrange
        InspectionTemplateItemEditorViewModel?
            removedItem =
                null;


        var sut =
            CreateViewModel(
                removeRequested:
                    item =>
                        removedItem =
                            item);


        // Act
        sut.RemoveCommand
            .Execute(null);


        // Assert
        Assert.Same(
            sut,
            removedItem);
    }


    [Fact]
    public void RemoveCommand_WithoutCallback_DoesNotThrow()
    {
        // Arrange
        var sut =
            CreateViewModel(
                removeRequested:
                    null);


        // Act
        var exception =
            Record.Exception(
                () =>
                    sut.RemoveCommand
                        .Execute(null));


        // Assert
        Assert.Null(
            exception);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionTemplateItemEditorViewModel
        CreateViewModel(
            Guid? id = null,
            int displayOrder = 1,
            string itemName =
                "点検項目",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = false,
            bool isActive = true,
            Action<
                InspectionTemplateItemEditorViewModel>?
                removeRequested = null)
    {
        return new InspectionTemplateItemEditorViewModel(
            id:
                id ??
                ItemId,

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

            isActive:
                isActive,

            removeRequested:
                removeRequested);
    }
}