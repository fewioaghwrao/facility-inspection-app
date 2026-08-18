using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionTemplateListItemViewModelTests
{
    private static readonly Guid
        TemplateId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    private static readonly Guid
        ItemId1 =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEE1");


    private static readonly Guid
        ItemId2 =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEE2");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var items =
            new[]
            {
                CreateItem(
                    ItemId1,
                    1,
                    "吐出圧力"),

                CreateItem(
                    ItemId2,
                    2,
                    "異音確認")
            };


        // Act
        var sut =
            new InspectionTemplateListItemViewModel(
                id:
                    TemplateId,
                name:
                    "コンプレッサー日常点検",
                equipmentTypeName:
                    "エアコンプレッサー",
                version:
                    3,
                isActive:
                    true,
                items:
                    items);


        // Assert
        Assert.Equal(
            TemplateId,
            sut.Id);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.Name);

        Assert.Equal(
            "エアコンプレッサー",
            sut.EquipmentTypeName);

        Assert.Equal(
            3,
            sut.Version);

        Assert.True(
            sut.IsActive);

        Assert.Equal(
            2,
            sut.Items.Count);
    }


    // ============================================
    // Active Display
    // ============================================

    [Theory]
    [InlineData(
        true,
        "有効",
        "無効化",
        "現在このテンプレートは使用できます。")]
    [InlineData(
        false,
        "無効",
        "有効化",
        "現在このテンプレートは使用できません。")]
    public void ActiveDisplay_ReturnsExpectedValues(
        bool isActive,
        string expectedStatus,
        string expectedToggle,
        string expectedDescription)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isActive:
                    isActive);


        // Assert
        Assert.Equal(
            expectedStatus,
            sut.StatusText);

        Assert.Equal(
            expectedToggle,
            sut.ActiveToggleText);

        Assert.Equal(
            expectedDescription,
            sut.ActiveStatusDescription);
    }


    // ============================================
    // Version
    // ============================================

    [Fact]
    public void VersionText_ReturnsExpectedText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                version:
                    5);


        // Assert
        Assert.Equal(
            "バージョン 5",
            sut.VersionText);
    }


    // ============================================
    // Items
    // ============================================

    [Fact]
    public void Constructor_PreservesItemOrder()
    {
        // Arrange
        var first =
            CreateItem(
                ItemId1,
                1,
                "吐出圧力");

        var second =
            CreateItem(
                ItemId2,
                2,
                "異音確認");


        // Act
        var sut =
            CreateViewModel(
                items:
                    [
                        first,
                        second
                    ]);


        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Same(
            first,
            sut.Items[0]);

        Assert.Same(
            second,
            sut.Items[1]);
    }


    [Fact]
    public void Constructor_WithEmptyItems_CreatesEmptyCollection()
    {
        // Act
        var sut =
            CreateViewModel(
                items:
                    []);


        // Assert
        Assert.NotNull(
            sut.Items);

        Assert.Empty(
            sut.Items);
    }


    // ============================================
    // Helpers
    // ============================================

    private static InspectionTemplateListItemViewModel
        CreateViewModel(
            int version = 1,
            bool isActive = true,
            InspectionTemplateItemRowViewModel[]?
                items = null)
    {
        return new InspectionTemplateListItemViewModel(
            id:
                TemplateId,

            name:
                "日常点検テンプレート",

            equipmentTypeName:
                "エアコンプレッサー",

            version:
                version,

            isActive:
                isActive,

            items:
                items ??
                []);
    }


    private static InspectionTemplateItemRowViewModel
        CreateItem(
            Guid id,
            int displayOrder,
            string itemName)
    {
        return new InspectionTemplateItemRowViewModel(
            id:
                id,

            displayOrder:
                displayOrder,

            itemName:
                itemName,

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
    }
}