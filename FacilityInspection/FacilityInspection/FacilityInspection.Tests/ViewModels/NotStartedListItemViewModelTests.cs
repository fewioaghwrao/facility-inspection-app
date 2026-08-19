using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class NotStartedListItemViewModelTests
{
    private static readonly Guid
        ScheduleId =
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
                    new NotStartedListItemViewModel(
                        null!,
                        _ =>
                        {
                        }));


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
            CreateData();


        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new NotStartedListItemViewModel(
                        source,
                        null!));


        // Assert
        Assert.Equal(
            "openDetailRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var inspectionId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


        var source =
            CreateData(
                inspectionId:
                    inspectionId);


        // Act
        var sut =
            new NotStartedListItemViewModel(
                source,
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            ScheduleId,
            sut.ScheduleId);

        Assert.Equal(
            inspectionId,
            sut.InspectionId);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                20),
            sut.ScheduledDate);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "コンプレッサー室",
            sut.LocationName);

        Assert.Equal(
            "COMP-01",
            sut.EquipmentCode);

        Assert.Equal(
            "コンプレッサー1号機",
            sut.EquipmentName);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.TemplateName);

        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);
    }


    // ============================================
    // Display
    // ============================================

    [Fact]
    public void DisplayProperties_ReturnExpectedValues()
    {
        // Arrange
        var source =
            CreateData();


        var sut =
            new NotStartedListItemViewModel(
                source,
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            "2026/08/20",
            sut.ScheduledDateText);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationDisplayName);

        Assert.Equal(
            "COMP-01  コンプレッサー1号機",
            sut.EquipmentDisplayName);

        Assert.Equal(
            "未実施",
            sut.StatusText);
    }


    [Theory]
    [InlineData(
        2026,
        1,
        5,
        "2026/01/05")]
    [InlineData(
        2026,
        8,
        20,
        "2026/08/20")]
    [InlineData(
        2026,
        12,
        31,
        "2026/12/31")]
    public void ScheduledDateText_UsesYearMonthDayFormat(
        int year,
        int month,
        int day,
        string expected)
    {
        // Arrange
        var source =
            CreateData(
                scheduledDate:
                    new DateOnly(
                        year,
                        month,
                        day));


        var sut =
            new NotStartedListItemViewModel(
                source,
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            expected,
            sut.ScheduledDateText);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public void OpenDetailCommand_PassesScheduleIdToCallback()
    {
        // Arrange
        Guid?
            capturedScheduleId =
                null;


        var sut =
            new NotStartedListItemViewModel(
                CreateData(),
                scheduleId =>
                    capturedScheduleId =
                        scheduleId);


        // Act
        sut.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            ScheduleId,
            capturedScheduleId);
    }


    [Fact]
    public void OpenDetailCommand_WhenScheduleIdIsEmpty_DoesNotInvokeCallback()
    {
        // Arrange
        var callbackCallCount =
            0;


        var source =
            CreateData(
                scheduleId:
                    Guid.Empty);


        var sut =
            new NotStartedListItemViewModel(
                source,
                _ =>
                {
                    callbackCallCount++;
                });


        // Act
        sut.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            0,
            callbackCallCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static InspectionListData
        CreateData(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            DateOnly? scheduledDate = null)
    {
        return new InspectionListData(
            ScheduleId:
                scheduleId ??
                ScheduleId,

            InspectionId:
                inspectionId,

            ScheduledDate:
                scheduledDate ??
                new DateOnly(
                    2026,
                    8,
                    20),

            FactorySiteName:
                "第1工場",

            LocationName:
                "コンプレッサー室",

            EquipmentCode:
                "COMP-01",

            EquipmentName:
                "コンプレッサー1号機",

            TemplateName:
                "コンプレッサー日常点検",

            OperatorName:
                "点検担当者A",

            Status:
                InspectionStatus.NotStarted,

            ResultCount:
                0,

            AbnormalCount:
                0,

            PhotoCount:
                0);
    }
}