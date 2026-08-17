using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AbnormalListItemViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_CopiesSourceValues()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        var inspectionId =
            Guid.NewGuid();

        var resultId =
            Guid.NewGuid();

        var source =
            CreateSource(
                scheduleId: scheduleId,
                inspectionId: inspectionId,
                resultId: resultId,
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue: false,
                comment: "異音あり",
                photoCount: 2);

        // Act
        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Assert
        Assert.Equal(
            scheduleId,
            sut.ScheduleId);

        Assert.Equal(
            inspectionId,
            sut.InspectionId);

        Assert.Equal(
            resultId,
            sut.ResultId);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                18),
            sut.ScheduledDate);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "製造エリア",
            sut.LocationName);

        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "コンプレッサー",
            sut.EquipmentName);

        Assert.Equal(
            "日常点検",
            sut.TemplateName);

        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);

        Assert.Equal(
            InspectionStatus.Completed,
            sut.InspectionStatus);

        Assert.Equal(
            "異音確認",
            sut.ItemName);

        Assert.Equal(
            InspectionInputType.NormalAbnormal,
            sut.InputType);

        Assert.False(
            sut.CheckValue);

        Assert.Null(
            sut.NumericValue);

        Assert.Null(
            sut.TextValue);

        Assert.Null(
            sut.Unit);

        Assert.Equal(
            "異音あり",
            sut.Comment);

        Assert.Equal(
            2,
            sut.PhotoCount);
    }


    [Fact]
    public void Constructor_WithNullSource_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AbnormalListItemViewModel(
                        null!,
                        _ => { }));

        // Assert
        Assert.Equal(
            "source",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullOpenDetailRequested_ThrowsArgumentNullException()
    {
        // Arrange
        var source =
            CreateSource();

        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AbnormalListItemViewModel(
                        source,
                        null!));

        // Assert
        Assert.Equal(
            "openDetailRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullComment_SetsEmptyString()
    {
        // Arrange
        var source =
            CreateSource(
                comment: null);

        // Act
        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Assert
        Assert.Equal(
            string.Empty,
            sut.Comment);

        Assert.False(
            sut.HasComment);
    }


    // ============================================
    // ScheduledDateText
    // ============================================

    [Fact]
    public void ScheduledDateText_ReturnsFormattedDate()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        var actual =
            sut.ScheduledDateText;

        // Assert
        Assert.Equal(
            "2026/08/18",
            actual);
    }


    // ============================================
    // LocationDisplayName
    // ============================================

    [Fact]
    public void LocationDisplayName_ReturnsFactorySiteAndLocation()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        var actual =
            sut.LocationDisplayName;

        // Assert
        Assert.Equal(
            "第1工場 / 製造エリア",
            actual);
    }


    // ============================================
    // EquipmentDisplayName
    // ============================================

    [Fact]
    public void EquipmentDisplayName_ReturnsEquipmentCodeAndName()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        var actual =
            sut.EquipmentDisplayName;

        // Assert
        Assert.Equal(
            "EQ-001  コンプレッサー",
            actual);
    }


    // ============================================
    // PhotoCountText
    // ============================================

    [Theory]
    [InlineData(0, "0枚")]
    [InlineData(1, "1枚")]
    [InlineData(2, "2枚")]
    [InlineData(10, "10枚")]
    public void PhotoCountText_ReturnsFormattedPhotoCount(
        int photoCount,
        string expected)
    {
        // Arrange
        var source =
            CreateSource(
                photoCount: photoCount);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.PhotoCountText;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // HasComment
    // ============================================

    [Theory]
    [InlineData("異音あり", true)]
    [InlineData("確認してください", true)]
    [InlineData("", false)]
    [InlineData(" ", false)]
    [InlineData("   ", false)]
    public void HasComment_ReturnsExpectedValue(
        string comment,
        bool expected)
    {
        // Arrange
        var source =
            CreateSource(
                comment: comment);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.HasComment;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // ValueText
    // NormalAbnormal
    // ============================================

    [Theory]
    [InlineData(true, "正常")]
    [InlineData(false, "異常")]
    [InlineData(null, "未入力")]
    public void ValueText_NormalAbnormal_ReturnsExpectedValue(
        bool? checkValue,
        string expected)
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.NormalAbnormal,
                checkValue:
                    checkValue);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // ValueText
    // DoneNotDone
    // ============================================

    [Theory]
    [InlineData(true, "実施")]
    [InlineData(false, "未実施")]
    [InlineData(null, "未入力")]
    public void ValueText_DoneNotDone_ReturnsExpectedValue(
        bool? checkValue,
        string expected)
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.DoneNotDone,
                checkValue:
                    checkValue);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    // ============================================
    // ValueText
    // Numeric
    // ============================================

    [Fact]
    public void ValueText_Numeric_WithUnit_ReturnsValueAndUnit()
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Numeric,
                numericValue:
                    12m,
                unit:
                    "MPa");

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "12 MPa",
            actual);
    }


    [Fact]
    public void ValueText_Numeric_WithoutUnit_ReturnsValueOnly()
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Numeric,
                numericValue:
                    12m,
                unit:
                    null);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "12",
            actual);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void ValueText_Numeric_WithBlankUnit_ReturnsValueOnly(
        string unit)
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Numeric,
                numericValue:
                    12m,
                unit:
                    unit);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "12",
            actual);
    }


    [Fact]
    public void ValueText_Numeric_WithoutValue_ReturnsNotEntered()
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Numeric,
                numericValue:
                    null,
                unit:
                    "MPa");

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "未入力",
            actual);
    }


    // ============================================
    // ValueText
    // Text
    // ============================================

    [Theory]
    [InlineData("異音あり", "異音あり")]
    [InlineData("要確認", "要確認")]
    [InlineData("", "未入力")]
    [InlineData(" ", "未入力")]
    [InlineData("   ", "未入力")]
    public void ValueText_Text_ReturnsExpectedValue(
        string textValue,
        string expected)
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Text,
                textValue:
                    textValue);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    [Fact]
    public void ValueText_Text_WithNullValue_ReturnsNotEntered()
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    InspectionInputType.Text,
                textValue:
                    null);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "未入力",
            actual);
    }


    // ============================================
    // ValueText
    // Unknown
    // ============================================

    [Fact]
    public void ValueText_WithUnknownInputType_ReturnsHyphen()
    {
        // Arrange
        var source =
            CreateSource(
                inputType:
                    (InspectionInputType)999);

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ => { });

        // Act
        var actual =
            sut.ValueText;

        // Assert
        Assert.Equal(
            "-",
            actual);
    }


    // ============================================
    // OpenDetailCommand
    // ============================================

    [Fact]
    public void OpenDetailCommand_ExecutesCallbackWithScheduleId()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        Guid? requestedScheduleId =
            null;

        var source =
            CreateSource(
                scheduleId:
                    scheduleId);

        var sut =
            new AbnormalListItemViewModel(
                source,
                id =>
                    requestedScheduleId = id);

        // Act
        sut.OpenDetailCommand.Execute(
            null);

        // Assert
        Assert.Equal(
            scheduleId,
            requestedScheduleId);
    }


    [Fact]
    public void OpenDetailCommand_ExecutesCallbackOnce()
    {
        // Arrange
        var callCount =
            0;

        var source =
            CreateSource();

        var sut =
            new AbnormalListItemViewModel(
                source,
                _ =>
                    callCount++);

        // Act
        sut.OpenDetailCommand.Execute(
            null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static AbnormalListItemViewModel
        CreateViewModel()
    {
        return new AbnormalListItemViewModel(
            CreateSource(),
            _ => { });
    }


    private static AbnormalResultListData
        CreateSource(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            Guid? resultId = null,
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            bool? checkValue = false,
            decimal? numericValue = null,
            string? textValue = null,
            string? unit = null,
            string? comment = "異音あり",
            int photoCount = 2)
    {
        return new AbnormalResultListData(
            ScheduleId:
                scheduleId ??
                Guid.NewGuid(),

            InspectionId:
                inspectionId ??
                Guid.NewGuid(),

            ResultId:
                resultId ??
                Guid.NewGuid(),

            ScheduledDate:
                new DateOnly(
                    2026,
                    8,
                    18),

            FactorySiteName:
                "第1工場",

            LocationName:
                "製造エリア",

            EquipmentCode:
                "EQ-001",

            EquipmentName:
                "コンプレッサー",

            TemplateName:
                "日常点検",

            OperatorName:
                "点検担当者A",

            InspectionStatus:
                InspectionStatus.Completed,

            DisplayOrder:
                1,

            ItemName:
                "異音確認",

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

            Comment:
                comment,

            PhotoCount:
                photoCount);
    }
}