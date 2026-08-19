using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class MemberScheduleItemViewModelTests
{
    private static readonly Guid
        ScheduleId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyScheduleId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberScheduleItemViewModel(
                        Guid.Empty,
                        new DateOnly(
                            2026,
                            8,
                            20),
                        "第1工場",
                        "コンプレッサー室",
                        "COMP-01",
                        "コンプレッサー1号機",
                        "日常点検",
                        null,
                        InspectionStatus.NotStarted,
                        _ =>
                        {
                        }));


        // Assert
        Assert.Equal(
            "scheduleId",
            exception.ParamName);

        Assert.Contains(
            "点検予定IDを指定してください。",
            exception.Message);
    }


    [Theory]
    [InlineData(
        "factorySiteName")]
    [InlineData(
        "locationName")]
    [InlineData(
        "equipmentCode")]
    [InlineData(
        "equipmentName")]
    [InlineData(
        "templateName")]
    public void Constructor_WithBlankRequiredText_ThrowsArgumentException(
        string parameterName)
    {
        // Arrange
        var factorySiteName =
            "第1工場";

        var locationName =
            "コンプレッサー室";

        var equipmentCode =
            "COMP-01";

        var equipmentName =
            "コンプレッサー1号機";

        var templateName =
            "日常点検";


        switch (parameterName)
        {
            case "factorySiteName":
                factorySiteName =
                    "   ";
                break;

            case "locationName":
                locationName =
                    "   ";
                break;

            case "equipmentCode":
                equipmentCode =
                    "   ";
                break;

            case "equipmentName":
                equipmentName =
                    "   ";
                break;

            case "templateName":
                templateName =
                    "   ";
                break;
        }


        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberScheduleItemViewModel(
                        ScheduleId,
                        new DateOnly(
                            2026,
                            8,
                            20),
                        factorySiteName,
                        locationName,
                        equipmentCode,
                        equipmentName,
                        templateName,
                        null,
                        InspectionStatus.NotStarted,
                        _ =>
                        {
                        }));


        // Assert
        Assert.Equal(
            parameterName,
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullOpenInspection_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberScheduleItemViewModel(
                        ScheduleId,
                        new DateOnly(
                            2026,
                            8,
                            20),
                        "第1工場",
                        "コンプレッサー室",
                        "COMP-01",
                        "コンプレッサー1号機",
                        "日常点検",
                        null,
                        InspectionStatus.NotStarted,
                        null!));


        // Assert
        Assert.Equal(
            "openInspection",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsPropertiesAndDisplayTexts()
    {
        // Arrange
        var scheduledDate =
            new DateOnly(
                2026,
                8,
                20);


        // Act
        var sut =
            new MemberScheduleItemViewModel(
                ScheduleId,
                scheduledDate,
                "第1工場",
                "コンプレッサー室",
                "COMP-01",
                "コンプレッサー1号機",
                "コンプレッサー日常点検",
                "  異音に注意  ",
                InspectionStatus.NotStarted,
                _ =>
                {
                });


        // Assert
        Assert.Equal(
            ScheduleId,
            sut.ScheduleId);

        Assert.Equal(
            scheduledDate,
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
            "  異音に注意  ",
            sut.Notes);

        Assert.Equal(
            InspectionStatus.NotStarted,
            sut.Status);


        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationDisplay);

        Assert.Equal(
            "点検票: コンプレッサー日常点検",
            sut.TemplateDisplay);

        Assert.Equal(
            "備考: 異音に注意",
            sut.NotesDisplay);
    }


    // ============================================
    // Notes
    // ============================================

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotesDisplay_WhenNotesIsBlank_ReturnsNone(
        string? notes)
    {
        // Arrange
        var sut =
            CreateViewModel(
                notes:
                    notes);


        // Assert
        Assert.Equal(
            "備考: なし",
            sut.NotesDisplay);
    }


    // ============================================
    // Status Text
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "未実施")]
    [InlineData(
        InspectionStatus.InProgress,
        "実施中")]
    [InlineData(
        InspectionStatus.Completed,
        "完了・承認待ち")]
    [InlineData(
        InspectionStatus.Returned,
        "差し戻し")]
    [InlineData(
        InspectionStatus.Approved,
        "承認済み")]
    public void StatusText_ReturnsExpectedText(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                status:
                    status);


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    // ============================================
    // Status Colors
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "#F1F5F9",
        "#475569")]
    [InlineData(
        InspectionStatus.InProgress,
        "#EFF6FF",
        "#1D4ED8")]
    [InlineData(
        InspectionStatus.Completed,
        "#FFF7ED",
        "#C2410C")]
    [InlineData(
        InspectionStatus.Returned,
        "#FEF2F2",
        "#B91C1C")]
    [InlineData(
        InspectionStatus.Approved,
        "#F0FDF4",
        "#15803D")]
    public void StatusColors_ReturnExpectedValues(
        InspectionStatus status,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            CreateViewModel(
                status:
                    status);


        // Assert
        Assert.Equal(
            expectedBackground,
            sut.StatusBackground);

        Assert.Equal(
            expectedForeground,
            sut.StatusForeground);
    }


    // ============================================
    // Action State
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        true,
        "点検する")]
    [InlineData(
        InspectionStatus.InProgress,
        true,
        "点検を再開")]
    [InlineData(
        InspectionStatus.Returned,
        true,
        "修正する")]
    [InlineData(
        InspectionStatus.Completed,
        false,
        "確認")]
    [InlineData(
        InspectionStatus.Approved,
        false,
        "確認")]
    public void ActionState_ReturnsExpectedValues(
        InspectionStatus status,
        bool expectedCanStart,
        string expectedButtonText)
    {
        // Arrange
        var sut =
            CreateViewModel(
                status:
                    status);


        // Assert
        Assert.Equal(
            expectedCanStart,
            sut.CanStartInspection);

        Assert.Equal(
            expectedButtonText,
            sut.ActionButtonText);

        Assert.Equal(
            expectedCanStart,
            sut.StartInspectionCommand
                .CanExecute(null));
    }


    // ============================================
    // Unknown Status
    // ============================================

    [Fact]
    public void UnknownStatus_UsesFallbackValues()
    {
        // Arrange
        var unknownStatus =
            (InspectionStatus)999;


        var sut =
            CreateViewModel(
                status:
                    unknownStatus);


        // Assert
        Assert.Equal(
            "999",
            sut.StatusText);

        Assert.Equal(
            "#F8FAFC",
            sut.StatusBackground);

        Assert.Equal(
            "#475569",
            sut.StatusForeground);

        Assert.False(
            sut.CanStartInspection);

        Assert.Equal(
            "確認",
            sut.ActionButtonText);

        Assert.False(
            sut.StartInspectionCommand
                .CanExecute(null));
    }


    // ============================================
    // Command
    // ============================================

    [Fact]
    public void StartInspectionCommand_WhenExecutable_PassesScheduleIdToCallback()
    {
        // Arrange
        Guid?
            capturedScheduleId =
                null;


        var sut =
            CreateViewModel(
                status:
                    InspectionStatus.NotStarted,

                openInspection:
                    scheduleId =>
                        capturedScheduleId =
                            scheduleId);


        Assert.True(
            sut.StartInspectionCommand
                .CanExecute(null));


        // Act
        sut.StartInspectionCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            ScheduleId,
            capturedScheduleId);
    }


    // ============================================
    // Helpers
    // ============================================

    private static MemberScheduleItemViewModel
        CreateViewModel(
            InspectionStatus status =
                InspectionStatus.NotStarted,
            string? notes = null,
            Action<Guid>? openInspection = null)
    {
        return new MemberScheduleItemViewModel(
            scheduleId:
                ScheduleId,

            scheduledDate:
                new DateOnly(
                    2026,
                    8,
                    20),

            factorySiteName:
                "第1工場",

            locationName:
                "コンプレッサー室",

            equipmentCode:
                "COMP-01",

            equipmentName:
                "コンプレッサー1号機",

            templateName:
                "コンプレッサー日常点検",

            notes:
                notes,

            status:
                status,

            openInspection:
                openInspection ??
                (_ =>
                {
                }));
    }
}