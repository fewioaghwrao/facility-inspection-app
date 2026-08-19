using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class ScheduleCalendarViewModelTests
{
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
    public void Constructor_WithNullRepository_ThrowsArgumentNullException()
    {
        Assert.Throws<
            ArgumentNullException>(
            () =>
                new ScheduleCalendarViewModel(
                    null!));
    }


    [Theory]
    [InlineData("getMonth")]
    [InlineData("getFactorySites")]
    [InlineData("getLocations")]
    [InlineData("getEquipments")]
    [InlineData("getTemplates")]
    [InlineData("getInspectors")]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("cancel")]
    [InlineData("today")]
    public void InternalConstructor_WithNullDependency_ThrowsArgumentNullException(
        string target)
    {
        Func<
            DateOnly,
            Task<IReadOnlyList<InspectionSchedule>>>
            getMonth =
                _ =>
                    EmptySchedules();

        Func<
            Task<IReadOnlyList<FactorySite>>>
            getFactorySites =
                EmptyFactorySites;

        Func<
            Guid,
            Task<IReadOnlyList<Location>>>
            getLocations =
                _ =>
                    EmptyLocations();

        Func<
            Guid,
            Task<IReadOnlyList<Equipment>>>
            getEquipments =
                _ =>
                    EmptyEquipments();

        Func<
            EquipmentType,
            Task<IReadOnlyList<InspectionTemplate>>>
            getTemplates =
                _ =>
                    EmptyTemplates();

        Func<
            Task<IReadOnlyList<Operator>>>
            getInspectors =
                EmptyOperators;

        Func<
            DateOnly,
            Guid,
            Guid,
            Guid,
            string?,
            Task>
            create =
                (
                    _,
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask;

        Func<
            Guid,
            DateOnly,
            Guid,
            Guid,
            Guid,
            string?,
            Task>
            update =
                (
                    _,
                    _,
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask;

        Func<
            Guid,
            Task>
            cancel =
                _ =>
                    Task.CompletedTask;

        Func<DateOnly>
            today =
                () =>
                    Today;


        switch (target)
        {
            case "getMonth":
                getMonth =
                    null!;
                break;

            case "getFactorySites":
                getFactorySites =
                    null!;
                break;

            case "getLocations":
                getLocations =
                    null!;
                break;

            case "getEquipments":
                getEquipments =
                    null!;
                break;

            case "getTemplates":
                getTemplates =
                    null!;
                break;

            case "getInspectors":
                getInspectors =
                    null!;
                break;

            case "create":
                create =
                    null!;
                break;

            case "update":
                update =
                    null!;
                break;

            case "cancel":
                cancel =
                    null!;
                break;

            case "today":
                today =
                    null!;
                break;
        }


        Assert.Throws<
            ArgumentNullException>(
            () =>
                new ScheduleCalendarViewModel(
                    getMonth,
                    getFactorySites,
                    getLocations,
                    getEquipments,
                    getTemplates,
                    getInspectors,
                    create,
                    update,
                    cancel,
                    today));
    }


    [Fact]
    public void InternalConstructor_UsesClockAndDoesNotAutoLoad()
    {
        var loadCallCount =
            0;


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                    {
                        loadCallCount++;

                        return EmptySchedules();
                    });


        Assert.Equal(
            0,
            loadCallCount);

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
            "8月20日の点検予定",
            sut.SelectedDateTitle);

        Assert.Empty(
            sut.CalendarDays);

        Assert.Empty(
            sut.SelectedDaySchedules);

        Assert.True(
            sut.IsSelectedDayEmpty);
    }


    [Fact]
    public void DisplayProperties_ReturnExpectedText()
    {
        var sut =
            CreateViewModel();


        Assert.Equal(
            "点検予定管理",
            sut.Title);

        Assert.Equal(
            "設備の点検予定、点検担当者、実施状況をカレンダーで管理します。",
            sut.Description);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.HasOperationMessage);

        Assert.False(
            sut.HasEditorError);


        sut.IsCreateMode =
            true;


        Assert.Equal(
            "点検予定の新規登録",
            sut.EditorTitle);

        Assert.Equal(
            "予定日、設備、点検票、担当者を選択します。",
            sut.EditorDescription);

        Assert.Equal(
            "登録",
            sut.SaveButtonText);


        sut.IsCreateMode =
            false;


        Assert.Equal(
            "点検予定の編集",
            sut.EditorTitle);

        Assert.Equal(
            "未実施の点検予定を変更します。",
            sut.EditorDescription);

        Assert.Equal(
            "保存",
            sut.SaveButtonText);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadMonthAsync_PassesDisplayedMonthToLoader()
    {
        DateOnly?
            capturedMonth =
                null;


        var sut =
            CreateViewModel(
                getMonthAsync:
                    month =>
                    {
                        capturedMonth =
                            month;

                        return EmptySchedules();
                    });


        sut.DisplayedMonth =
            new DateOnly(
                2026,
                11,
                1);


        await sut.LoadMonthAsync();


        Assert.Equal(
            new DateOnly(
                2026,
                11,
                1),
            capturedMonth);
    }


    [Fact]
    public async Task LoadMonthAsync_BuildsFortyTwoCalendarDays()
    {
        var sut =
            CreateViewModel();


        await sut.LoadMonthAsync();


        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    [Fact]
    public async Task LoadMonthAsync_CalendarStartsOnSundayBeforeMonthStart()
    {
        var sut =
            CreateViewModel();


        await sut.LoadMonthAsync();


        // 2026/08/01 は土曜日。
        // カレンダーは日曜始まりなので 7/26 から。
        Assert.Equal(
            new DateOnly(
                2026,
                7,
                26),
            sut.CalendarDays[0].Date);

        Assert.Equal(
            new DateOnly(
                2026,
                9,
                5),
            sut.CalendarDays[^1].Date);
    }


    [Fact]
    public async Task LoadMonthAsync_MarksSelectedDate()
    {
        var sut =
            CreateViewModel();


        await sut.LoadMonthAsync();


        var selected =
            Assert.Single(
                sut.CalendarDays
                    .Where(
                        x =>
                            x.IsSelected));


        Assert.Equal(
            Today,
            selected.Date);
    }


    [Fact]
    public async Task LoadMonthAsync_BuildsSelectedDaySchedulesSortedByEquipmentCode()
    {
        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                CreateSchedule(
                    Today,
                    equipmentCode:
                        "EQ-030"),

                CreateSchedule(
                    Today,
                    equipmentCode:
                        "EQ-010"),

                CreateSchedule(
                    Today,
                    equipmentCode:
                        "EQ-020")
            ];


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult(
                            schedules));


        await sut.LoadMonthAsync();


        Assert.Equal(
            3,
            sut.SelectedDaySchedules.Count);

        Assert.Equal(
            "EQ-010",
            sut.SelectedDaySchedules[0]
                .EquipmentCode);

        Assert.Equal(
            "EQ-020",
            sut.SelectedDaySchedules[1]
                .EquipmentCode);

        Assert.Equal(
            "EQ-030",
            sut.SelectedDaySchedules[2]
                .EquipmentCode);
    }


    [Fact]
    public async Task LoadMonthAsync_CalendarDayContainsScheduleCount()
    {
        // Arrange
        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                CreateSchedule(
                Today),

            CreateSchedule(
                Today),

            CreateSchedule(
                Today.AddDays(
                    1))
            ];


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult(
                            schedules));


        // Act
        await sut.LoadMonthAsync();


        // Assert
        var day =
            sut.CalendarDays.Single(
                x =>
                    x.Date ==
                    Today);


        Assert.True(
            day.HasSchedules);

        Assert.Contains(
            "2",
            day.ScheduleCountText);
    }


    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        1)]
    [InlineData(
        InspectionStatus.NotStarted,
        true,
        0)]
    [InlineData(
        InspectionStatus.InProgress,
        false,
        0)]
    [InlineData(
        InspectionStatus.Completed,
        false,
        0)]
    public async Task LoadMonthAsync_CalculatesOverdueCount(
        InspectionStatus status,
        bool cancelled,
        int expected)
    {
        var pastDate =
            Today.AddDays(
                -1);


        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                CreateSchedule(
                    pastDate,
                    status:
                        status,
                    cancelled:
                        cancelled)
            ];


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult(
                            schedules));


        await sut.LoadMonthAsync();


        var day =
            sut.CalendarDays.Single(
                x =>
                    x.Date ==
                    pastDate);


        Assert.Equal(
            expected,
            GetProperty<int>(
                day,
                "OverdueCount"));
    }


    [Theory]
    [InlineData(
        InspectionStatus.Completed,
        false,
        1)]
    [InlineData(
        InspectionStatus.Approved,
        false,
        1)]
    [InlineData(
        InspectionStatus.NotStarted,
        false,
        0)]
    [InlineData(
        InspectionStatus.InProgress,
        false,
        0)]
    [InlineData(
        InspectionStatus.Completed,
        true,
        0)]
    public async Task LoadMonthAsync_CalculatesCompletedCount(
        InspectionStatus status,
        bool cancelled,
        int expected)
    {
        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                CreateSchedule(
                    Today,
                    status:
                        status,
                    cancelled:
                        cancelled)
            ];


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult(
                            schedules));


        await sut.LoadMonthAsync();


        var day =
            sut.CalendarDays.Single(
                x =>
                    x.Date ==
                    Today);


        Assert.Equal(
            expected,
            GetProperty<int>(
                day,
                "CompletedCount"));
    }


    [Fact]
    public async Task LoadMonthAsync_WithNoSelectedDaySchedules_IsEmpty()
    {
        var sut =
            CreateViewModel();


        await sut.LoadMonthAsync();


        Assert.Empty(
            sut.SelectedDaySchedules);

        Assert.True(
            sut.IsSelectedDayEmpty);
    }


    [Fact]
    public async Task LoadMonthAsync_WhileLoading_IsNotEmpty()
    {
        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<InspectionSchedule>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        completionSource.Task);


        var task =
            sut.LoadMonthAsync();


        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsSelectedDayEmpty);


        completionSource.SetResult(
            []);


        await task;


        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsSelectedDayEmpty);
    }


    [Fact]
    public async Task LoadMonthAsync_WhileAlreadyLoading_IgnoresSecondRequest()
    {
        var callCount =
            0;


        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<InspectionSchedule>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                    {
                        callCount++;

                        return completionSource.Task;
                    });


        var first =
            sut.LoadMonthAsync();


        var second =
            sut.LoadMonthAsync();


        await second;


        Assert.Equal(
            1,
            callCount);


        completionSource.SetResult(
            []);


        await first;


        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public async Task LoadMonthAsync_WhenLoaderFails_SetsError()
    {
        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromException<
                            IReadOnlyList<InspectionSchedule>>(
                            new InvalidOperationException(
                                "月取得テストエラー")));


        await sut.LoadMonthAsync();


        Assert.True(
            sut.HasError);

        Assert.Contains(
            "点検予定を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "月取得テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task LoadMonthAsync_AfterFailureThenSuccess_ClearsError()
    {
        var shouldFail =
            true;


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                    {
                        if (shouldFail)
                        {
                            return Task.FromException<
                                IReadOnlyList<InspectionSchedule>>(
                                new InvalidOperationException(
                                    "一時エラー"));
                        }


                        return EmptySchedules();
                    });


        await sut.LoadMonthAsync();


        Assert.True(
            sut.HasError);


        shouldFail =
            false;


        await sut.LoadMonthAsync();


        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Equal(
            42,
            sut.CalendarDays.Count);
    }


    // ============================================
    // Month Navigation
    // ============================================

    [Theory]
    [InlineData(
        -1,
        7)]
    [InlineData(
        1,
        9)]
    public async Task MonthCommand_MovesMonthAndPreservesDay(
        int direction,
        int expectedMonth)
    {
        var sut =
            CreateViewModel();


        if (direction < 0)
        {
            await sut.PreviousMonthCommand
                .ExecuteAsync(null);
        }
        else
        {
            await sut.NextMonthCommand
                .ExecuteAsync(null);
        }


        Assert.Equal(
            new DateOnly(
                2026,
                expectedMonth,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            20,
            sut.SelectedDate.Day);
    }


    [Theory]
    [InlineData(
        2026,
        1,
        31,
        2026,
        2,
        28)]
    [InlineData(
        2028,
        1,
        31,
        2028,
        2,
        29)]
    [InlineData(
        2026,
        3,
        31,
        2026,
        2,
        28)]
    public async Task MonthCommand_AdjustsDayToLastDayOfTargetMonth(
        int sourceYear,
        int sourceMonth,
        int sourceDay,
        int expectedYear,
        int expectedMonth,
        int expectedDay)
    {
        var sut =
            CreateViewModel();


        sut.DisplayedMonth =
            new DateOnly(
                sourceYear,
                sourceMonth,
                1);

        sut.SelectedDate =
            new DateOnly(
                sourceYear,
                sourceMonth,
                sourceDay);


        if (sourceMonth ==
            3)
        {
            await sut.PreviousMonthCommand
                .ExecuteAsync(null);
        }
        else
        {
            await sut.NextMonthCommand
                .ExecuteAsync(null);
        }


        Assert.Equal(
            new DateOnly(
                expectedYear,
                expectedMonth,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            new DateOnly(
                expectedYear,
                expectedMonth,
                expectedDay),
            sut.SelectedDate);
    }


    [Fact]
    public async Task GoToTodayCommand_UsesInjectedClock()
    {
        var today =
            new DateOnly(
                2030,
                12,
                25);


        var sut =
            CreateViewModel(
                todayProvider:
                    () =>
                        today);


        sut.DisplayedMonth =
            new DateOnly(
                2026,
                1,
                1);

        sut.SelectedDate =
            new DateOnly(
                2026,
                1,
                10);


        await sut.GoToTodayCommand
            .ExecuteAsync(null);


        Assert.Equal(
            new DateOnly(
                2030,
                12,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            today,
            sut.SelectedDate);
    }


    // ============================================
    // Day Selection
    // ============================================

    [Fact]
    public async Task SelectCalendarDayAsync_SameMonth_ChangesSelectionAndBuildsDaySchedules()
    {
        var targetDate =
            new DateOnly(
                2026,
                8,
                25);


        IReadOnlyList<InspectionSchedule>
            schedules =
            [
                CreateSchedule(
                    targetDate)
            ];


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult(
                            schedules));


        await sut.LoadMonthAsync();


        var day =
            sut.CalendarDays.Single(
                x =>
                    x.Date ==
                    targetDate);


        await sut.SelectCalendarDayAsync(
            day);


        Assert.Equal(
            targetDate,
            sut.SelectedDate);

        Assert.Single(
            sut.SelectedDaySchedules);

        Assert.True(
            day.IsSelected);

        Assert.Single(
            sut.CalendarDays
                .Where(
                    x =>
                        x.IsSelected));
    }


    [Fact]
    public async Task SelectCalendarDayAsync_OtherMonth_MovesMonthAndReloads()
    {
        var callCount =
            0;

        DateOnly?
            capturedMonth =
                null;


        var sut =
            CreateViewModel(
                getMonthAsync:
                    month =>
                    {
                        callCount++;

                        capturedMonth =
                            month;

                        return EmptySchedules();
                    });


        await sut.LoadMonthAsync();


        var otherMonthDay =
            sut.CalendarDays
                .First(
                    x =>
                        x.Date.Month ==
                        7);


        await sut.SelectCalendarDayAsync(
            otherMonthDay);


        Assert.Equal(
            new DateOnly(
                2026,
                7,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            otherMonthDay.Date,
            sut.SelectedDate);

        Assert.Equal(
            new DateOnly(
                2026,
                7,
                1),
            capturedMonth);

        Assert.Equal(
            2,
            callCount);
    }


    // ============================================
    // Create Editor
    // ============================================

    [Fact]
    public async Task OpenCreateEditorCommand_PopulatesOptionsAndOpensEditor()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.IsCreateMode);

        Assert.Null(
            sut.EditingScheduleId);

        Assert.Equal(
            Today,
            DateOnly.FromDateTime(
                sut.EditorScheduledDate!
                    .Value
                    .Date));

        Assert.Equal(
            string.Empty,
            sut.EditorNotes);

        Assert.Single(
            sut.FactorySiteOptions);

        Assert.Single(
            sut.LocationOptions);

        Assert.Single(
            sut.EquipmentOptions);

        Assert.Single(
            sut.TemplateOptions);

        Assert.Single(
            sut.OperatorOptions);

        Assert.Equal(
            data.FactorySite.Id,
            sut.SelectedFactorySite!.Id);

        Assert.Equal(
            data.Location.Id,
            sut.SelectedLocation!.Id);

        Assert.Equal(
            data.Equipment.Id,
            sut.SelectedEquipment!.Id);

        Assert.Equal(
            data.Template.Id,
            sut.SelectedTemplate!.Id);

        Assert.Equal(
            data.Operator.Id,
            sut.SelectedOperator!.Id);
    }


    [Fact]
    public async Task OpenCreateEditorCommand_WhileSaving_DoesNothing()
    {
        var factoryCallCount =
            0;


        var sut =
            CreateViewModel(
                getFactorySitesAsync:
                    () =>
                    {
                        factoryCallCount++;

                        return EmptyFactorySites();
                    });


        sut.IsSaving =
            true;


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        Assert.False(
            sut.IsEditorOpen);

        Assert.Equal(
            0,
            factoryCallCount);
    }


    [Fact]
    public async Task OpenCreateEditorCommand_WhenOptionLoadingFails_SetsError()
    {
        var sut =
            CreateViewModel(
                getFactorySitesAsync:
                    () =>
                        Task.FromException<
                            IReadOnlyList<FactorySite>>(
                            new InvalidOperationException(
                                "選択肢エラー")));


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        Assert.False(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasError);

        Assert.Contains(
            "予定登録画面を準備できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "選択肢エラー",
            sut.ErrorMessage);
    }


    [Fact]
    public async Task CancelEditorCommand_ClosesAndClearsEditor()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        sut.EditorNotes =
            "テスト備考";

        sut.EditorErrorMessage =
            "テストエラー";


        sut.CancelEditorCommand
            .Execute(null);


        Assert.False(
            sut.IsEditorOpen);

        Assert.Null(
            sut.EditorErrorMessage);

        Assert.Null(
            sut.EditingScheduleId);

        Assert.Null(
            sut.EditorScheduledDate);

        Assert.Null(
            sut.SelectedFactorySite);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Null(
            sut.SelectedEquipment);

        Assert.Null(
            sut.SelectedTemplate);

        Assert.Null(
            sut.SelectedOperator);

        Assert.Equal(
            string.Empty,
            sut.EditorNotes);

        Assert.Empty(
            sut.FactorySiteOptions);

        Assert.Empty(
            sut.LocationOptions);

        Assert.Empty(
            sut.EquipmentOptions);

        Assert.Empty(
            sut.TemplateOptions);

        Assert.Empty(
            sut.OperatorOptions);
    }


    [Fact]
    public void CancelEditorCommand_WhileSaving_DoesNothing()
    {
        var sut =
            CreateViewModel();


        sut.IsEditorOpen =
            true;

        sut.EditorNotes =
            "保持";

        sut.IsSaving =
            true;


        sut.CancelEditorCommand
            .Execute(null);


        Assert.True(
            sut.IsEditorOpen);

        Assert.Equal(
            "保持",
            sut.EditorNotes);
    }


    // ============================================
    // Cascading Options
    // ============================================

    [Fact]
    public async Task HandleFactorySiteChangedAsync_LoadsLocationEquipmentAndTemplate()
    {
        var data =
            CreateEditorData();


        Guid capturedFactoryId =
            Guid.Empty;

        Guid capturedLocationId =
            Guid.Empty;

        EquipmentType?
            capturedType =
                null;


        var sut =
            CreateViewModel(
                getLocationsAsync:
                    id =>
                    {
                        capturedFactoryId =
                            id;

                        return Task.FromResult<
                            IReadOnlyList<Location>>(
                        [
                            data.Location
                        ]);
                    },

                getEquipmentsAsync:
                    id =>
                    {
                        capturedLocationId =
                            id;

                        return Task.FromResult<
                            IReadOnlyList<Equipment>>(
                        [
                            data.Equipment
                        ]);
                    },

                getTemplatesAsync:
                    type =>
                    {
                        capturedType =
                            type;

                        return Task.FromResult<
                            IReadOnlyList<InspectionTemplate>>(
                        [
                            data.Template
                        ]);
                    });


        var option =
            new ScheduleSelectionOptionViewModel(
                data.FactorySite.Id,
                data.FactorySite.Name);


        await sut.HandleFactorySiteChangedAsync(
            option);


        Assert.Equal(
            data.FactorySite.Id,
            capturedFactoryId);

        Assert.Equal(
            data.Location.Id,
            capturedLocationId);

        Assert.Equal(
            data.Equipment.EquipmentType,
            capturedType);

        Assert.Equal(
            data.Location.Id,
            sut.SelectedLocation!.Id);

        Assert.Equal(
            data.Equipment.Id,
            sut.SelectedEquipment!.Id);

        Assert.Equal(
            data.Template.Id,
            sut.SelectedTemplate!.Id);
    }


    [Fact]
    public async Task HandleFactorySiteChangedAsync_WithNull_ClearsDependentOptions()
    {
        var sut =
            CreateViewModel();


        sut.LocationOptions.Add(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "Location"));

        sut.EquipmentOptions.Add(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "Equipment",
                EquipmentType.AirCompressor));

        sut.TemplateOptions.Add(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "Template"));


        await sut.HandleFactorySiteChangedAsync(
            null);


        Assert.Empty(
            sut.LocationOptions);

        Assert.Empty(
            sut.EquipmentOptions);

        Assert.Empty(
            sut.TemplateOptions);

        Assert.Null(
            sut.SelectedLocation);

        Assert.Null(
            sut.SelectedEquipment);

        Assert.Null(
            sut.SelectedTemplate);
    }


    [Fact]
    public async Task HandleFactorySiteChangedAsync_WhenLoadingFails_SetsEditorError()
    {
        var sut =
            CreateViewModel(
                getLocationsAsync:
                    _ =>
                        Task.FromException<
                            IReadOnlyList<Location>>(
                            new InvalidOperationException(
                                "場所読込エラー")));


        await sut.HandleFactorySiteChangedAsync(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "工場"));


        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "設備の選択肢を読み込めませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "場所読込エラー",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task HandleLocationChangedAsync_LoadsEquipmentAndTemplate()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModel(
                getEquipmentsAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<Equipment>>(
                        [
                            data.Equipment
                        ]),

                getTemplatesAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionTemplate>>(
                        [
                            data.Template
                        ]));


        await sut.HandleLocationChangedAsync(
            new ScheduleSelectionOptionViewModel(
                data.Location.Id,
                data.Location.Name));


        Assert.Equal(
            data.Equipment.Id,
            sut.SelectedEquipment!.Id);

        Assert.Equal(
            data.Template.Id,
            sut.SelectedTemplate!.Id);
    }


    [Fact]
    public async Task HandleLocationChangedAsync_WhenLoadingFails_SetsEditorError()
    {
        var sut =
            CreateViewModel(
                getEquipmentsAsync:
                    _ =>
                        Task.FromException<
                            IReadOnlyList<Equipment>>(
                            new InvalidOperationException(
                                "設備読込エラー")));


        await sut.HandleLocationChangedAsync(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "場所"));


        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "設備を読み込めませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "設備読込エラー",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task HandleEquipmentChangedAsync_LoadsTemplate()
    {
        var data =
            CreateEditorData();


        EquipmentType?
            capturedType =
                null;


        var sut =
            CreateViewModel(
                getTemplatesAsync:
                    type =>
                    {
                        capturedType =
                            type;

                        return Task.FromResult<
                            IReadOnlyList<InspectionTemplate>>(
                        [
                            data.Template
                        ]);
                    });


        await sut.HandleEquipmentChangedAsync(
            new ScheduleSelectionOptionViewModel(
                data.Equipment.Id,
                data.Equipment.Name,
                data.Equipment.EquipmentType));


        Assert.Equal(
            data.Equipment.EquipmentType,
            capturedType);

        Assert.Equal(
            data.Template.Id,
            sut.SelectedTemplate!.Id);
    }


    [Fact]
    public async Task HandleEquipmentChangedAsync_WhenLoadingFails_SetsEditorError()
    {
        var sut =
            CreateViewModel(
                getTemplatesAsync:
                    _ =>
                        Task.FromException<
                            IReadOnlyList<InspectionTemplate>>(
                            new InvalidOperationException(
                                "テンプレート読込エラー")));


        await sut.HandleEquipmentChangedAsync(
            new ScheduleSelectionOptionViewModel(
                Guid.NewGuid(),
                "設備",
                EquipmentType.AirCompressor));


        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "点検票テンプレートを読み込めませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "テンプレート読込エラー",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Validation
    // ============================================

    [Theory]
    [InlineData(
        "date",
        "点検予定日を選択してください。")]
    [InlineData(
        "factory",
        "工場を選択してください。")]
    [InlineData(
        "location",
        "設置場所を選択してください。")]
    [InlineData(
        "equipment",
        "設備を選択してください。")]
    [InlineData(
        "template",
        "点検票テンプレートを選択してください。")]
    [InlineData(
        "operator",
        "点検担当者を選択してください。")]
    public async Task SaveEditorCommand_WithMissingInput_SetsExpectedError(
        string missing,
        string expected)
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        switch (missing)
        {
            case "date":
                sut.EditorScheduledDate =
                    null;
                break;

            case "factory":
                sut.SelectedFactorySite =
                    null;
                break;

            case "location":
                sut.SelectedLocation =
                    null;
                break;

            case "equipment":
                sut.SelectedEquipment =
                    null;
                break;

            case "template":
                sut.SelectedTemplate =
                    null;
                break;

            case "operator":
                sut.SelectedOperator =
                    null;
                break;
        }


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.Equal(
            expected,
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveEditorCommand_WithPastDate_UsesInjectedClock()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        sut.EditorScheduledDate =
            new DateTimeOffset(
                Today
                    .AddDays(-1)
                    .ToDateTime(
                        TimeOnly.MinValue));


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.Equal(
            "過去の日付は選択できません。",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveEditorCommand_WithNotesOver500Characters_SetsError()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        sut.EditorNotes =
            new string(
                'あ',
                501);


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.Equal(
            "備考は500文字以内で入力してください。",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Create
    // ============================================

    [Fact]
    public async Task SaveEditorCommand_Create_PassesValuesClosesEditorAndReloads()
    {
        var data =
            CreateEditorData();


        DateOnly?
            capturedDate =
                null;

        Guid capturedEquipment =
            Guid.Empty;

        Guid capturedTemplate =
            Guid.Empty;

        Guid capturedOperator =
            Guid.Empty;

        string? capturedNotes =
            null;

        var createCallCount =
            0;

        var loadCallCount =
            0;


        var sut =
            CreateViewModelForEditor(
                data,
                getMonthAsync:
                    _ =>
                    {
                        loadCallCount++;

                        return EmptySchedules();
                    },
                createAsync:
                    (
                        date,
                        equipmentId,
                        templateId,
                        operatorId,
                        notes) =>
                    {
                        createCallCount++;

                        capturedDate =
                            date;

                        capturedEquipment =
                            equipmentId;

                        capturedTemplate =
                            templateId;

                        capturedOperator =
                            operatorId;

                        capturedNotes =
                            notes;


                        return Task.CompletedTask;
                    });


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        var saveDate =
            new DateOnly(
                2026,
                9,
                10);


        sut.EditorScheduledDate =
            new DateTimeOffset(
                saveDate.ToDateTime(
                    TimeOnly.MinValue));

        sut.EditorNotes =
            "月次予定";


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.Equal(
            1,
            createCallCount);

        Assert.Equal(
            saveDate,
            capturedDate);

        Assert.Equal(
            data.Equipment.Id,
            capturedEquipment);

        Assert.Equal(
            data.Template.Id,
            capturedTemplate);

        Assert.Equal(
            data.Operator.Id,
            capturedOperator);

        Assert.Equal(
            "月次予定",
            capturedNotes);


        Assert.Equal(
            "点検予定を登録しました。",
            sut.OperationMessage);

        Assert.False(
            sut.IsEditorOpen);

        Assert.False(
            sut.IsSaving);

        Assert.Null(
            sut.EditorScheduledDate);

        Assert.Equal(
            new DateOnly(
                2026,
                9,
                1),
            sut.DisplayedMonth);

        Assert.Equal(
            saveDate,
            sut.SelectedDate);

        Assert.Equal(
            1,
            loadCallCount);
    }


    [Fact]
    public async Task SaveEditorCommand_WhenCreateFails_SetsEditorError()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data,
                createAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "登録エラー")));


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "点検予定を登録できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "登録エラー",
            sut.EditorErrorMessage);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // Edit
    // ============================================

    [Fact]
    public async Task OpenEditEditorAsync_PopulatesExistingScheduleValues()
    {
        var data =
            CreateEditorData();


        var schedule =
            CreateSchedule(
                Today,
                notes:
                    "既存備考",
                factorySite:
                    data.FactorySite,
                location:
                    data.Location,
                equipment:
                    data.Equipment,
                template:
                    data.Template,
                assignedOperator:
                    data.Operator);


        var sut =
            CreateViewModelForEditor(
                data,
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]));


        await sut.LoadMonthAsync();


        var item =
            Assert.Single(
                sut.SelectedDaySchedules);


        await sut.OpenEditEditorAsync(
            item);


        Assert.True(
            sut.IsEditorOpen);

        Assert.False(
            sut.IsCreateMode);

        Assert.Equal(
            schedule.Id,
            sut.EditingScheduleId);

        Assert.Equal(
            Today,
            DateOnly.FromDateTime(
                sut.EditorScheduledDate!
                    .Value
                    .Date));

        Assert.Equal(
            "既存備考",
            sut.EditorNotes);

        Assert.Equal(
            data.FactorySite.Id,
            sut.SelectedFactorySite!.Id);

        Assert.Equal(
            data.Location.Id,
            sut.SelectedLocation!.Id);

        Assert.Equal(
            data.Equipment.Id,
            sut.SelectedEquipment!.Id);

        Assert.Equal(
            data.Template.Id,
            sut.SelectedTemplate!.Id);

        Assert.Equal(
            data.Operator.Id,
            sut.SelectedOperator!.Id);
    }


    [Fact]
    public async Task OpenEditEditorAsync_WhenItemCannotEdit_DoesNothing()
    {
        var schedule =
            CreateSchedule(
                Today,
                status:
                    InspectionStatus.Completed);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]));


        await sut.LoadMonthAsync();


        var item =
            Assert.Single(
                sut.SelectedDaySchedules);


        Assert.False(
            item.CanEdit);


        await sut.OpenEditEditorAsync(
            item);


        Assert.False(
            sut.IsEditorOpen);
    }


    [Fact]
    public async Task SaveEditorCommand_Update_PassesValuesAndReloads()
    {
        var data =
            CreateEditorData();


        var schedule =
            CreateSchedule(
                Today,
                factorySite:
                    data.FactorySite,
                location:
                    data.Location,
                equipment:
                    data.Equipment,
                template:
                    data.Template,
                assignedOperator:
                    data.Operator);


        Guid capturedScheduleId =
            Guid.Empty;

        DateOnly?
            capturedDate =
                null;

        var updateCallCount =
            0;

        var loadCallCount =
            0;


        var sut =
            CreateViewModelForEditor(
                data,
                getMonthAsync:
                    _ =>
                    {
                        loadCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]);
                    },
                updateAsync:
                    (
                        scheduleId,
                        date,
                        _,
                        _,
                        _,
                        _) =>
                    {
                        updateCallCount++;

                        capturedScheduleId =
                            scheduleId;

                        capturedDate =
                            date;


                        return Task.CompletedTask;
                    });


        await sut.LoadMonthAsync();


        await sut.OpenEditEditorAsync(
            sut.SelectedDaySchedules[0]);


        var newDate =
            new DateOnly(
                2026,
                8,
                25);


        sut.EditorScheduledDate =
            new DateTimeOffset(
                newDate.ToDateTime(
                    TimeOnly.MinValue));


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.Equal(
            1,
            updateCallCount);

        Assert.Equal(
            schedule.Id,
            capturedScheduleId);

        Assert.Equal(
            newDate,
            capturedDate);

        Assert.Equal(
            "点検予定を更新しました。",
            sut.OperationMessage);

        Assert.False(
            sut.IsEditorOpen);

        // 初回Load + 更新後Load
        Assert.Equal(
            2,
            loadCallCount);
    }


    [Fact]
    public async Task SaveEditorCommand_EditWithoutEditingId_SetsEditorError()
    {
        var data =
            CreateEditorData();


        var sut =
            CreateViewModelForEditor(
                data);


        await sut.OpenCreateEditorCommand
            .ExecuteAsync(null);


        // Validationは通るが編集対象IDだけ存在しない状態。
        sut.IsCreateMode =
            false;

        sut.EditingScheduleId =
            null;


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "点検予定を更新できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "編集対象の点検予定が選択されていません。",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveEditorCommand_WhenUpdateFails_SetsEditorError()
    {
        var data =
            CreateEditorData();


        var schedule =
            CreateSchedule(
                Today,
                factorySite:
                    data.FactorySite,
                location:
                    data.Location,
                equipment:
                    data.Equipment,
                template:
                    data.Template,
                assignedOperator:
                    data.Operator);


        var sut =
            CreateViewModelForEditor(
                data,
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]),
                updateAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "更新エラー")));


        await sut.LoadMonthAsync();


        await sut.OpenEditEditorAsync(
            sut.SelectedDaySchedules[0]);


        await sut.SaveEditorCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasEditorError);

        Assert.Contains(
            "点検予定を更新できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "更新エラー",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Cancel Schedule
    // ============================================

    [Fact]
    public async Task RequestCancelSchedule_OpensDialogForCancelableSchedule()
    {
        var schedule =
            CreateSchedule(
                Today);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]));


        await sut.LoadMonthAsync();


        var item =
            Assert.Single(
                sut.SelectedDaySchedules);


        Assert.True(
            item.CanCancel);


        sut.RequestCancelSchedule(
            item);


        Assert.True(
            sut.IsCancelDialogOpen);

        Assert.Same(
            item,
            sut.PendingCancelSchedule);
    }


    [Fact]
    public async Task RequestCancelSchedule_WhenItemCannotCancel_DoesNothing()
    {
        var schedule =
            CreateSchedule(
                Today,
                status:
                    InspectionStatus.Completed);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]));


        await sut.LoadMonthAsync();


        var item =
            Assert.Single(
                sut.SelectedDaySchedules);


        Assert.False(
            item.CanCancel);


        sut.RequestCancelSchedule(
            item);


        Assert.False(
            sut.IsCancelDialogOpen);

        Assert.Null(
            sut.PendingCancelSchedule);
    }


    [Fact]
    public async Task CloseCancelDialogCommand_ClosesDialog()
    {
        var schedule =
            CreateSchedule(
                Today);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]));


        await sut.LoadMonthAsync();


        sut.RequestCancelSchedule(
            sut.SelectedDaySchedules[0]);


        sut.CloseCancelDialogCommand
            .Execute(null);


        Assert.False(
            sut.IsCancelDialogOpen);

        Assert.Null(
            sut.PendingCancelSchedule);
    }


    [Fact]
    public async Task ConfirmCancelScheduleCommand_WithNoPendingSchedule_DoesNothing()
    {
        var cancelCallCount =
            0;


        var sut =
            CreateViewModel(
                cancelAsync:
                    _ =>
                    {
                        cancelCallCount++;

                        return Task.CompletedTask;
                    });


        await sut.ConfirmCancelScheduleCommand
            .ExecuteAsync(null);


        Assert.Equal(
            0,
            cancelCallCount);
    }


    [Fact]
    public async Task ConfirmCancelScheduleCommand_CancelsClosesDialogAndReloads()
    {
        var schedule =
            CreateSchedule(
                Today);


        Guid capturedId =
            Guid.Empty;

        var cancelCallCount =
            0;

        var loadCallCount =
            0;


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                    {
                        loadCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]);
                    },
                cancelAsync:
                    id =>
                    {
                        cancelCallCount++;

                        capturedId =
                            id;

                        return Task.CompletedTask;
                    });


        await sut.LoadMonthAsync();


        sut.RequestCancelSchedule(
            sut.SelectedDaySchedules[0]);


        await sut.ConfirmCancelScheduleCommand
            .ExecuteAsync(null);


        Assert.Equal(
            1,
            cancelCallCount);

        Assert.Equal(
            schedule.Id,
            capturedId);

        Assert.False(
            sut.IsCancelDialogOpen);

        Assert.Null(
            sut.PendingCancelSchedule);

        Assert.Equal(
            "点検予定を取り消しました。",
            sut.OperationMessage);

        Assert.False(
            sut.IsSaving);

        Assert.Equal(
            2,
            loadCallCount);
    }


    [Fact]
    public async Task ConfirmCancelScheduleCommand_WhenCancelFails_SetsErrorAndKeepsDialog()
    {
        var schedule =
            CreateSchedule(
                Today);


        var sut =
            CreateViewModel(
                getMonthAsync:
                    _ =>
                        Task.FromResult<
                            IReadOnlyList<InspectionSchedule>>(
                        [
                            schedule
                        ]),
                cancelAsync:
                    _ =>
                        Task.FromException(
                            new InvalidOperationException(
                                "取消エラー")));


        await sut.LoadMonthAsync();


        var item =
            sut.SelectedDaySchedules[0];


        sut.RequestCancelSchedule(
            item);


        await sut.ConfirmCancelScheduleCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.HasError);

        Assert.Contains(
            "点検予定を取り消せませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "取消エラー",
            sut.ErrorMessage);

        Assert.True(
            sut.IsCancelDialogOpen);

        Assert.Same(
            item,
            sut.PendingCancelSchedule);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // Helpers
    // ============================================

    private static ScheduleCalendarViewModel
        CreateViewModel(
            Func<
                DateOnly,
                Task<IReadOnlyList<InspectionSchedule>>>?
                getMonthAsync =
                    null,
            Func<
                Task<IReadOnlyList<FactorySite>>>?
                getFactorySitesAsync =
                    null,
            Func<
                Guid,
                Task<IReadOnlyList<Location>>>?
                getLocationsAsync =
                    null,
            Func<
                Guid,
                Task<IReadOnlyList<Equipment>>>?
                getEquipmentsAsync =
                    null,
            Func<
                EquipmentType,
                Task<IReadOnlyList<InspectionTemplate>>>?
                getTemplatesAsync =
                    null,
            Func<
                Task<IReadOnlyList<Operator>>>?
                getInspectorsAsync =
                    null,
            Func<
                DateOnly,
                Guid,
                Guid,
                Guid,
                string?,
                Task>?
                createAsync =
                    null,
            Func<
                Guid,
                DateOnly,
                Guid,
                Guid,
                Guid,
                string?,
                Task>?
                updateAsync =
                    null,
            Func<
                Guid,
                Task>?
                cancelAsync =
                    null,
            Func<DateOnly>?
                todayProvider =
                    null)
    {
        getMonthAsync ??=
            _ =>
                EmptySchedules();

        getFactorySitesAsync ??=
            EmptyFactorySites;

        getLocationsAsync ??=
            _ =>
                EmptyLocations();

        getEquipmentsAsync ??=
            _ =>
                EmptyEquipments();

        getTemplatesAsync ??=
            _ =>
                EmptyTemplates();

        getInspectorsAsync ??=
            EmptyOperators;

        createAsync ??=
            (
                _,
                _,
                _,
                _,
                _) =>
                Task.CompletedTask;

        updateAsync ??=
            (
                _,
                _,
                _,
                _,
                _,
                _) =>
                Task.CompletedTask;

        cancelAsync ??=
            _ =>
                Task.CompletedTask;

        todayProvider ??=
            () =>
                Today;


        return new ScheduleCalendarViewModel(
            getMonthAsync,
            getFactorySitesAsync,
            getLocationsAsync,
            getEquipmentsAsync,
            getTemplatesAsync,
            getInspectorsAsync,
            createAsync,
            updateAsync,
            cancelAsync,
            todayProvider);
    }


    private static ScheduleCalendarViewModel
        CreateViewModelForEditor(
            EditorTestData data,
            Func<
                DateOnly,
                Task<IReadOnlyList<InspectionSchedule>>>?
                getMonthAsync =
                    null,
            Func<
                DateOnly,
                Guid,
                Guid,
                Guid,
                string?,
                Task>?
                createAsync =
                    null,
            Func<
                Guid,
                DateOnly,
                Guid,
                Guid,
                Guid,
                string?,
                Task>?
                updateAsync =
                    null)
    {
        return CreateViewModel(
            getMonthAsync:
                getMonthAsync,

            getFactorySitesAsync:
                () =>
                    Task.FromResult<
                        IReadOnlyList<FactorySite>>(
                    [
                        data.FactorySite
                    ]),

            getLocationsAsync:
                factorySiteId =>
                    Task.FromResult<
                        IReadOnlyList<Location>>(
                    [
                        data.Location
                    ]),

            getEquipmentsAsync:
                locationId =>
                    Task.FromResult<
                        IReadOnlyList<Equipment>>(
                    [
                        data.Equipment
                    ]),

            getTemplatesAsync:
                equipmentType =>
                    Task.FromResult<
                        IReadOnlyList<InspectionTemplate>>(
                    [
                        data.Template
                    ]),

            getInspectorsAsync:
                () =>
                    Task.FromResult<
                        IReadOnlyList<Operator>>(
                    [
                        data.Operator
                    ]),

            createAsync:
                createAsync,

            updateAsync:
                updateAsync);
    }


    private static EditorTestData
        CreateEditorData()
    {
        var factorySite =
            new FactorySite(
                "SITE-001",
                "第1工場");


        var location =
            CreateUninitialized<
                Location>();


        SetProperty(
            location,
            "Id",
            Guid.NewGuid());

        SetProperty(
            location,
            "FactorySiteId",
            factorySite.Id);

        SetProperty(
            location,
            "FactorySite",
            factorySite);

        SetProperty(
            location,
            "Code",
            "COMPRESSOR-ROOM");

        SetProperty(
            location,
            "Name",
            "コンプレッサー室");

        SetProperty(
            location,
            "Floor",
            "1F");

        SetProperty(
            location,
            "IsActive",
            true);


        var equipment =
            CreateUninitialized<
                Equipment>();


        SetProperty(
            equipment,
            "Id",
            Guid.NewGuid());

        SetProperty(
            equipment,
            "LocationId",
            location.Id);

        SetProperty(
            equipment,
            "Location",
            location);

        SetProperty(
            equipment,
            "EquipmentCode",
            "COMP-001");

        SetProperty(
            equipment,
            "Name",
            "コンプレッサー1号機");

        SetProperty(
            equipment,
            "EquipmentType",
            EquipmentType.AirCompressor);

        SetProperty(
            equipment,
            "Status",
            EquipmentStatus.InService);


        var template =
            new InspectionTemplate
            {
                Name =
                    "コンプレッサー日常点検",

                EquipmentType =
                    EquipmentType.AirCompressor,

                Version =
                    1,

                IsActive =
                    true
            };


        var operatorEntity =
            new Operator
            {
                LoginId =
                    "inspector01",

                NormalizedLoginId =
                    "INSPECTOR01",

                DisplayName =
                    "点検担当者A",

                PasswordHash =
                    "TEST_HASH",

                Role =
                    OperatorRole.Inspector,

                IsActive =
                    true
            };


        return new EditorTestData(
            factorySite,
            location,
            equipment,
            template,
            operatorEntity);
    }


    private static InspectionSchedule
        CreateSchedule(
            DateOnly scheduledDate,
            string equipmentCode =
                "EQ-001",
            InspectionStatus status =
                InspectionStatus.NotStarted,
            bool cancelled =
                false,
            string? notes =
                null,
            FactorySite? factorySite =
                null,
            Location? location =
                null,
            Equipment? equipment =
                null,
            InspectionTemplate? template =
                null,
            Operator? assignedOperator =
                null)
    {
        factorySite ??=
            new FactorySite(
                $"SITE-{Guid.NewGuid():N}"
                    [..12],
                "第1工場");


        if (location is null)
        {
            location =
                CreateUninitialized<
                    Location>();


            SetProperty(
                location,
                "Id",
                Guid.NewGuid());

            SetProperty(
                location,
                "FactorySiteId",
                factorySite.Id);

            SetProperty(
                location,
                "FactorySite",
                factorySite);

            SetProperty(
                location,
                "Code",
                "LOCATION");

            SetProperty(
                location,
                "Name",
                "設備エリア");

            SetProperty(
                location,
                "Floor",
                "1F");

            SetProperty(
                location,
                "IsActive",
                true);
        }


        if (equipment is null)
        {
            equipment =
                CreateUninitialized<
                    Equipment>();


            SetProperty(
                equipment,
                "Id",
                Guid.NewGuid());

            SetProperty(
                equipment,
                "LocationId",
                location.Id);

            SetProperty(
                equipment,
                "Location",
                location);

            SetProperty(
                equipment,
                "EquipmentCode",
                equipmentCode);

            SetProperty(
                equipment,
                "Name",
                $"設備 {equipmentCode}");

            SetProperty(
                equipment,
                "EquipmentType",
                EquipmentType.AirCompressor);

            SetProperty(
                equipment,
                "Status",
                EquipmentStatus.InService);
        }


        template ??=
            new InspectionTemplate
            {
                Name =
                    "日常点検",

                EquipmentType =
                    equipment.EquipmentType,

                Version =
                    1,

                IsActive =
                    true
            };


        assignedOperator ??=
            new Operator
            {
                LoginId =
                    $"inspector-{Guid.NewGuid():N}",

                NormalizedLoginId =
                    "INSPECTOR",

                DisplayName =
                    "点検担当者A",

                PasswordHash =
                    "TEST_HASH",

                Role =
                    OperatorRole.Inspector,

                IsActive =
                    true
            };


        var schedule =
            new InspectionSchedule(
                scheduledDate,
                equipment.Id,
                template.Id,
                assignedOperator.Id,
                notes);


        SetProperty(
            schedule,
            "Equipment",
            equipment);

        SetProperty(
            schedule,
            "InspectionTemplate",
            template);

        SetProperty(
            schedule,
            "AssignedOperator",
            assignedOperator);


        if (status !=
            InspectionStatus.NotStarted)
        {
            var inspection =
                new Inspection(
                    schedule.Id);


            SetProperty(
                inspection,
                "Status",
                status);


            schedule.AttachInspection(
                inspection);
        }


        if (cancelled)
        {
            schedule.Cancel();
        }


        return schedule;
    }


    private static T
        CreateUninitialized<T>()
        where T : class
    {
        return (T)
            RuntimeHelpers
                .GetUninitializedObject(
                    typeof(T));
    }


    private static void SetProperty(
        object target,
        string propertyName,
        object? value)
    {
        ArgumentNullException.ThrowIfNull(
            target);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            propertyName);


        var type =
            target.GetType();


        while (type is not null)
        {
            // ============================================
            // 1. Property の setter を探す
            // ============================================

            var property =
                type.GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);


            if (property is not null)
            {
                var setter =
                    property.GetSetMethod(
                        nonPublic: true);


                if (setter is not null)
                {
                    setter.Invoke(
                        target,
                        [value]);

                    return;
                }
            }


            // ============================================
            // 2. getter-only auto property の
            //    backing field を探す
            //
            //    例:
            //    public FactorySite FactorySite { get; }
            //
            //    ↓ コンパイラ内部
            //
            //    <FactorySite>k__BackingField
            // ============================================

            var backingField =
                type.GetField(
                    $"<{propertyName}>k__BackingField",
                    BindingFlags.Instance |
                    BindingFlags.NonPublic |
                    BindingFlags.DeclaredOnly);


            if (backingField is not null)
            {
                backingField.SetValue(
                    target,
                    value);

                return;
            }


            // ============================================
            // 3. 基底クラスも探す
            //
            // EntityBase.Id などへの対応
            // ============================================

            type =
                type.BaseType;
        }


        throw new InvalidOperationException(
            $"Property or backing field not found: " +
            $"{target.GetType().Name}.{propertyName}");
    }


    private static T GetProperty<T>(
        object target,
        string propertyName)
    {
        var property =
            target
                .GetType()
                .GetProperty(
                    propertyName,
                    BindingFlags.Instance |
                    BindingFlags.Public |
                    BindingFlags.NonPublic);


        if (property is null)
        {
            throw new InvalidOperationException(
                $"Property not found: " +
                $"{target.GetType().Name}.{propertyName}");
        }


        return (T)property
            .GetValue(
                target)!;
    }


    private static Task<
        IReadOnlyList<InspectionSchedule>>
        EmptySchedules()
    {
        return Task.FromResult<
            IReadOnlyList<InspectionSchedule>>(
            []);
    }


    private static Task<
        IReadOnlyList<FactorySite>>
        EmptyFactorySites()
    {
        return Task.FromResult<
            IReadOnlyList<FactorySite>>(
            []);
    }


    private static Task<
        IReadOnlyList<Location>>
        EmptyLocations()
    {
        return Task.FromResult<
            IReadOnlyList<Location>>(
            []);
    }


    private static Task<
        IReadOnlyList<Equipment>>
        EmptyEquipments()
    {
        return Task.FromResult<
            IReadOnlyList<Equipment>>(
            []);
    }


    private static Task<
        IReadOnlyList<InspectionTemplate>>
        EmptyTemplates()
    {
        return Task.FromResult<
            IReadOnlyList<InspectionTemplate>>(
            []);
    }


    private static Task<
        IReadOnlyList<Operator>>
        EmptyOperators()
    {
        return Task.FromResult<
            IReadOnlyList<Operator>>(
            []);
    }


    private sealed record EditorTestData(
        FactorySite FactorySite,
        Location Location,
        Equipment Equipment,
        InspectionTemplate Template,
        Operator Operator);
}