using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public partial class MemberShellViewModel
    : ViewModelBase
{
    private readonly Action
        _logout;

    private readonly Guid
        _operatorId;

    private readonly ScheduleRepository
        _scheduleRepository;

    private readonly InspectionRepository
        _inspectionRepository;

    private MemberDashboardViewModel?
        _dashboardViewModel;

    private ScheduleCalendarViewModel?
        _scheduleCalendarViewModel;


    // ============================================
    // 表示中コンテンツ
    // ============================================

    [ObservableProperty]
    private ViewModelBase currentContent;


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
    private MemberMenuItem selectedMenuItem;


    // ============================================
    // Constructor
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

        _scheduleRepository =
            scheduleRepository;

        _inspectionRepository =
            inspectionRepository;

        _logout =
            logout;

        currentContent =
            GetDashboardViewModel();

        selectedMenuItem =
            MemberMenuItem.Dashboard;
    }


    // ============================================
    // ログインユーザー
    // ============================================

    public string OperatorName { get; }


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
        ShowDashboard(
            reload: true);
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
            new MemberInspectionListViewModel(
                _operatorId,
                _inspectionRepository);

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

    private MemberDashboardViewModel
        GetDashboardViewModel()
    {
        return _dashboardViewModel ??=
            new MemberDashboardViewModel(
                _operatorId,
                _scheduleRepository,
                OpenInspection);
    }


    // ============================================
    // Schedule Calendar
    // ============================================

    private ScheduleCalendarViewModel
        GetScheduleCalendarViewModel()
    {
        return _scheduleCalendarViewModel ??=
            new ScheduleCalendarViewModel(
                _scheduleRepository);
    }


    // ============================================
    // 点検実施
    // ============================================

    private void OpenInspection(
        Guid scheduleId)
    {
        CurrentContent =
            new InspectionEntryViewModel(
                scheduleId,
                _operatorId,
                _inspectionRepository,
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
            reload: true);
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
