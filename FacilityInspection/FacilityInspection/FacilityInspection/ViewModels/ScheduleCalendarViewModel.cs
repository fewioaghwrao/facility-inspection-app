using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class ScheduleCalendarViewModel
    : ViewModelBase
{
    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<
        DateOnly,
        Task<IReadOnlyList<InspectionSchedule>>>
        _getMonthAsync;

    private readonly Func<
        Task<IReadOnlyList<FactorySite>>>
        _getFactorySitesAsync;

    private readonly Func<
        Guid,
        Task<IReadOnlyList<Location>>>
        _getLocationsAsync;

    private readonly Func<
        Guid,
        Task<IReadOnlyList<Equipment>>>
        _getEquipmentsAsync;

    private readonly Func<
        EquipmentType,
        Task<IReadOnlyList<InspectionTemplate>>>
        _getTemplatesAsync;

    private readonly Func<
        Task<IReadOnlyList<Operator>>>
        _getInspectorsAsync;

    private readonly Func<
        DateOnly,
        Guid,
        Guid,
        Guid,
        string?,
        Task>
        _createAsync;

    private readonly Func<
        Guid,
        DateOnly,
        Guid,
        Guid,
        Guid,
        string?,
        Task>
        _updateAsync;

    private readonly Func<
        Guid,
        Task>
        _cancelAsync;

    private readonly Func<DateOnly>
        _todayProvider;


    // ============================================
    // Data
    // ============================================

    private IReadOnlyList<InspectionSchedule>
        _monthSchedules = [];

    private bool
        _isPopulatingEditor;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public ScheduleCalendarViewModel(
        ScheduleRepository scheduleRepository)
    {
        ArgumentNullException.ThrowIfNull(
            scheduleRepository);


        _getMonthAsync =
            displayedMonth =>
                scheduleRepository
                    .GetMonthAsync(
                        displayedMonth);


        _getFactorySitesAsync =
            () =>
                scheduleRepository
                    .GetFactorySitesAsync();


        _getLocationsAsync =
            factorySiteId =>
                scheduleRepository
                    .GetLocationsAsync(
                        factorySiteId);


        _getEquipmentsAsync =
            locationId =>
                scheduleRepository
                    .GetEquipmentsAsync(
                        locationId);


        _getTemplatesAsync =
            equipmentType =>
                scheduleRepository
                    .GetTemplatesAsync(
                        equipmentType);


        _getInspectorsAsync =
            () =>
                scheduleRepository
                    .GetInspectorsAsync();


        _createAsync =
            async (
                scheduledDate,
                equipmentId,
                inspectionTemplateId,
                assignedOperatorId,
                notes) =>
            {
                await scheduleRepository
                    .CreateAsync(
                        scheduledDate,
                        equipmentId,
                        inspectionTemplateId,
                        assignedOperatorId,
                        notes);
            };


        _updateAsync =
            (
                scheduleId,
                scheduledDate,
                equipmentId,
                inspectionTemplateId,
                assignedOperatorId,
                notes) =>
                scheduleRepository
                    .UpdateAsync(
                        scheduleId,
                        scheduledDate,
                        equipmentId,
                        inspectionTemplateId,
                        assignedOperatorId,
                        notes);


        _cancelAsync =
            scheduleId =>
                scheduleRepository
                    .CancelAsync(
                        scheduleId);


        _todayProvider =
            () =>
                DateOnly.FromDateTime(
                    DateTime.Today);


        InitializeDate();


        // 本番では従来どおり自動ロード。
        _ = LoadMonthAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal ScheduleCalendarViewModel(
        Func<
            DateOnly,
            Task<IReadOnlyList<InspectionSchedule>>>
            getMonthAsync,
        Func<
            Task<IReadOnlyList<FactorySite>>>
            getFactorySitesAsync,
        Func<
            Guid,
            Task<IReadOnlyList<Location>>>
            getLocationsAsync,
        Func<
            Guid,
            Task<IReadOnlyList<Equipment>>>
            getEquipmentsAsync,
        Func<
            EquipmentType,
            Task<IReadOnlyList<InspectionTemplate>>>
            getTemplatesAsync,
        Func<
            Task<IReadOnlyList<Operator>>>
            getInspectorsAsync,
        Func<
            DateOnly,
            Guid,
            Guid,
            Guid,
            string?,
            Task>
            createAsync,
        Func<
            Guid,
            DateOnly,
            Guid,
            Guid,
            Guid,
            string?,
            Task>
            updateAsync,
        Func<
            Guid,
            Task>
            cancelAsync,
        Func<DateOnly>
            todayProvider)
    {
        ArgumentNullException.ThrowIfNull(
            getMonthAsync);

        ArgumentNullException.ThrowIfNull(
            getFactorySitesAsync);

        ArgumentNullException.ThrowIfNull(
            getLocationsAsync);

        ArgumentNullException.ThrowIfNull(
            getEquipmentsAsync);

        ArgumentNullException.ThrowIfNull(
            getTemplatesAsync);

        ArgumentNullException.ThrowIfNull(
            getInspectorsAsync);

        ArgumentNullException.ThrowIfNull(
            createAsync);

        ArgumentNullException.ThrowIfNull(
            updateAsync);

        ArgumentNullException.ThrowIfNull(
            cancelAsync);

        ArgumentNullException.ThrowIfNull(
            todayProvider);


        _getMonthAsync =
            getMonthAsync;

        _getFactorySitesAsync =
            getFactorySitesAsync;

        _getLocationsAsync =
            getLocationsAsync;

        _getEquipmentsAsync =
            getEquipmentsAsync;

        _getTemplatesAsync =
            getTemplatesAsync;

        _getInspectorsAsync =
            getInspectorsAsync;

        _createAsync =
            createAsync;

        _updateAsync =
            updateAsync;

        _cancelAsync =
            cancelAsync;

        _todayProvider =
            todayProvider;


        InitializeDate();


        // internal版では自動ロードしない。
    }


    // ============================================
    // Initial Date
    // ============================================

    private void InitializeDate()
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
    // Header
    // ============================================

    public string Title =>
        "点検予定管理";


    public string Description =>
        "設備の点検予定、点検担当者、実施状況をカレンダーで管理します。";


    // ============================================
    // Collections
    // ============================================

    public ObservableCollection<
        CalendarDayViewModel>
        CalendarDays
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleListItemViewModel>
        SelectedDaySchedules
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleSelectionOptionViewModel>
        FactorySiteOptions
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleSelectionOptionViewModel>
        LocationOptions
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleSelectionOptionViewModel>
        EquipmentOptions
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleSelectionOptionViewModel>
        TemplateOptions
    {
        get;
    } = [];


    public ObservableCollection<
        ScheduleSelectionOptionViewModel>
        OperatorOptions
    {
        get;
    } = [];


    // ============================================
    // Calendar
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
    // Messages
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasOperationMessage))]
    private string? operationMessage;


    // ============================================
    // Editor
    // ============================================

    [ObservableProperty]
    private bool isEditorOpen;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(EditorTitle))]
    [NotifyPropertyChangedFor(
        nameof(EditorDescription))]
    [NotifyPropertyChangedFor(
        nameof(SaveButtonText))]
    private bool isCreateMode;


    [ObservableProperty]
    private Guid? editingScheduleId;


    [ObservableProperty]
    private DateTimeOffset?
        editorScheduledDate;


    [ObservableProperty]
    private ScheduleSelectionOptionViewModel?
        selectedFactorySite;


    [ObservableProperty]
    private ScheduleSelectionOptionViewModel?
        selectedLocation;


    [ObservableProperty]
    private ScheduleSelectionOptionViewModel?
        selectedEquipment;


    [ObservableProperty]
    private ScheduleSelectionOptionViewModel?
        selectedTemplate;


    [ObservableProperty]
    private ScheduleSelectionOptionViewModel?
        selectedOperator;


    [ObservableProperty]
    private string editorNotes =
        string.Empty;


    [ObservableProperty]
    private bool isSaving;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasEditorError))]
    private string? editorErrorMessage;


    // ============================================
    // Cancel Dialog
    // ============================================

    [ObservableProperty]
    private bool isCancelDialogOpen;


    [ObservableProperty]
    private ScheduleListItemViewModel?
        pendingCancelSchedule;


    // ============================================
    // Calculated Properties
    // ============================================

    public string MonthTitle =>
        $"{DisplayedMonth.Year}年{DisplayedMonth.Month}月";


    public string SelectedDateTitle =>
        $"{SelectedDate.Month}月{SelectedDate.Day}日の点検予定";


    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    public bool HasOperationMessage =>
        !string.IsNullOrWhiteSpace(
            OperationMessage);


    public bool HasEditorError =>
        !string.IsNullOrWhiteSpace(
            EditorErrorMessage);


    public bool IsSelectedDayEmpty =>
        !IsLoading &&
        SelectedDaySchedules.Count == 0;


    public string EditorTitle =>
        IsCreateMode
            ? "点検予定の新規登録"
            : "点検予定の編集";


    public string EditorDescription =>
        IsCreateMode
            ? "予定日、設備、点検票、担当者を選択します。"
            : "未実施の点検予定を変更します。";


    public string SaveButtonText =>
        IsCreateMode
            ? "登録"
            : "保存";


    // ============================================
    // Selection Changed
    // ============================================

    partial void OnSelectedFactorySiteChanged(
        ScheduleSelectionOptionViewModel? value)
    {
        if (_isPopulatingEditor)
        {
            return;
        }


        _ =
            HandleFactorySiteChangedAsync(
                value);
    }


    partial void OnSelectedLocationChanged(
        ScheduleSelectionOptionViewModel? value)
    {
        if (_isPopulatingEditor)
        {
            return;
        }


        _ =
            HandleLocationChangedAsync(
                value);
    }


    partial void OnSelectedEquipmentChanged(
        ScheduleSelectionOptionViewModel? value)
    {
        if (_isPopulatingEditor)
        {
            return;
        }


        _ =
            HandleEquipmentChangedAsync(
                value);
    }


    // ============================================
    // Load Month
    // ============================================

    [RelayCommand]
    internal async Task LoadMonthAsync()
    {
        if (IsLoading)
        {
            return;
        }


        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;


            _monthSchedules =
                await _getMonthAsync(
                    DisplayedMonth);


            BuildCalendarDays();

            BuildSelectedDaySchedules();
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
    // Previous Month
    // ============================================

    [RelayCommand]
    private async Task PreviousMonthAsync()
    {
        await MoveMonthAsync(
            -1);
    }


    // ============================================
    // Next Month
    // ============================================

    [RelayCommand]
    private async Task NextMonthAsync()
    {
        await MoveMonthAsync(
            1);
    }


    // ============================================
    // Today
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


        await LoadMonthAsync();
    }


    // ============================================
    // Open Create Editor
    // ============================================

    [RelayCommand]
    private async Task OpenCreateEditorAsync()
    {
        if (IsSaving)
        {
            return;
        }


        IsCreateMode =
            true;

        EditingScheduleId =
            null;


        EditorScheduledDate =
            ToDateTimeOffset(
                SelectedDate);


        EditorNotes =
            string.Empty;

        EditorErrorMessage =
            null;

        OperationMessage =
            null;


        try
        {
            await PopulateEditorOptionsAsync(
                null);


            IsEditorOpen =
                true;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "予定登録画面を準備できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
    }


    // ============================================
    // Open Edit Editor
    // ============================================

    internal async Task OpenEditEditorAsync(
        ScheduleListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(
            item);


        if (IsSaving ||
            !item.CanEdit)
        {
            return;
        }


        IsCreateMode =
            false;

        EditingScheduleId =
            item.Id;


        EditorScheduledDate =
            ToDateTimeOffset(
                item.ScheduledDate);


        EditorNotes =
            item.Notes ??
            string.Empty;


        EditorErrorMessage =
            null;

        OperationMessage =
            null;


        try
        {
            await PopulateEditorOptionsAsync(
                item);


            IsEditorOpen =
                true;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "予定編集画面を準備できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
    }


    // ============================================
    // Cancel Editor
    // ============================================

    [RelayCommand]
    private void CancelEditor()
    {
        if (IsSaving)
        {
            return;
        }


        IsEditorOpen =
            false;

        EditorErrorMessage =
            null;


        ClearEditor();
    }


    // ============================================
    // Save Editor
    // ============================================

    [RelayCommand]
    private async Task SaveEditorAsync()
    {
        if (IsSaving)
        {
            return;
        }


        if (!TryGetEditorInput(
                out var scheduledDate,
                out var equipmentId,
                out var templateId,
                out var operatorId))
        {
            return;
        }


        try
        {
            IsSaving =
                true;

            EditorErrorMessage =
                null;

            OperationMessage =
                null;


            if (IsCreateMode)
            {
                await _createAsync(
                    scheduledDate,
                    equipmentId,
                    templateId,
                    operatorId,
                    EditorNotes);


                OperationMessage =
                    "点検予定を登録しました。";
            }
            else
            {
                if (!EditingScheduleId.HasValue)
                {
                    throw new InvalidOperationException(
                        "編集対象の点検予定が選択されていません。");
                }


                await _updateAsync(
                    EditingScheduleId.Value,
                    scheduledDate,
                    equipmentId,
                    templateId,
                    operatorId,
                    EditorNotes);


                OperationMessage =
                    "点検予定を更新しました。";
            }


            IsEditorOpen =
                false;


            ClearEditor();


            DisplayedMonth =
                new DateOnly(
                    scheduledDate.Year,
                    scheduledDate.Month,
                    1);


            SelectedDate =
                scheduledDate;


            await LoadMonthAsync();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                IsCreateMode
                    ? "点検予定を登録できませんでした。" +
                      Environment.NewLine +
                      exception.Message

                    : "点検予定を更新できませんでした。" +
                      Environment.NewLine +
                      exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Request Cancel
    // ============================================

    internal void RequestCancelSchedule(
        ScheduleListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(
            item);


        if (!item.CanCancel)
        {
            return;
        }


        PendingCancelSchedule =
            item;

        IsCancelDialogOpen =
            true;
    }


    // ============================================
    // Close Cancel Dialog
    // ============================================

    [RelayCommand]
    private void CloseCancelDialog()
    {
        if (IsSaving)
        {
            return;
        }


        IsCancelDialogOpen =
            false;

        PendingCancelSchedule =
            null;
    }


    // ============================================
    // Confirm Cancel
    // ============================================

    [RelayCommand]
    private async Task ConfirmCancelScheduleAsync()
    {
        if (IsSaving ||
            PendingCancelSchedule is null)
        {
            return;
        }


        try
        {
            IsSaving =
                true;

            ErrorMessage =
                null;

            OperationMessage =
                null;


            await _cancelAsync(
                PendingCancelSchedule.Id);


            IsCancelDialogOpen =
                false;

            PendingCancelSchedule =
                null;


            OperationMessage =
                "点検予定を取り消しました。";


            await LoadMonthAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検予定を取り消せませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Move Month
    // ============================================

    private async Task MoveMonthAsync(
        int months)
    {
        var targetMonth =
            DisplayedMonth
                .AddMonths(
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
                        x =>
                            x.ScheduledDate ==
                            date)
                    .ToList();


            var overdueCount =
                schedules.Count(
                    IsOverdue);


            var completedCount =
                schedules.Count(
                    x =>
                        !x.IsCancelled &&
                        GetStatus(x) is
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
    // Calendar Day Callback
    // ============================================

    private void SelectCalendarDay(
        CalendarDayViewModel day)
    {
        _ =
            SelectCalendarDayAsync(
                day);
    }


    internal async Task SelectCalendarDayAsync(
        CalendarDayViewModel day)
    {
        ArgumentNullException.ThrowIfNull(
            day);


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
    // Selected Day
    // ============================================

    private void BuildSelectedDaySchedules()
    {
        SelectedDaySchedules.Clear();


        foreach (var schedule in
                 _monthSchedules
                     .Where(
                         x =>
                             x.ScheduledDate ==
                             SelectedDate)
                     .OrderBy(
                         x =>
                             x.Equipment
                                 .EquipmentCode))
        {
            SelectedDaySchedules.Add(
                CreateListItemViewModel(
                    schedule));
        }


        OnPropertyChanged(
            nameof(IsSelectedDayEmpty));
    }


    // ============================================
    // Create List Item
    // ============================================

    private ScheduleListItemViewModel
        CreateListItemViewModel(
            InspectionSchedule schedule)
    {
        return new ScheduleListItemViewModel(
            schedule.Id,
            schedule.ScheduledDate,
            schedule.Equipment
                .Location
                .FactorySiteId,
            schedule.Equipment
                .LocationId,
            schedule.EquipmentId,
            schedule.InspectionTemplateId,
            schedule.AssignedOperatorId,
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
            schedule.AssignedOperator
                .DisplayName,
            schedule.Notes,
            GetStatus(
                schedule),
            schedule.IsCancelled,
            OpenEditEditorAsync,
            RequestCancelSchedule);
    }


    // ============================================
    // Populate Editor
    // ============================================

    internal async Task PopulateEditorOptionsAsync(
        ScheduleListItemViewModel? editingItem)
    {
        _isPopulatingEditor =
            true;


        try
        {
            FactorySiteOptions.Clear();

            LocationOptions.Clear();

            EquipmentOptions.Clear();

            TemplateOptions.Clear();

            OperatorOptions.Clear();


            var factorySites =
                await _getFactorySitesAsync();


            foreach (var factorySite
                     in factorySites)
            {
                FactorySiteOptions.Add(
                    new ScheduleSelectionOptionViewModel(
                        factorySite.Id,
                        factorySite.Name));
            }


            var operators =
                await _getInspectorsAsync();


            foreach (var operatorEntity
                     in operators)
            {
                OperatorOptions.Add(
                    new ScheduleSelectionOptionViewModel(
                        operatorEntity.Id,
                        operatorEntity.DisplayName));
            }


            SelectedFactorySite =
                editingItem is null
                    ? FactorySiteOptions
                        .FirstOrDefault()

                    : FactorySiteOptions
                        .FirstOrDefault(
                            x =>
                                x.Id ==
                                editingItem.FactorySiteId);


            SelectedOperator =
                editingItem is null
                    ? OperatorOptions
                        .FirstOrDefault()

                    : OperatorOptions
                        .FirstOrDefault(
                            x =>
                                x.Id ==
                                editingItem
                                    .AssignedOperatorId);


            if (SelectedFactorySite
                is not null)
            {
                await LoadLocationsAsync(
                    SelectedFactorySite.Id);


                SelectedLocation =
                    editingItem is null
                        ? LocationOptions
                            .FirstOrDefault()

                        : LocationOptions
                            .FirstOrDefault(
                                x =>
                                    x.Id ==
                                    editingItem
                                        .LocationId);
            }


            if (SelectedLocation
                is not null)
            {
                await LoadEquipmentsAsync(
                    SelectedLocation.Id);


                SelectedEquipment =
                    editingItem is null
                        ? EquipmentOptions
                            .FirstOrDefault()

                        : EquipmentOptions
                            .FirstOrDefault(
                                x =>
                                    x.Id ==
                                    editingItem
                                        .EquipmentId);
            }


            if (SelectedEquipment?
                    .EquipmentType
                is EquipmentType
                    equipmentType)
            {
                await LoadTemplatesAsync(
                    equipmentType);


                SelectedTemplate =
                    editingItem is null
                        ? TemplateOptions
                            .FirstOrDefault()

                        : TemplateOptions
                            .FirstOrDefault(
                                x =>
                                    x.Id ==
                                    editingItem
                                        .InspectionTemplateId);
            }
        }
        finally
        {
            _isPopulatingEditor =
                false;
        }
    }


    // ============================================
    // Factory Site Changed
    // ============================================

    internal async Task HandleFactorySiteChangedAsync(
        ScheduleSelectionOptionViewModel? option)
    {
        try
        {
            _isPopulatingEditor =
                true;

            EditorErrorMessage =
                null;


            LocationOptions.Clear();

            EquipmentOptions.Clear();

            TemplateOptions.Clear();


            SelectedLocation =
                null;

            SelectedEquipment =
                null;

            SelectedTemplate =
                null;


            if (option is null)
            {
                return;
            }


            await LoadLocationsAsync(
                option.Id);


            SelectedLocation =
                LocationOptions
                    .FirstOrDefault();


            if (SelectedLocation
                is null)
            {
                return;
            }


            await LoadEquipmentsAsync(
                SelectedLocation.Id);


            SelectedEquipment =
                EquipmentOptions
                    .FirstOrDefault();


            if (SelectedEquipment?
                    .EquipmentType
                is not EquipmentType
                    equipmentType)
            {
                return;
            }


            await LoadTemplatesAsync(
                equipmentType);


            SelectedTemplate =
                TemplateOptions
                    .FirstOrDefault();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                "設備の選択肢を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            _isPopulatingEditor =
                false;
        }
    }


    // ============================================
    // Location Changed
    // ============================================

    internal async Task HandleLocationChangedAsync(
        ScheduleSelectionOptionViewModel? option)
    {
        try
        {
            _isPopulatingEditor =
                true;

            EditorErrorMessage =
                null;


            EquipmentOptions.Clear();

            TemplateOptions.Clear();


            SelectedEquipment =
                null;

            SelectedTemplate =
                null;


            if (option is null)
            {
                return;
            }


            await LoadEquipmentsAsync(
                option.Id);


            SelectedEquipment =
                EquipmentOptions
                    .FirstOrDefault();


            if (SelectedEquipment?
                    .EquipmentType
                is not EquipmentType
                    equipmentType)
            {
                return;
            }


            await LoadTemplatesAsync(
                equipmentType);


            SelectedTemplate =
                TemplateOptions
                    .FirstOrDefault();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                "設備を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            _isPopulatingEditor =
                false;
        }
    }


    // ============================================
    // Equipment Changed
    // ============================================

    internal async Task HandleEquipmentChangedAsync(
        ScheduleSelectionOptionViewModel? option)
    {
        try
        {
            _isPopulatingEditor =
                true;

            EditorErrorMessage =
                null;


            TemplateOptions.Clear();

            SelectedTemplate =
                null;


            if (option?
                    .EquipmentType
                is not EquipmentType
                    equipmentType)
            {
                return;
            }


            await LoadTemplatesAsync(
                equipmentType);


            SelectedTemplate =
                TemplateOptions
                    .FirstOrDefault();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                "点検票テンプレートを読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            _isPopulatingEditor =
                false;
        }
    }


    // ============================================
    // Locations
    // ============================================

    private async Task LoadLocationsAsync(
        Guid factorySiteId)
    {
        LocationOptions.Clear();


        var locations =
            await _getLocationsAsync(
                factorySiteId);


        foreach (var location
                 in locations)
        {
            var floorText =
                string.IsNullOrWhiteSpace(
                    location.Floor)
                    ? string.Empty
                    : $"（{location.Floor}）";


            LocationOptions.Add(
                new ScheduleSelectionOptionViewModel(
                    location.Id,
                    $"{location.Name}{floorText}"));
        }
    }


    // ============================================
    // Equipments
    // ============================================

    private async Task LoadEquipmentsAsync(
        Guid locationId)
    {
        EquipmentOptions.Clear();


        var equipments =
            await _getEquipmentsAsync(
                locationId);


        foreach (var equipment
                 in equipments)
        {
            EquipmentOptions.Add(
                new ScheduleSelectionOptionViewModel(
                    equipment.Id,
                    $"{equipment.EquipmentCode}  {equipment.Name}",
                    equipment.EquipmentType));
        }
    }


    // ============================================
    // Templates
    // ============================================

    private async Task LoadTemplatesAsync(
        EquipmentType equipmentType)
    {
        TemplateOptions.Clear();


        var templates =
            await _getTemplatesAsync(
                equipmentType);


        foreach (var template
                 in templates)
        {
            TemplateOptions.Add(
                new ScheduleSelectionOptionViewModel(
                    template.Id,
                    $"{template.Name}（v{template.Version}）"));
        }
    }


    // ============================================
    // Validation
    // ============================================

    private bool TryGetEditorInput(
        out DateOnly scheduledDate,
        out Guid equipmentId,
        out Guid templateId,
        out Guid operatorId)
    {
        scheduledDate =
            default;

        equipmentId =
            Guid.Empty;

        templateId =
            Guid.Empty;

        operatorId =
            Guid.Empty;


        if (!EditorScheduledDate.HasValue)
        {
            EditorErrorMessage =
                "点検予定日を選択してください。";

            return false;
        }


        scheduledDate =
            DateOnly.FromDateTime(
                EditorScheduledDate
                    .Value
                    .Date);


        if (scheduledDate <
            _todayProvider())
        {
            EditorErrorMessage =
                "過去の日付は選択できません。";

            return false;
        }


        if (SelectedFactorySite
            is null)
        {
            EditorErrorMessage =
                "工場を選択してください。";

            return false;
        }


        if (SelectedLocation
            is null)
        {
            EditorErrorMessage =
                "設置場所を選択してください。";

            return false;
        }


        if (SelectedEquipment
            is null)
        {
            EditorErrorMessage =
                "設備を選択してください。";

            return false;
        }


        if (SelectedTemplate
            is null)
        {
            EditorErrorMessage =
                "点検票テンプレートを選択してください。";

            return false;
        }


        if (SelectedOperator
            is null)
        {
            EditorErrorMessage =
                "点検担当者を選択してください。";

            return false;
        }


        if (EditorNotes
                .Trim()
                .Length >
            500)
        {
            EditorErrorMessage =
                "備考は500文字以内で入力してください。";

            return false;
        }


        equipmentId =
            SelectedEquipment.Id;

        templateId =
            SelectedTemplate.Id;

        operatorId =
            SelectedOperator.Id;


        return true;
    }


    // ============================================
    // Clear Editor
    // ============================================

    private void ClearEditor()
    {
        EditingScheduleId =
            null;

        EditorScheduledDate =
            null;

        SelectedFactorySite =
            null;

        SelectedLocation =
            null;

        SelectedEquipment =
            null;

        SelectedTemplate =
            null;

        SelectedOperator =
            null;

        EditorNotes =
            string.Empty;


        FactorySiteOptions.Clear();

        LocationOptions.Clear();

        EquipmentOptions.Clear();

        TemplateOptions.Clear();

        OperatorOptions.Clear();
    }


    // ============================================
    // Status
    // ============================================

    private static InspectionStatus GetStatus(
        InspectionSchedule schedule)
    {
        return schedule.Inspection?
                   .Status ??
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


    // ============================================
    // DateOnly -> DateTimeOffset
    // ============================================

    private static DateTimeOffset
        ToDateTimeOffset(
            DateOnly date)
    {
        return new DateTimeOffset(
            date.ToDateTime(
                TimeOnly.MinValue));
    }
}