using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class ScheduleListItemViewModelTests
{
    private static readonly Guid ScheduleId =
        Guid.Parse(
            "11111111-1111-1111-1111-111111111111");

    private static readonly Guid FactorySiteId =
        Guid.Parse(
            "22222222-2222-2222-2222-222222222222");

    private static readonly Guid LocationId =
        Guid.Parse(
            "33333333-3333-3333-3333-333333333333");

    private static readonly Guid EquipmentId =
        Guid.Parse(
            "44444444-4444-4444-4444-444444444444");

    private static readonly Guid InspectionTemplateId =
        Guid.Parse(
            "55555555-5555-5555-5555-555555555555");

    private static readonly Guid AssignedOperatorId =
        Guid.Parse(
            "66666666-6666-6666-6666-666666666666");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullEditRequested_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    new ScheduleListItemViewModel(
                        id:
                            ScheduleId,

                        scheduledDate:
                            Today(),

                        factorySiteId:
                            FactorySiteId,

                        locationId:
                            LocationId,

                        equipmentId:
                            EquipmentId,

                        inspectionTemplateId:
                            InspectionTemplateId,

                        assignedOperatorId:
                            AssignedOperatorId,

                        factorySiteName:
                            "第1工場",

                        locationName:
                            "コンプレッサー室",

                        equipmentCode:
                            "COMP-001",

                        equipmentName:
                            "コンプレッサー1号機",

                        templateName:
                            "コンプレッサー日常点検",

                        operatorName:
                            "点検担当者A",

                        notes:
                            null,

                        status:
                            InspectionStatus.NotStarted,

                        isCancelled:
                            false,

                        editRequested:
                            null!,

                        cancelRequested:
                            _ =>
                            {
                            }));


        // Assert
        Assert.Equal(
            "editRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullCancelRequested_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    new ScheduleListItemViewModel(
                        id:
                            ScheduleId,

                        scheduledDate:
                            Today(),

                        factorySiteId:
                            FactorySiteId,

                        locationId:
                            LocationId,

                        equipmentId:
                            EquipmentId,

                        inspectionTemplateId:
                            InspectionTemplateId,

                        assignedOperatorId:
                            AssignedOperatorId,

                        factorySiteName:
                            "第1工場",

                        locationName:
                            "コンプレッサー室",

                        equipmentCode:
                            "COMP-001",

                        equipmentName:
                            "コンプレッサー1号機",

                        templateName:
                            "コンプレッサー日常点検",

                        operatorName:
                            "点検担当者A",

                        notes:
                            null,

                        status:
                            InspectionStatus.NotStarted,

                        isCancelled:
                            false,

                        editRequested:
                            _ =>
                                Task.CompletedTask,

                        cancelRequested:
                            null!));


        // Assert
        Assert.Equal(
            "cancelRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var date =
            Today()
                .AddDays(3);


        // Act
        var sut =
            CreateViewModel(
                scheduledDate:
                    date,

                notes:
                    "月次点検",

                status:
                    InspectionStatus.InProgress,

                isCancelled:
                    true);


        // Assert
        Assert.Equal(
            ScheduleId,
            sut.Id);

        Assert.Equal(
            date,
            sut.ScheduledDate);

        Assert.Equal(
            FactorySiteId,
            sut.FactorySiteId);

        Assert.Equal(
            LocationId,
            sut.LocationId);

        Assert.Equal(
            EquipmentId,
            sut.EquipmentId);

        Assert.Equal(
            InspectionTemplateId,
            sut.InspectionTemplateId);

        Assert.Equal(
            AssignedOperatorId,
            sut.AssignedOperatorId);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "コンプレッサー室",
            sut.LocationName);

        Assert.Equal(
            "COMP-001",
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

        Assert.Equal(
            "月次点検",
            sut.Notes);

        Assert.Equal(
            InspectionStatus.InProgress,
            sut.Status);

        Assert.True(
            sut.IsCancelled);
    }


    // ============================================
    // Display
    // ============================================

    [Fact]
    public void DisplayProperties_ReturnExpectedText()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Assert
        Assert.Equal(
            "COMP-001  コンプレッサー1号機",
            sut.EquipmentDisplayName);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationDisplayName);

        Assert.Equal(
            "点検票：コンプレッサー日常点検",
            sut.TemplateDisplayText);

        Assert.Equal(
            "担当：点検担当者A",
            sut.OperatorDisplayText);
    }


    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void NotesText_WithBlankNotes_ReturnsNoNotes(
        string? notes)
    {
        // Arrange
        var sut =
            CreateViewModel(
                notes:
                    notes);


        // Assert
        Assert.Equal(
            "備考なし",
            sut.NotesText);
    }


    [Fact]
    public void NotesText_WithNotes_ReturnsOriginalNotes()
    {
        // Arrange
        var sut =
            CreateViewModel(
                notes:
                    "異音の有無を重点確認");


        // Assert
        Assert.Equal(
            "異音の有無を重点確認",
            sut.NotesText);
    }


    // ============================================
    // IsOverdue
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        true)]
    [InlineData(
        InspectionStatus.InProgress,
        false,
        false)]
    [InlineData(
        InspectionStatus.Completed,
        false,
        false)]
    [InlineData(
        InspectionStatus.Returned,
        false,
        false)]
    [InlineData(
        InspectionStatus.Approved,
        false,
        false)]
    [InlineData(
        InspectionStatus.NotStarted,
        true,
        false)]
    public void IsOverdue_WithPastDate_ReturnsExpectedValue(
        InspectionStatus status,
        bool isCancelled,
        bool expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today()
                        .AddDays(-1),

                status:
                    status,

                isCancelled:
                    isCancelled);


        // Assert
        Assert.Equal(
            expected,
            sut.IsOverdue);
    }


    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(30)]
    public void IsOverdue_WithTodayOrFutureDate_ReturnsFalse(
        int daysFromToday)
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today()
                        .AddDays(
                            daysFromToday),

                status:
                    InspectionStatus.NotStarted,

                isCancelled:
                    false);


        // Assert
        Assert.False(
            sut.IsOverdue);
    }


    // ============================================
    // CanEdit / CanCancel
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        true)]
    [InlineData(
        InspectionStatus.NotStarted,
        true,
        false)]
    [InlineData(
        InspectionStatus.InProgress,
        false,
        false)]
    [InlineData(
        InspectionStatus.Completed,
        false,
        false)]
    [InlineData(
        InspectionStatus.Returned,
        false,
        false)]
    [InlineData(
        InspectionStatus.Approved,
        false,
        false)]
    public void CanEditAndCanCancel_ReturnExpectedValue(
        InspectionStatus status,
        bool isCancelled,
        bool expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                status:
                    status,

                isCancelled:
                    isCancelled);


        // Assert
        Assert.Equal(
            expected,
            sut.CanEdit);

        Assert.Equal(
            expected,
            sut.CanCancel);
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
    public void StatusText_WithKnownStatus_ReturnsExpectedText(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        /*
         * 期限超過判定に入らないよう、
         * 今日の日付を使用する。
         */
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today(),

                status:
                    status,

                isCancelled:
                    false);


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    [Fact]
    public void StatusText_WithOverdueNotStarted_ReturnsOverdue()
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today()
                        .AddDays(-1),

                status:
                    InspectionStatus.NotStarted,

                isCancelled:
                    false);


        // Assert
        Assert.True(
            sut.IsOverdue);

        Assert.Equal(
            "期限超過",
            sut.StatusText);
    }


    [Fact]
    public void StatusText_WhenCancelled_TakesPriorityOverOverdue()
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today()
                        .AddDays(-10),

                status:
                    InspectionStatus.NotStarted,

                isCancelled:
                    true);


        // Assert
        Assert.False(
            sut.IsOverdue);

        Assert.Equal(
            "取消",
            sut.StatusText);
    }


    // ============================================
    // Status Color
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        0,
        "#F1F5F9",
        "#475569")]
    [InlineData(
        InspectionStatus.InProgress,
        false,
        0,
        "#DBEAFE",
        "#1D4ED8")]
    [InlineData(
        InspectionStatus.Completed,
        false,
        0,
        "#FFEDD5",
        "#C2410C")]
    [InlineData(
        InspectionStatus.Approved,
        false,
        0,
        "#DCFCE7",
        "#15803D")]
    [InlineData(
        InspectionStatus.Returned,
        false,
        0,
        "#FEE2E2",
        "#B91C1C")]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        -1,
        "#FEE2E2",
        "#B91C1C")]
    [InlineData(
        InspectionStatus.NotStarted,
        true,
        -1,
        "#E2E8F0",
        "#64748B")]
    public void StatusColors_ReturnExpectedValues(
        InspectionStatus status,
        bool isCancelled,
        int daysFromToday,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            CreateViewModel(
                scheduledDate:
                    Today()
                        .AddDays(
                            daysFromToday),

                status:
                    status,

                isCancelled:
                    isCancelled);


        // Assert
        Assert.Equal(
            expectedBackground,
            sut.StatusBackground);

        Assert.Equal(
            expectedForeground,
            sut.StatusForeground);
    }


    // ============================================
    // Commands
    // ============================================

    [Fact]
    public async Task EditCommand_PassesOwnInstanceToCallback()
    {
        // Arrange
        ScheduleListItemViewModel?
            capturedItem =
                null;

        var sut =
            CreateViewModel(
                editRequested:
                    item =>
                    {
                        capturedItem =
                            item;

                        return Task.CompletedTask;
                    });


        // Act
        await sut.EditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Same(
            sut,
            capturedItem);
    }


    [Fact]
    public void CancelScheduleCommand_PassesOwnInstanceToCallback()
    {
        // Arrange
        ScheduleListItemViewModel?
            capturedItem =
                null;


        var sut =
            CreateViewModel(
                cancelRequested:
                    item =>
                    {
                        capturedItem =
                            item;
                    });


        // Act
        sut.CancelScheduleCommand
            .Execute(null);


        // Assert
        Assert.Same(
            sut,
            capturedItem);
    }


    // ============================================
    // Helpers
    // ============================================

    private static ScheduleListItemViewModel
        CreateViewModel(
            DateOnly? scheduledDate =
                null,
            string? notes =
                null,
            InspectionStatus status =
                InspectionStatus.NotStarted,
            bool isCancelled =
                false,
            Func<
                ScheduleListItemViewModel,
                Task>?
                editRequested =
                    null,
            Action<
                ScheduleListItemViewModel>?
                cancelRequested =
                    null)
    {
        editRequested ??=
            _ =>
                Task.CompletedTask;

        cancelRequested ??=
            _ =>
            {
            };


        return new ScheduleListItemViewModel(
            id:
                ScheduleId,

            scheduledDate:
                scheduledDate ??
                Today(),

            factorySiteId:
                FactorySiteId,

            locationId:
                LocationId,

            equipmentId:
                EquipmentId,

            inspectionTemplateId:
                InspectionTemplateId,

            assignedOperatorId:
                AssignedOperatorId,

            factorySiteName:
                "第1工場",

            locationName:
                "コンプレッサー室",

            equipmentCode:
                "COMP-001",

            equipmentName:
                "コンプレッサー1号機",

            templateName:
                "コンプレッサー日常点検",

            operatorName:
                "点検担当者A",

            notes:
                notes,

            status:
                status,

            isCancelled:
                isCancelled,

            editRequested:
                editRequested,

            cancelRequested:
                cancelRequested);
    }


    private static DateOnly Today()
    {
        return DateOnly.FromDateTime(
            DateTime.Today);
    }
}