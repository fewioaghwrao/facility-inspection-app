using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Sites;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Xunit;

using DomainEquipment =
    FacilityInspection.Domain.Equipments.Equipment;

using DomainLocation =
    FacilityInspection.Domain.Locations.Location;

namespace FacilityInspection.Tests.ViewModels;

public sealed class MemberDashboardViewModelTests
{
    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    private static readonly DateOnly
        Today =
            new(
                2026,
                8,
                20);


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberDashboardViewModel(
                        Guid.Empty,
                        (_, _) =>
                            Task.FromResult<
                                IReadOnlyList<
                                    InspectionSchedule>>(
                                []),
                        (_, _) =>
                            Task.FromResult<
                                IReadOnlyList<
                                    InspectionSchedule>>(
                                []),
                        _ =>
                        {
                        },
                        () =>
                            Today));


        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);

        Assert.Contains(
            "点検担当者IDを指定してください。",
            exception.Message);
    }


    [Fact]
    public void Constructor_WithNullDayLoader_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberDashboardViewModel(
                        OperatorId,
                        null!,
                        (_, _) =>
                            Task.FromResult<
                                IReadOnlyList<
                                    InspectionSchedule>>(
                                []),
                        _ =>
                        {
                        },
                        () =>
                            Today));


        // Assert
        Assert.Equal(
            "getDayForOperatorAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_UsesClockAndDoesNotAutoLoad()
    {
        // Arrange
        var dayCallCount =
            0;

        var monthCallCount =
            0;


        // Act
        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                    {
                        dayCallCount++;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (_, _) =>
                    {
                        monthCallCount++;

                        return EmptySchedules();
                    });


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                8,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            Today,
            sut.SelectedDate);

        Assert.Equal(
            "2026年8月",
            sut.MonthTitle);

        Assert.Equal(
            "2026年8月20日の点検予定",
            sut.SelectedDateTitle);

        Assert.Equal(
            "点検担当者予定",
            sut.Title);

        Assert.Equal(
            "担当する点検予定をカレンダーから確認し、点検を開始します。",
            sut.Description);

        Assert.Equal(
            "カレンダーの日付を選択し、点検対象設備から点検を開始してください。",
            sut.InformationMessage);

        Assert.Empty(
            sut.CalendarDays);

        Assert.Empty(
            sut.SelectedDaySchedules);

        Assert.Equal(
            "全 0 件",
            sut.SelectedDayScheduleCountText);

        Assert.Equal(
            0,
            sut.TodayScheduleCount);

        Assert.Equal(
            0,
            sut.InProgressCount);

        Assert.Equal(
            0,
            sut.CompletedCount);

        Assert.Equal(
            0,
            sut.AbnormalityCount);

        Assert.True(
            sut.IsSelectedDayEmpty);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsLoading);

        Assert.Equal(
            0,
            dayCallCount);

        Assert.Equal(
            0,
            monthCallCount);
    }


    // ============================================
    // Initialize
    // ============================================

    [Fact]
    public async Task InitializeAsync_PassesOperatorAndDatesAndBuildsCalendar()
    {
        // Arrange
        Guid?
            capturedDayOperatorId =
                null;

        DateOnly?
            capturedDay =
                null;

        Guid?
            capturedMonthOperatorId =
                null;

        DateOnly?
            capturedMonth =
                null;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (
                        operatorId,
                        date) =>
                    {
                        capturedDayOperatorId =
                            operatorId;

                        capturedDay =
                            date;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (
                        operatorId,
                        month) =>
                    {
                        capturedMonthOperatorId =
                            operatorId;

                        capturedMonth =
                            month;

                        return EmptySchedules();
                    });


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Equal(
            OperatorId,
            capturedDayOperatorId);

        Assert.Equal(
            Today,
            capturedDay);

        Assert.Equal(
            OperatorId,
            capturedMonthOperatorId);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                1),
            capturedMonth);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);

        var selectedDay =
            sut.CalendarDays
                .Single(
                    x =>
                        x.IsSelected);

        Assert.Equal(
            Today,
            selectedDay.Date);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);
    }


    [Fact]
    public async Task InitializeAsync_WhileLoading_SetsIsLoading()
    {
        // Arrange
        var dayCompletionSource =
            new TaskCompletionSource<
                IReadOnlyList<
                    InspectionSchedule>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);

        var monthCompletionSource =
            new TaskCompletionSource<
                IReadOnlyList<
                    InspectionSchedule>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                        dayCompletionSource.Task,

                getMonthForOperatorAsync:
                    (_, _) =>
                        monthCompletionSource.Task);


        // Act
        var task =
            sut.InitializeAsync();


        // Assert - loading
        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsSelectedDayEmpty);


        // Complete
        dayCompletionSource.SetResult(
            []);

        monthCompletionSource.SetResult(
            []);


        await task;


        // Assert - completed
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsSelectedDayEmpty);
    }


    [Fact]
    public async Task InitializeAsync_WhenLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                        Task.FromException<
                            IReadOnlyList<
                                InspectionSchedule>>(
                            new InvalidOperationException(
                                "当日予定読込エラー")));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検予定を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "当日予定読込エラー",
            sut.ErrorMessage);
    }


    [Fact]
    public async Task InitializeAsync_AfterFailureThenSuccess_ClearsPreviousError()
    {
        // Arrange
        var shouldFail =
            true;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                    {
                        if (shouldFail)
                        {
                            return Task.FromException<
                                IReadOnlyList<
                                    InspectionSchedule>>(
                                new InvalidOperationException(
                                    "一時的なエラー"));
                        }

                        return EmptySchedules();
                    });


        await sut.InitializeAsync();


        Assert.True(
            sut.HasError);


        // Act
        shouldFail =
            false;

        await sut.InitializeAsync();


        // Assert
        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    // ============================================
    // Summary
    // ============================================

    [Fact]
    public async Task InitializeAsync_CalculatesTodaySummaryCounts()
    {
        // Arrange
        var notStarted =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-001");

        var inProgress =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-002",
                status:
                    InspectionStatus.InProgress);

        var completed =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-003",
                status:
                    InspectionStatus.Completed);

        var approved =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-004",
                status:
                    InspectionStatus.Approved);


        IReadOnlyList<InspectionSchedule>
            todaySchedules =
            [
                notStarted,
                inProgress,
                completed,
                approved
            ];


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            todaySchedules),

                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            todaySchedules));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Equal(
            4,
            sut.TodayScheduleCount);

        Assert.Equal(
            1,
            sut.InProgressCount);

        Assert.Equal(
            2,
            sut.CompletedCount);
    }


    [Fact]
    public async Task InitializeAsync_CalculatesAbnormalityCount()
    {
        // Arrange
        var first =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-001",
                status:
                    InspectionStatus.InProgress,
                abnormalCount:
                    2,
                normalCount:
                    1);

        var second =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-002",
                status:
                    InspectionStatus.Completed,
                abnormalCount:
                    1,
                normalCount:
                    2);

        var third =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-003");


        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                first,
                second,
                third
            ];


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            schedules),

                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            schedules));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Equal(
            3,
            sut.AbnormalityCount);
    }


    // ============================================
    // Selected Day
    // ============================================

    [Fact]
    public async Task InitializeAsync_BuildsOnlySelectedDateSchedules()
    {
        // Arrange
        var todayFirst =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-001");

        var todaySecond =
            CreateSchedule(
                Today,
                equipmentCode:
                    "EQ-002");

        var tomorrow =
            CreateSchedule(
                Today.AddDays(1),
                equipmentCode:
                    "EQ-003");


        IReadOnlyList<InspectionSchedule>
            monthSchedules =
            [
                todayFirst,
                tomorrow,
                todaySecond
            ];


        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            monthSchedules));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Equal(
            2,
            sut.SelectedDaySchedules.Count);

        Assert.Equal(
            "全 2 件",
            sut.SelectedDayScheduleCountText);

        Assert.False(
            sut.IsSelectedDayEmpty);

        Assert.DoesNotContain(
            sut.SelectedDaySchedules,
            x =>
                x.EquipmentCode ==
                "EQ-003");
    }


    [Fact]
    public async Task SelectedDaySchedules_AreSortedByFactoryLocationAndEquipmentCode()
    {
        // Arrange
        var siteB =
            CreateSchedule(
                Today,
                factorySiteName:
                    "第2工場",
                locationName:
                    "Aエリア",
                equipmentCode:
                    "EQ-001");

        var siteALocationB =
            CreateSchedule(
                Today,
                factorySiteName:
                    "第1工場",
                locationName:
                    "Bエリア",
                equipmentCode:
                    "EQ-010");

        var siteALocationASecond =
            CreateSchedule(
                Today,
                factorySiteName:
                    "第1工場",
                locationName:
                    "Aエリア",
                equipmentCode:
                    "EQ-002");

        var siteALocationAFirst =
            CreateSchedule(
                Today,
                factorySiteName:
                    "第1工場",
                locationName:
                    "Aエリア",
                equipmentCode:
                    "EQ-001");


        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                siteB,
                siteALocationB,
                siteALocationASecond,
                siteALocationAFirst
            ];


        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromResult(
                            schedules));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Collection(
            sut.SelectedDaySchedules,

            item =>
            {
                Assert.Equal(
                    "第1工場",
                    item.FactorySiteName);

                Assert.Equal(
                    "Aエリア",
                    item.LocationName);

                Assert.Equal(
                    "EQ-001",
                    item.EquipmentCode);
            },

            item =>
            {
                Assert.Equal(
                    "第1工場",
                    item.FactorySiteName);

                Assert.Equal(
                    "Aエリア",
                    item.LocationName);

                Assert.Equal(
                    "EQ-002",
                    item.EquipmentCode);
            },

            item =>
            {
                Assert.Equal(
                    "第1工場",
                    item.FactorySiteName);

                Assert.Equal(
                    "Bエリア",
                    item.LocationName);

                Assert.Equal(
                    "EQ-010",
                    item.EquipmentCode);
            },

            item =>
            {
                Assert.Equal(
                    "第2工場",
                    item.FactorySiteName);

                Assert.Equal(
                    "Aエリア",
                    item.LocationName);

                Assert.Equal(
                    "EQ-001",
                    item.EquipmentCode);
            });
    }


    [Fact]
    public async Task SelectedDaySchedule_StartInspectionCommand_InvokesOpenInspection()
    {
        // Arrange
        Guid?
            openedScheduleId =
                null;


        var schedule =
            CreateSchedule(
                Today);


        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionSchedule>>(
                        [
                            schedule
                        ]),

                openInspection:
                    scheduleId =>
                        openedScheduleId =
                            scheduleId);


        await sut.InitializeAsync();


        var item =
            Assert.Single(
                sut.SelectedDaySchedules);


        // Act
        item.StartInspectionCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            schedule.Id,
            openedScheduleId);
    }


    // ============================================
    // Previous Month
    // ============================================

    [Fact]
    public async Task PreviousMonthCommand_MovesToPreviousMonthAndLoadsMonthOnly()
    {
        // Arrange
        var dayCallCount =
            0;

        var monthCallCount =
            0;

        DateOnly?
            loadedMonth =
                null;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                    {
                        dayCallCount++;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (_, month) =>
                    {
                        monthCallCount++;

                        loadedMonth =
                            month;

                        return EmptySchedules();
                    });


        // Act
        await sut.PreviousMonthCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                7,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            new DateOnly(
                2026,
                7,
                20),
            sut.SelectedDate);

        Assert.Equal(
            new DateOnly(
                2026,
                7,
                1),
            loadedMonth);

        Assert.Equal(
            0,
            dayCallCount);

        Assert.Equal(
            1,
            monthCallCount);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    // ============================================
    // Next Month
    // ============================================

    [Fact]
    public async Task NextMonthCommand_MovesToNextMonthAndLoadsMonthOnly()
    {
        // Arrange
        var dayCallCount =
            0;

        var monthCallCount =
            0;

        DateOnly?
            loadedMonth =
                null;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                    {
                        dayCallCount++;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (_, month) =>
                    {
                        monthCallCount++;

                        loadedMonth =
                            month;

                        return EmptySchedules();
                    });


        // Act
        await sut.NextMonthCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                9,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            new DateOnly(
                2026,
                9,
                20),
            sut.SelectedDate);

        Assert.Equal(
            new DateOnly(
                2026,
                9,
                1),
            loadedMonth);

        Assert.Equal(
            0,
            dayCallCount);

        Assert.Equal(
            1,
            monthCallCount);
    }


    // ============================================
    // Month End Clamp
    // ============================================

    [Fact]
    public async Task PreviousMonthCommand_WhenSelectedDayDoesNotExist_ClampsToMonthEnd()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.DisplayedMonth =
            new DateOnly(
                2026,
                3,
                1);

        sut.SelectedDate =
            new DateOnly(
                2026,
                3,
                31);


        // Act
        await sut.PreviousMonthCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                2,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            new DateOnly(
                2026,
                2,
                28),
            sut.SelectedDate);
    }


    // ============================================
    // Go To Today
    // ============================================

    [Fact]
    public async Task GoToTodayCommand_ResetsDateAndReloadsDayAndMonth()
    {
        // Arrange
        var dayCallCount =
            0;

        var monthCallCount =
            0;

        DateOnly?
            loadedDay =
                null;

        DateOnly?
            loadedMonth =
                null;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, date) =>
                    {
                        dayCallCount++;

                        loadedDay =
                            date;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (_, month) =>
                    {
                        monthCallCount++;

                        loadedMonth =
                            month;

                        return EmptySchedules();
                    });


        sut.DisplayedMonth =
            new DateOnly(
                2027,
                1,
                1);

        sut.SelectedDate =
            new DateOnly(
                2027,
                1,
                15);


        // Act
        await sut.GoToTodayCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                8,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            Today,
            sut.SelectedDate);

        Assert.Equal(
            Today,
            loadedDay);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                1),
            loadedMonth);

        Assert.Equal(
            1,
            dayCallCount);

        Assert.Equal(
            1,
            monthCallCount);
    }


    // ============================================
    // Refresh
    // ============================================

    [Fact]
    public async Task RefreshCommand_ReloadsDayAndMonth()
    {
        // Arrange
        var dayCallCount =
            0;

        var monthCallCount =
            0;


        var sut =
            CreateViewModel(
                getDayForOperatorAsync:
                    (_, _) =>
                    {
                        dayCallCount++;

                        return EmptySchedules();
                    },

                getMonthForOperatorAsync:
                    (_, _) =>
                    {
                        monthCallCount++;

                        return EmptySchedules();
                    });


        // Act
        await sut.RefreshCommand
            .ExecuteAsync(null);

        await sut.RefreshCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            dayCallCount);

        Assert.Equal(
            2,
            monthCallCount);
    }


    // ============================================
    // Select Same Month
    // ============================================

    [Fact]
    public async Task SelectCalendarDayAsync_SameMonth_ChangesSelectionWithoutReloadingMonth()
    {
        // Arrange
        var monthCallCount =
            0;


        var selectedDate =
            new DateOnly(
                2026,
                8,
                25);


        var schedule =
            CreateSchedule(
                selectedDate,
                equipmentCode:
                    "EQ-025");


        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, _) =>
                    {
                        monthCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<
                                InspectionSchedule>>(
                        [
                            schedule
                        ]);
                    });


        await sut.InitializeAsync();


        Assert.Equal(
            1,
            monthCallCount);


        var calendarDay =
            sut.CalendarDays
                .Single(
                    x =>
                        x.Date ==
                        selectedDate);


        // Act
        await sut.SelectCalendarDayAsync(
            calendarDay);


        // Assert
        Assert.Equal(
            selectedDate,
            sut.SelectedDate);

        Assert.Equal(
            1,
            monthCallCount);

        Assert.True(
            calendarDay.IsSelected);

        Assert.Single(
            sut.SelectedDaySchedules);

        Assert.Equal(
            "EQ-025",
            sut.SelectedDaySchedules[0]
                .EquipmentCode);

        Assert.Single(
            sut.CalendarDays
                .Where(
                    x =>
                        x.IsSelected));
    }


    // ============================================
    // Select Outside Month
    // ============================================

    [Fact]
    public async Task SelectCalendarDayAsync_OutsideMonth_MovesMonthAndReloads()
    {
        // Arrange
        var monthCallCount =
            0;

        DateOnly?
            latestLoadedMonth =
                null;


        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, month) =>
                    {
                        monthCallCount++;

                        latestLoadedMonth =
                            month;

                        return EmptySchedules();
                    });


        await sut.InitializeAsync();


        Assert.Equal(
            1,
            monthCallCount);


        var outsideDay =
            sut.CalendarDays
                .First(
                    x =>
                        x.Date.Month !=
                            sut.DisplayedMonth.Month ||
                        x.Date.Year !=
                            sut.DisplayedMonth.Year);


        var expectedMonth =
            new DateOnly(
                outsideDay.Date.Year,
                outsideDay.Date.Month,
                1);


        // Act
        await sut.SelectCalendarDayAsync(
            outsideDay);


        // Assert
        Assert.Equal(
            expectedMonth,
            sut.DisplayedMonth);

        Assert.Equal(
            outsideDay.Date,
            sut.SelectedDate);

        Assert.Equal(
            expectedMonth,
            latestLoadedMonth);

        Assert.Equal(
            2,
            monthCallCount);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    // ============================================
    // Month Load Error
    // ============================================

    [Fact]
    public async Task NextMonthCommand_WhenMonthLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                getMonthForOperatorAsync:
                    (_, _) =>
                        Task.FromException<
                            IReadOnlyList<
                                InspectionSchedule>>(
                            new InvalidOperationException(
                                "月間予定テストエラー")));


        // Act
        await sut.NextMonthCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "月間の点検予定を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "月間予定テストエラー",
            sut.ErrorMessage);
    }


    // ============================================
    // Empty State
    // ============================================

    [Fact]
    public async Task InitializeAsync_WhenNoSchedules_SelectedDayIsEmpty()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Empty(
            sut.SelectedDaySchedules);

        Assert.Equal(
            "全 0 件",
            sut.SelectedDayScheduleCountText);

        Assert.True(
            sut.IsSelectedDayEmpty);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    // ============================================
    // Clock
    // ============================================

    [Fact]
    public async Task GoToTodayCommand_UsesLatestValueFromClock()
    {
        // Arrange
        var currentDate =
            new DateOnly(
                2026,
                8,
                20);


        var sut =
            CreateViewModel(
                todayProvider:
                    () =>
                        currentDate);


        Assert.Equal(
            new DateOnly(
                2026,
                8,
                20),
            sut.SelectedDate);


        currentDate =
            new DateOnly(
                2026,
                9,
                5);


        // Act
        await sut.GoToTodayCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new DateOnly(
                2026,
                9,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            new DateOnly(
                2026,
                9,
                5),
            sut.SelectedDate);
    }


    // ============================================
    // Helpers
    // ============================================

    private static MemberDashboardViewModel
        CreateViewModel(
            Func<
                Guid,
                DateOnly,
                Task<
                    IReadOnlyList<
                        InspectionSchedule>>>?
                getDayForOperatorAsync = null,

            Func<
                Guid,
                DateOnly,
                Task<
                    IReadOnlyList<
                        InspectionSchedule>>>?
                getMonthForOperatorAsync = null,

            Action<Guid>?
                openInspection = null,

            Func<DateOnly>?
                todayProvider = null)
    {
        return new MemberDashboardViewModel(
            OperatorId,

            getDayForOperatorAsync ??
            ((_, _) =>
                EmptySchedules()),

            getMonthForOperatorAsync ??
            ((_, _) =>
                EmptySchedules()),

            openInspection ??
            (_ =>
            {
            }),

            todayProvider ??
            (() =>
                Today));
    }


    private static Task<
        IReadOnlyList<InspectionSchedule>>
        EmptySchedules()
    {
        return Task.FromResult<
            IReadOnlyList<
                InspectionSchedule>>(
            []);
    }


    // ============================================
    // Schedule Factory
    // ============================================

    private static InspectionSchedule
        CreateSchedule(
            DateOnly scheduledDate,
            string factorySiteName =
                "第1工場",
            string locationName =
                "設備エリア",
            string equipmentCode =
                "EQ-001",
            string equipmentName =
                "設備A",
            string templateName =
                "日常点検",
            string? notes =
                null,
            InspectionStatus status =
                InspectionStatus.NotStarted,
            int abnormalCount =
                0,
            int normalCount =
                0)
    {
        var factorySite =
            new FactorySite(
                code:
                    $"SITE-{Guid.NewGuid():N}"
                        [..12],
                name:
                    factorySiteName);


        var location =
            CreateEfEntity<
                DomainLocation>();


        SetProperty(
            location,
            nameof(
                DomainLocation.Name),
            locationName);

        SetProperty(
            location,
            nameof(
                DomainLocation.FactorySite),
            factorySite);


        var equipment =
            CreateEfEntity<
                DomainEquipment>();


        SetProperty(
            equipment,
            nameof(
                DomainEquipment.EquipmentCode),
            equipmentCode);

        SetProperty(
            equipment,
            nameof(
                DomainEquipment.Name),
            equipmentName);

        SetProperty(
            equipment,
            nameof(
                DomainEquipment.Location),
            location);


        var template =
            new InspectionTemplate
            {
                Name =
                    templateName,

                EquipmentType =
                    EquipmentType.AirCompressor,

                Version =
                    1,

                IsActive =
                    true,

                CreatedAt =
                    new DateTime(
                        2026,
                        8,
                        1,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
            };


        var schedule =
            new InspectionSchedule(
                scheduledDate:
                    scheduledDate,

                equipmentId:
                    equipment.Id,

                inspectionTemplateId:
                    template.Id,

                assignedOperatorId:
                    OperatorId,

                notes:
                    notes);


        SetProperty(
            schedule,
            nameof(
                InspectionSchedule.Equipment),
            equipment);

        SetProperty(
            schedule,
            nameof(
                InspectionSchedule.InspectionTemplate),
            template);


        if (status !=
                InspectionStatus.NotStarted ||
            abnormalCount > 0 ||
            normalCount > 0)
        {
            AttachInspection(
                schedule,
                status,
                abnormalCount,
                normalCount);
        }


        return schedule;
    }


    // ============================================
    // Inspection Factory
    // ============================================

    private static void AttachInspection(
        InspectionSchedule schedule,
        InspectionStatus status,
        int abnormalCount,
        int normalCount)
    {
        var inspection =
            new Inspection(
                schedule.Id);


        var startedAtUtc =
            new DateTime(
                2026,
                8,
                20,
                0,
                0,
                0,
                DateTimeKind.Utc);


        switch (status)
        {
            case InspectionStatus.NotStarted:
                break;


            case InspectionStatus.InProgress:

                inspection.Start(
                    OperatorId,
                    startedAtUtc);

                break;


            case InspectionStatus.Completed:

                inspection.Start(
                    OperatorId,
                    startedAtUtc);

                inspection.Complete(
                    startedAtUtc
                        .AddHours(1));

                break;


            case InspectionStatus.Approved:

                inspection.Start(
                    OperatorId,
                    startedAtUtc);

                inspection.Complete(
                    startedAtUtc
                        .AddHours(1));

                inspection.Approve(
                    startedAtUtc
                        .AddHours(2));

                break;


            default:

                throw new InvalidOperationException(
                    $"テスト未対応の点検状態です: {status}");
        }


        var displayOrder =
            1;


        for (var index = 0;
             index < abnormalCount;
             index++)
        {
            var result =
                CreateResult(
                    inspection.Id,
                    displayOrder++,
                    isAbnormal:
                        true);


            inspection.Results.Add(
                result);
        }


        for (var index = 0;
             index < normalCount;
             index++)
        {
            var result =
                CreateResult(
                    inspection.Id,
                    displayOrder++,
                    isAbnormal:
                        false);


            inspection.Results.Add(
                result);
        }


        schedule.AttachInspection(
            inspection);
    }


    private static InspectionResult
        CreateResult(
            Guid inspectionId,
            int displayOrder,
            bool isAbnormal)
    {
        var result =
            new InspectionResult(
                inspectionId:
                    inspectionId,

                inspectionTemplateItemId:
                    Guid.NewGuid(),

                displayOrder:
                    displayOrder,

                itemName:
                    $"点検項目{displayOrder}",

                inputType:
                    InspectionInputType.NormalAbnormal);


        result.UpdateResult(
            checkValue:
                !isAbnormal,

            numericValue:
                null,

            textValue:
                null,

            isAbnormal:
                isAbnormal,

            comment:
                null);


        return result;
    }


    // ============================================
    // EF Navigation Helpers
    // ============================================

    /*
     * InspectionSchedule → Equipment
     * Equipment → Location
     * Location → FactorySite
     *
     * のNavigation Propertyはprivate setter。
     *
     * 本番コードへテスト専用setterを追加したくないため、
     * テストデータ構築時だけReflectionを利用する。
     */

    private static T
        CreateEfEntity<T>()
        where T : class
    {
        var instance =
            Activator.CreateInstance(
                typeof(T),
                nonPublic:
                    true);


        return (T)(
            instance ??
            throw new InvalidOperationException(
                $"{typeof(T).Name}を生成できませんでした。"));
    }


    private static void SetProperty(
        object target,
        string propertyName,
        object? value)
    {
        var property =
            target.GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);


        if (property is null)
        {
            throw new InvalidOperationException(
                $"{target.GetType().Name}.{propertyName}" +
                "が見つかりません。");
        }


        property.SetValue(
            target,
            value);
    }
}