using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionStatusListItemViewModelTests
{
    private static readonly Guid
        ScheduleId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

    private static readonly Guid
        InspectionId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


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
                    new InspectionStatusListItemViewModel(
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
                    new InspectionStatusListItemViewModel(
                        source,
                        null!));


        // Assert
        Assert.Equal(
            "openDetailRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsPropertiesAndDisplayValues()
    {
        // Arrange
        var source =
            CreateData(
                scheduledDate:
                    new DateOnly(
                        2026,
                        8,
                        19),

                factorySiteName:
                    "第1工場",

                locationName:
                    "コンプレッサー室",

                equipmentCode:
                    "EQ-001",

                equipmentName:
                    "コンプレッサー1号",

                templateName:
                    "日常点検",

                operatorName:
                    "点検担当者A",

                status:
                    InspectionStatus.Completed,

                resultCount:
                    5,

                abnormalCount:
                    2,

                photoCount:
                    3);


        // Act
        var sut =
            new InspectionStatusListItemViewModel(
                source,
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            ScheduleId,
            sut.ScheduleId);

        Assert.Equal(
            InspectionId,
            sut.InspectionId);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                19),
            sut.ScheduledDate);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "コンプレッサー室",
            sut.LocationName);

        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "コンプレッサー1号",
            sut.EquipmentName);

        Assert.Equal(
            "日常点検",
            sut.TemplateName);

        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);

        Assert.Equal(
            InspectionStatus.Completed,
            sut.Status);

        Assert.Equal(
            5,
            sut.ResultCount);

        Assert.Equal(
            2,
            sut.AbnormalCount);

        Assert.Equal(
            3,
            sut.PhotoCount);


        Assert.Equal(
            "2026/08/19",
            sut.ScheduledDateText);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationDisplayName);

        Assert.Equal(
            "EQ-001  コンプレッサー1号",
            sut.EquipmentDisplayName);

        Assert.Equal(
            "5項目",
            sut.ResultCountText);

        Assert.Equal(
            "3枚",
            sut.PhotoCountText);
    }


    // ============================================
    // InspectionId
    // ============================================

    [Fact]
    public void Constructor_WhenInspectionIdIsNull_KeepsNull()
    {
        // Arrange
        var source =
            CreateData(
                inspectionId:
                    null,
                hasInspection:
                    false);


        // Act
        var sut =
            new InspectionStatusListItemViewModel(
                source,
                _ =>
                {
                });


        // Assert
        Assert.Null(
            sut.InspectionId);
    }


    // ============================================
    // Abnormal Display
    // ============================================

    [Theory]
    [InlineData(
        0,
        "異常なし",
        "#DCFCE7",
        "#15803D")]
    [InlineData(
        1,
        "異常 1件",
        "#FEE2E2",
        "#B91C1C")]
    [InlineData(
        3,
        "異常 3件",
        "#FEE2E2",
        "#B91C1C")]
    public void AbnormalDisplay_ReturnsExpectedValues(
        int abnormalCount,
        string expectedText,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            new InspectionStatusListItemViewModel(
                CreateData(
                    abnormalCount:
                        abnormalCount),
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            expectedText,
            sut.AbnormalCountText);

        Assert.Equal(
            expectedBackground,
            sut.AbnormalBackground);

        Assert.Equal(
            expectedForeground,
            sut.AbnormalForeground);
    }


    // ============================================
    // Status Display
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "未実施",
        "#F1F5F9",
        "#475569")]
    [InlineData(
        InspectionStatus.InProgress,
        "実施中",
        "#DBEAFE",
        "#1D4ED8")]
    [InlineData(
        InspectionStatus.Completed,
        "完了・承認待ち",
        "#FFEDD5",
        "#C2410C")]
    [InlineData(
        InspectionStatus.Approved,
        "承認済み",
        "#DCFCE7",
        "#15803D")]
    [InlineData(
        InspectionStatus.Returned,
        "差し戻し",
        "#FEE2E2",
        "#B91C1C")]
    public void StatusDisplay_ReturnsExpectedValues(
        InspectionStatus status,
        string expectedText,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            new InspectionStatusListItemViewModel(
                CreateData(
                    status:
                        status),
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            expectedText,
            sut.StatusText);

        Assert.Equal(
            expectedBackground,
            sut.StatusBackground);

        Assert.Equal(
            expectedForeground,
            sut.StatusForeground);
    }


    [Fact]
    public void StatusDisplay_WithUnknownStatus_UsesFallbackValues()
    {
        // Arrange
        var status =
            (InspectionStatus)999;

        var sut =
            new InspectionStatusListItemViewModel(
                CreateData(
                    status:
                        status),
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            status.ToString(),
            sut.StatusText);

        Assert.Equal(
            "#F1F5F9",
            sut.StatusBackground);

        Assert.Equal(
            "#475569",
            sut.StatusForeground);
    }


    // ============================================
    // Open Detail
    // ============================================

    [Fact]
    public void OpenDetailCommand_RequestsDetailWithScheduleId()
    {
        // Arrange
        Guid?
            requestedScheduleId =
                null;

        var sut =
            new InspectionStatusListItemViewModel(
                CreateData(),
                scheduleId =>
                    requestedScheduleId =
                        scheduleId);


        // Act
        sut.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            ScheduleId,
            requestedScheduleId);
    }


    [Fact]
    public void OpenDetailCommand_WhenScheduleIdIsEmpty_DoesNotRequestDetail()
    {
        // Arrange
        var callCount =
            0;

        var sut =
            new InspectionStatusListItemViewModel(
                CreateData(
                    scheduleId:
                        Guid.Empty),
                _ =>
                    callCount++);


        // Act
        sut.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            0,
            callCount);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionListData
        CreateData(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            bool hasInspection = true,
            DateOnly? scheduledDate = null,
            string factorySiteName =
                "第1工場",
            string locationName =
                "設備エリア",
            string equipmentCode =
                "EQ-001",
            string equipmentName =
                "設備1",
            string templateName =
                "日常点検",
            string operatorName =
                "点検担当者A",
            InspectionStatus status =
                InspectionStatus.InProgress,
            int resultCount = 0,
            int abnormalCount = 0,
            int photoCount = 0)
    {
        Guid? actualInspectionId =
            hasInspection
                ? inspectionId ??
                  InspectionId
                : null;


        return new InspectionListData(
            ScheduleId:
                scheduleId ??
                ScheduleId,

            InspectionId:
                actualInspectionId,

            ScheduledDate:
                scheduledDate ??
                new DateOnly(
                    2026,
                    8,
                    19),

            FactorySiteName:
                factorySiteName,

            LocationName:
                locationName,

            EquipmentCode:
                equipmentCode,

            EquipmentName:
                equipmentName,

            TemplateName:
                templateName,

            OperatorName:
                operatorName,

            Status:
                status,

            ResultCount:
                resultCount,

            AbnormalCount:
                abnormalCount,

            PhotoCount:
                photoCount);
    }
}