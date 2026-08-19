using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class MemberDashboardViewModel
    : ViewModelBase
{
    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<
        Guid,
        DateOnly,
        Task<IReadOnlyList<InspectionSchedule>>>
        _getDayForOperatorAsync;

    private readonly Func<
        Guid,
        DateOnly,
        Task<IReadOnlyList<InspectionSchedule>>>
        _getMonthForOperatorAsync;

    private readonly Func<DateOnly>
        _todayProvider;

    private readonly Guid
        _operatorId;

    private readonly Action<Guid>
        _openInspection;


    // ============================================
    // Loaded Data
    // ============================================

    private IReadOnlyList<InspectionSchedule>
        _monthSchedules = [];

    private IReadOnlyList<InspectionSchedule>
        _todaySchedules = [];


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public MemberDashboardViewModel(
        Guid operatorId,
        ScheduleRepository scheduleRepository,
        Action<Guid> openInspection)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            scheduleRepository);

        ArgumentNullException.ThrowIfNull(
            openInspection);


        _operatorId =
            operatorId;

        _openInspection =
            openInspection;


        /*
         * Repository側はoptional CancellationTokenを持つため、
         * method groupではなくlambdaでラップする。
         */
        _getDayForOperatorAsync =
            (id, date) =>
                scheduleRepository
                    .GetDayForOperatorAsync(
                        id,
                        date);

        _getMonthForOperatorAsync =
            (id, month) =>
                scheduleRepository
                    .GetMonthForOperatorAsync(
                        id,
                        month);


        _todayProvider =
            () =>
                DateOnly.FromDateTime(
                    DateTime.Today);


        InitializeCurrentDate();


        /*
         * 本番では従来どおり
         * 生成後に自動ロードする。
         */
        _ = InitializeAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal MemberDashboardViewModel(
        Guid operatorId,
        Func<
            Guid,
            DateOnly,
            Task<IReadOnlyList<InspectionSchedule>>>
            getDayForOperatorAsync,
        Func<
            Guid,
            DateOnly,
            Task<IReadOnlyList<InspectionSchedule>>>
            getMonthForOperatorAsync,
        Action<Guid> openInspection,
        Func<DateOnly> todayProvider)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            getDayForOperatorAsync);

        ArgumentNullException.ThrowIfNull(
            getMonthForOperatorAsync);

        ArgumentNullException.ThrowIfNull(
            openInspection);

        ArgumentNullException.ThrowIfNull(
            todayProvider);


        _operatorId =
            operatorId;

        _getDayForOperatorAsync =
            getDayForOperatorAsync;

        _getMonthForOperatorAsync =
            getMonthForOperatorAsync;

        _openInspection =
            openInspection;

        _todayProvider =
            todayProvider;


        InitializeCurrentDate();


        /*
         * テスト用では自動ロードしない。
         * InitializeAsync または各Commandを
         * テスト側から明示的に実行する。
         */
    }


    // ============================================
    // Basic Information
    // ============================================

    public string Title =>
        "点検担当者予定";


    public string Description =>
        "担当する点検予定をカレンダーから確認し、点検を開始します。";


    public string InformationMessage =>
        "カレンダーの日付を選択し、点検対象設備から点検を開始してください。";


    // ============================================
    // Collections
    // ============================================

    public ObservableCollection<
        CalendarDayViewModel>
        CalendarDays
    { get; } = [];


    public ObservableCollection<
        MemberScheduleItemViewModel>
        SelectedDaySchedules
    { get; } = [];


    // ============================================
    // Date
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(MonthTitle))]
    private DateOnly displayedMonth;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(SelectedDateTitle))]
    private DateOnly selectedDate;


    // ============================================
    // Loading
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsSelectedDayEmpty))]
    private bool isLoading;


    // ============================================
    // Error
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    // ============================================
    // Calculated Properties
    // ============================================

    public string MonthTitle =>
        $"{DisplayedMonth.Year}年" +
        $"{DisplayedMonth.Month}月";


    public string SelectedDateTitle =>
        $"{SelectedDate.Year}年" +
        $"{SelectedDate.Month}月" +
        $"{SelectedDate.Day}日の点検予定";


    public string SelectedDayScheduleCountText =>
        $"全 {SelectedDaySchedules.Count} 件";


    public int TodayScheduleCount =>
        _todaySchedules.Count;


    public int InProgressCount =>
        _todaySchedules.Count(
            schedule =>
                GetStatus(schedule) ==
                    InspectionStatus.InProgress);


    public int CompletedCount =>
        _todaySchedules.Count(
            schedule =>
                GetStatus(schedule) is
                    InspectionStatus.Completed or
                    InspectionStatus.Approved);


    public int AbnormalityCount =>
        _todaySchedules.Sum(
            schedule =>
                schedule.Inspection?
                    .Results
                    .Count(
                        result =>
                            result.IsAbnormal)
                ?? 0);


    public bool IsSelectedDayEmpty =>
        !IsLoading &&
        SelectedDaySchedules.Count == 0;


    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    // ============================================
    // Initial Date
    // ============================================

    private void InitializeCurrentDate()
    {
        var today =
            _todayProvider();


        DisplayedMonth =
            new DateOnly(
                today.Year,
                today.Month,
                1);

        SelectedDate =
            today;
    }


    // ============================================
    // Initialize
    // ============================================

    internal async Task InitializeAsync()
    {
        IsLoading =
            true;

        ErrorMessage =
            null;


        try
        {
            var today =
                _todayProvider();


            var todayTask =
                _getDayForOperatorAsync(
                    _operatorId,
                    today);


            var monthTask =
                _getMonthForOperatorAsync(
                    _operatorId,
                    DisplayedMonth);


            await Task.WhenAll(
                todayTask,
                monthTask);


            _todaySchedules =
                await todayTask;

            _monthSchedules =
                await monthTask;


            BuildCalendarDays();

            BuildSelectedDaySchedules();

            RefreshSummaryCards();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検予定を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;


            OnPropertyChanged(
                nameof(IsSelectedDayEmpty));
        }
    }


    // ============================================
    // Load Month
    // ============================================

    private async Task LoadMonthAsync()
    {
        IsLoading =
            true;

        ErrorMessage =
            null;


        try
        {
            _monthSchedules =
                await _getMonthForOperatorAsync(
                    _operatorId,
                    DisplayedMonth);


            BuildCalendarDays();

            BuildSelectedDaySchedules();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "月間の点検予定を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;


            OnPropertyChanged(
                nameof(IsSelectedDayEmpty));
        }
    }


    // ============================================
    // Previous Month
    // ============================================

    [RelayCommand]
    private Task PreviousMonthAsync()
    {
        return MoveMonthAsync(
            -1);
    }


    // ============================================
    // Next Month
    // ============================================

    [RelayCommand]
    private Task NextMonthAsync()
    {
        return MoveMonthAsync(
            1);
    }


    // ============================================
    // Go To Today
    // ============================================

    [RelayCommand]
    private async Task GoToTodayAsync()
    {
        var today =
            _todayProvider();


        DisplayedMonth =
            new DateOnly(
                today.Year,
                today.Month,
                1);

        SelectedDate =
            today;


        await InitializeAsync();
    }


    // ============================================
    // Refresh
    // ============================================

    [RelayCommand]
    private Task RefreshAsync()
    {
        return InitializeAsync();
    }


    // ============================================
    // Move Month
    // ============================================

    private async Task MoveMonthAsync(
        int months)
    {
        var targetMonth =
            DisplayedMonth.AddMonths(
                months);


        var selectedDay =
            Math.Min(
                SelectedDate.Day,
                DateTime.DaysInMonth(
                    targetMonth.Year,
                    targetMonth.Month));


        DisplayedMonth =
            new DateOnly(
                targetMonth.Year,
                targetMonth.Month,
                1);


        SelectedDate =
            new DateOnly(
                targetMonth.Year,
                targetMonth.Month,
                selectedDay);


        await LoadMonthAsync();
    }


    // ============================================
    // Build Calendar
    // ============================================

    private void BuildCalendarDays()
    {
        CalendarDays.Clear();


        var monthStart =
            new DateOnly(
                DisplayedMonth.Year,
                DisplayedMonth.Month,
                1);


        var startOffset =
            (int)monthStart.DayOfWeek;


        var gridStart =
            monthStart.AddDays(
                -startOffset);


        for (var index = 0;
             index < 42;
             index++)
        {
            var date =
                gridStart.AddDays(
                    index);


            var schedules =
                _monthSchedules
                    .Where(
                        schedule =>
                            schedule.ScheduledDate ==
                            date)
                    .ToList();


            var overdueCount =
                schedules.Count(
                    IsOverdue);


            var completedCount =
                schedules.Count(
                    schedule =>
                        GetStatus(schedule) is
                            InspectionStatus.Completed or
                            InspectionStatus.Approved);


            var day =
                new CalendarDayViewModel(
                    date,
                    date.Month ==
                        DisplayedMonth.Month &&
                    date.Year ==
                        DisplayedMonth.Year,
                    schedules.Count,
                    overdueCount,
                    completedCount,
                    SelectCalendarDay);


            day.IsSelected =
                date ==
                SelectedDate;


            CalendarDays.Add(
                day);
        }
    }


    // ============================================
    // Select Calendar Day
    // ============================================

    private void SelectCalendarDay(
        CalendarDayViewModel day)
    {
        _ =
            SelectCalendarDayAsync(
                day);
    }


    /*
     * テストから非同期処理を確実にawaitするためinternal。
     */
    internal async Task SelectCalendarDayAsync(
        CalendarDayViewModel day)
    {
        ArgumentNullException.ThrowIfNull(
            day);


        /*
         * 前月・翌月の日付セルを選択した場合は
         * 表示月そのものを移動する。
         */
        if (day.Date.Month !=
                DisplayedMonth.Month ||
            day.Date.Year !=
                DisplayedMonth.Year)
        {
            DisplayedMonth =
                new DateOnly(
                    day.Date.Year,
                    day.Date.Month,
                    1);


            SelectedDate =
                day.Date;


            await LoadMonthAsync();


            return;
        }


        SelectedDate =
            day.Date;


        foreach (var calendarDay
                 in CalendarDays)
        {
            calendarDay.IsSelected =
                calendarDay.Date ==
                SelectedDate;
        }


        BuildSelectedDaySchedules();
    }


    // ============================================
    // Build Selected Day Schedules
    // ============================================

    private void BuildSelectedDaySchedules()
    {
        SelectedDaySchedules.Clear();


        foreach (var schedule in
                 _monthSchedules
                     .Where(
                         schedule =>
                             schedule.ScheduledDate ==
                             SelectedDate)
                     .OrderBy(
                         schedule =>
                             schedule.Equipment
                                 .Location
                                 .FactorySite
                                 .Name)
                     .ThenBy(
                         schedule =>
                             schedule.Equipment
                                 .Location
                                 .Name)
                     .ThenBy(
                         schedule =>
                             schedule.Equipment
                                 .EquipmentCode))
        {
            SelectedDaySchedules.Add(
                new MemberScheduleItemViewModel(
                    schedule.Id,
                    schedule.ScheduledDate,
                    schedule.Equipment
                        .Location
                        .FactorySite
                        .Name,
                    schedule.Equipment
                        .Location
                        .Name,
                    schedule.Equipment
                        .EquipmentCode,
                    schedule.Equipment
                        .Name,
                    schedule.InspectionTemplate
                        .Name,
                    schedule.Notes,
                    GetStatus(
                        schedule),
                    _openInspection));
        }


        OnPropertyChanged(
            nameof(IsSelectedDayEmpty));


        OnPropertyChanged(
            nameof(
                SelectedDayScheduleCountText));
    }


    // ============================================
    // Summary
    // ============================================

    private void RefreshSummaryCards()
    {
        OnPropertyChanged(
            nameof(TodayScheduleCount));

        OnPropertyChanged(
            nameof(InProgressCount));

        OnPropertyChanged(
            nameof(CompletedCount));

        OnPropertyChanged(
            nameof(AbnormalityCount));
    }


    // ============================================
    // Status
    // ============================================

    private static InspectionStatus GetStatus(
        InspectionSchedule schedule)
    {
        return schedule.Inspection?.Status ??
            InspectionStatus.NotStarted;
    }


    // ============================================
    // Overdue
    // ============================================

    private bool IsOverdue(
        InspectionSchedule schedule)
    {
        return
            !schedule.IsCancelled &&
            GetStatus(schedule) ==
                InspectionStatus.NotStarted &&
            schedule.ScheduledDate <
                _todayProvider();
    }
}