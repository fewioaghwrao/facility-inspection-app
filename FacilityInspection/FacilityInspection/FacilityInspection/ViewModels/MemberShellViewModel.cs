using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public partial class MemberShellViewModel
    : ViewModelBase
{
    // ============================================
    // Basic
    // ============================================

    private readonly Action
        _logout;

    private readonly Guid
        _operatorId;


    // ============================================
    // ViewModel Factories
    // ============================================

    private readonly Func<
        Guid,
        Action<Guid>,
        ViewModelBase>
        _createDashboardViewModel;

    private readonly Func<
        Guid,
        ViewModelBase>
        _createInspectionListViewModel;

    private readonly Func<
        ViewModelBase>
        _createScheduleCalendarViewModel;

    private readonly Func<
        Guid,
        Guid,
        Action,
        ViewModelBase>
        _createInspectionEntryViewModel;


    // ============================================
    // Cached ViewModels
    // ============================================

    private ViewModelBase?
        _dashboardViewModel;

    private ViewModelBase?
        _scheduleCalendarViewModel;


    // ============================================
    // 表示中コンテンツ
    // ============================================

    [ObservableProperty]
    private ViewModelBase
        currentContent = null!;


    // ============================================
    // ログアウト確認
    // ============================================

    [ObservableProperty]
    private bool isLogoutDialogOpen;


    // ============================================
    // 選択中メニュー
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsInspectionListSelected))]
    private MemberMenuItem
        selectedMenuItem;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public MemberShellViewModel(
        Guid operatorId,
        string operatorName,
        ScheduleRepository scheduleRepository,
        InspectionRepository inspectionRepository,
        Action logout)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            operatorName);

        ArgumentNullException.ThrowIfNull(
            scheduleRepository);

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        ArgumentNullException.ThrowIfNull(
            logout);


        _operatorId =
            operatorId;

        OperatorName =
            operatorName;

        _logout =
            logout;


        // ========================================
        // Dashboard Factory
        // ========================================

        _createDashboardViewModel =
            (
                id,
                openInspection) =>
                new MemberDashboardViewModel(
                    id,
                    scheduleRepository,
                    openInspection);


        // ========================================
        // Inspection List Factory
        // ========================================

        _createInspectionListViewModel =
            id =>
                new MemberInspectionListViewModel(
                    id,
                    inspectionRepository);


        // ========================================
        // Schedule Calendar Factory
        // ========================================

        _createScheduleCalendarViewModel =
            () =>
                new ScheduleCalendarViewModel(
                    scheduleRepository);


        // ========================================
        // Inspection Entry Factory
        // ========================================

        _createInspectionEntryViewModel =
            (
                scheduleId,
                id,
                back) =>
                new InspectionEntryViewModel(
                    scheduleId,
                    id,
                    inspectionRepository,
                    back);


        // ========================================
        // Initial Page
        // ========================================

        CurrentContent =
            GetDashboardViewModel();

        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal MemberShellViewModel(
        Guid operatorId,
        string operatorName,
        Action logout,
        Func<
            Guid,
            Action<Guid>,
            ViewModelBase>
            createDashboardViewModel,
        Func<
            Guid,
            ViewModelBase>
            createInspectionListViewModel,
        Func<
            ViewModelBase>
            createScheduleCalendarViewModel,
        Func<
            Guid,
            Guid,
            Action,
            ViewModelBase>
            createInspectionEntryViewModel)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            operatorName);

        ArgumentNullException.ThrowIfNull(
            logout);

        ArgumentNullException.ThrowIfNull(
            createDashboardViewModel);

        ArgumentNullException.ThrowIfNull(
            createInspectionListViewModel);

        ArgumentNullException.ThrowIfNull(
            createScheduleCalendarViewModel);

        ArgumentNullException.ThrowIfNull(
            createInspectionEntryViewModel);


        _operatorId =
            operatorId;

        OperatorName =
            operatorName;

        _logout =
            logout;

        _createDashboardViewModel =
            createDashboardViewModel;

        _createInspectionListViewModel =
            createInspectionListViewModel;

        _createScheduleCalendarViewModel =
            createScheduleCalendarViewModel;

        _createInspectionEntryViewModel =
            createInspectionEntryViewModel;


        // ========================================
        // Initial Page
        // ========================================

        CurrentContent =
            GetDashboardViewModel();

        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }


    // ============================================
    // ログインユーザー
    // ============================================

    public string OperatorName
    {
        get;
    }


    // ============================================
    // メニュー選択状態
    // ============================================

    public bool IsDashboardSelected =>
        SelectedMenuItem ==
            MemberMenuItem.Dashboard;


    public bool IsInspectionListSelected =>
        SelectedMenuItem ==
            MemberMenuItem.InspectionList;


    // ============================================
    // 点検予定
    // ============================================

    [RelayCommand]
    private void OpenDashboard()
    {
        /*
         * Dashboardは最新状態を表示するため
         * メニューから開くたび再生成する。
         */
        ShowDashboard(
            reload:
                true);
    }


    // ============================================
    // 点検一覧
    // ============================================

    [RelayCommand]
    private void OpenInspectionList()
    {
        /*
         * 点検完了・承認・差し戻し等の状態を
         * 常に最新DBから表示できるよう、
         * メニューを開くたびにVMを作り直す。
         */
        CurrentContent =
            _createInspectionListViewModel(
                _operatorId);


        SelectedMenuItem =
            MemberMenuItem.InspectionList;
    }


    // ============================================
    // カレンダー
    //
    // 現在のサイドメニューには表示しないが、
    // Dashboard等から呼ぶ場合に備えて残す。
    // ============================================

    [RelayCommand]
    private void OpenScheduleCalendar()
    {
        CurrentContent =
            GetScheduleCalendarViewModel();


        SelectedMenuItem =
            MemberMenuItem.ScheduleCalendar;
    }


    // ============================================
    // ログアウト
    // ============================================

    [RelayCommand]
    private void OpenLogoutDialog()
    {
        IsLogoutDialogOpen =
            true;
    }


    [RelayCommand]
    private void CancelLogout()
    {
        IsLogoutDialogOpen =
            false;
    }


    [RelayCommand]
    private void ConfirmLogout()
    {
        IsLogoutDialogOpen =
            false;


        _logout();
    }


    // ============================================
    // Dashboard
    // ============================================

    private ViewModelBase
        GetDashboardViewModel()
    {
        return _dashboardViewModel ??=
            _createDashboardViewModel(
                _operatorId,
                OpenInspection);
    }


    // ============================================
    // Schedule Calendar
    // ============================================

    private ViewModelBase
        GetScheduleCalendarViewModel()
    {
        return _scheduleCalendarViewModel ??=
            _createScheduleCalendarViewModel();
    }


    // ============================================
    // 点検実施
    // ============================================

    private void OpenInspection(
        Guid scheduleId)
    {
        CurrentContent =
            _createInspectionEntryViewModel(
                scheduleId,
                _operatorId,
                ReturnFromInspection);


        /*
         * 点検実施画面は「点検予定」から開始するため、
         * 左メニュー上は点検予定を選択状態にする。
         */
        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }


    // ============================================
    // 点検実施から戻る
    // ============================================

    private void ReturnFromInspection()
    {
        /*
         * 完了後の件数・ステータスを反映するため
         * Dashboardを再生成する。
         */
        ShowDashboard(
            reload:
                true);
    }


    // ============================================
    // Dashboard表示
    // ============================================

    private void ShowDashboard(
        bool reload)
    {
        if (reload)
        {
            _dashboardViewModel =
                null;
        }


        CurrentContent =
            GetDashboardViewModel();


        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }
}
