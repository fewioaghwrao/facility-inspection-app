using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class ApprovalPendingListItemViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        var inspectionId =
            Guid.NewGuid();

        var scheduledDate =
            new DateOnly(
                2026,
                8,
                19);

        // Act
        var sut =
            new ApprovalPendingListItemViewModel(
                scheduleId,
                inspectionId,
                scheduledDate,
                "第1工場",
                "製造エリア",
                "EQ-001",
                "コンプレッサー",
                "日常点検",
                "点検担当者A",
                5,
                2,
                3,
                _ =>
                {
                });

        // Assert
        Assert.Equal(
            scheduleId,
            sut.ScheduleId);

        Assert.Equal(
            inspectionId,
            sut.InspectionId);

        Assert.Equal(
            scheduledDate,
            sut.ScheduledDate);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "コンプレッサー",
            sut.EquipmentName);

        Assert.Equal(
            5,
            sut.ResultCount);

        Assert.Equal(
            2,
            sut.AbnormalCount);

        Assert.Equal(
            3,
            sut.PhotoCount);
    }


    // ============================================
    // Scheduled Date
    // ============================================

    [Fact]
    public void ScheduledDateText_ReturnsFormattedDate()
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    new DateOnly(
                        2026,
                        8,
                        19));

        // Act
        var actual =
            sut.ScheduledDateText;

        // Assert
        Assert.Equal(
            "2026/08/19",
            actual);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public void OpenDetailCommand_RequestsDetailWithScheduleId()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        Guid? requestedScheduleId =
            null;

        var sut =
            CreateViewModel(
                scheduleId:
                    scheduleId,
                detailRequested:
                    id =>
                        requestedScheduleId = id);

        // Act
        sut.OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            scheduleId,
            requestedScheduleId);
    }


    // ============================================
    // Helper
    // ============================================

    private static ApprovalPendingListItemViewModel
        CreateViewModel(
            Guid? scheduleId = null,
            DateOnly? scheduledDate = null,
            Action<Guid>? detailRequested = null)
    {
        return new ApprovalPendingListItemViewModel(
            scheduleId ??
                Guid.NewGuid(),

            Guid.NewGuid(),

            scheduledDate ??
                new DateOnly(
                    2026,
                    8,
                    19),

            "第1工場",
            "製造エリア",
            "EQ-001",
            "コンプレッサー",
            "日常点検",
            "点検担当者A",
            5,
            2,
            3,

            detailRequested ??
                (_ =>
                {
                }));
    }
}