using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionTemplateItemRowViewModelTests
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
            new InspectionTemplateItemRowViewModel(
                id:
                    ItemId,
                displayOrder:
                    2,
                itemName:
                    "吐出圧力",
                inputType:
                    InspectionInputType.Numeric,
                inputTypeName:
                    "数値",
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
            InspectionInputType.Numeric,
            sut.InputType);

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
    // Unit
    // ============================================

    [Theory]
    [InlineData(
        "MPa",
        "MPa")]
    [InlineData(
        "",
        "－")]
    [InlineData(
        "   ",
        "－")]
    [InlineData(
        null,
        "－")]
    public void UnitText_ReturnsExpectedText(
        string? unit,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                unit:
                    unit);


        // Assert
        Assert.Equal(
            expected,
            sut.UnitText);
    }


    // ============================================
    // Required
    // ============================================

    [Theory]
    [InlineData(
        true,
        "必須")]
    [InlineData(
        false,
        "任意")]
    public void RequiredText_ReturnsExpectedText(
        bool isRequired,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isRequired:
                    isRequired);


        // Assert
        Assert.Equal(
            expected,
            sut.RequiredText);
    }


    // ============================================
    // Status
    // ============================================

    [Theory]
    [InlineData(
        true,
        "有効")]
    [InlineData(
        false,
        "無効")]
    public void StatusText_ReturnsExpectedText(
        bool isActive,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isActive:
                    isActive);


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    // ============================================
    // Standard Range
    // ============================================

    [Theory]
    [InlineData(
        null,
        null,
        "－")]
    [InlineData(
        10.0,
        20.0,
        "10 ～ 20")]
    [InlineData(
        10.0,
        null,
        "10以上")]
    [InlineData(
        null,
        20.0,
        "20以下")]
    public void StandardRangeText_ReturnsExpectedText(
        double? minimumValue,
        double? maximumValue,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                minimumValue:
                    minimumValue,
                maximumValue:
                    maximumValue);


        // Assert
        Assert.Equal(
            expected,
            sut.StandardRangeText);
    }


    [Fact]
    public void StandardRangeText_WithZeroValues_TreatsZeroAsSpecifiedValue()
    {
        // Arrange
        var sut =
            CreateViewModel(
                minimumValue:
                    0,
                maximumValue:
                    0);


        // Assert
        Assert.Equal(
            "0 ～ 0",
            sut.StandardRangeText);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionTemplateItemRowViewModel
        CreateViewModel(
            Guid? id = null,
            int displayOrder = 1,
            string itemName =
                "点検項目",
            InspectionInputType inputType =
                InspectionInputType.Numeric,
            string inputTypeName =
                "数値",
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = false,
            bool isActive = true)
    {
        return new InspectionTemplateItemRowViewModel(
            id:
                id ??
                ItemId,

            displayOrder:
                displayOrder,

            itemName:
                itemName,

            inputType:
                inputType,

            inputTypeName:
                inputTypeName,

            unit:
                unit,

            minimumValue:
                minimumValue,

            maximumValue:
                maximumValue,

            isRequired:
                isRequired,

            isActive:
                isActive);
    }
}