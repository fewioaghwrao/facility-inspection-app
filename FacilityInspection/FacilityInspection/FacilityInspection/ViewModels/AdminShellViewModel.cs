using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class AdminShellViewModel
    : ViewModelBase
{
    private readonly Action
        _logoutRequested;

    private readonly AdminDashboardViewModel
        _dashboardViewModel;

    private readonly EquipmentManagementViewModel
        _equipmentManagementViewModel;

    private readonly ScheduleCalendarViewModel
        _scheduleCalendarViewModel;

    private readonly InspectionStatusViewModel
        _inspectionStatusViewModel;

    private readonly AbnormalListViewModel
        _abnormalListViewModel;

    private readonly InspectionTemplateManagementViewModel
        _inspectionTemplateManagementViewModel;

    private readonly OperatorManagementViewModel
        _operatorManagementViewModel;

    private readonly InspectionRepository
        _inspectionRepository;

    private readonly NotStartedListViewModel
        _notStartedListViewModel;

    private readonly AuditLogViewModel
        _auditLogViewModel;

    private readonly ApprovalPendingListViewModel
        _approvalPendingListViewModel;

    private readonly Guid
    _operatorId;


    // ============================================
    // Constructor
    // ============================================

    public AdminShellViewModel(
        Guid operatorId,
        string displayName,
        AdminDashboardViewModel dashboardViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel,
        ScheduleCalendarViewModel scheduleCalendarViewModel,
        InspectionStatusViewModel inspectionStatusViewModel,
        AbnormalListViewModel abnormalListViewModel,
        NotStartedListViewModel notStartedListViewModel,
        AuditLogViewModel auditLogViewModel,
        ApprovalPendingListViewModel approvalPendingListViewModel,
        InspectionTemplateManagementViewModel
            inspectionTemplateManagementViewModel,
        OperatorManagementViewModel
            operatorManagementViewModel,
        InspectionRepository inspectionRepository,
        Action logoutRequested)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        ArgumentNullException.ThrowIfNull(
            dashboardViewModel);

        ArgumentNullException.ThrowIfNull(
            equipmentManagementViewModel);

        ArgumentNullException.ThrowIfNull(
            scheduleCalendarViewModel);

        ArgumentNullException.ThrowIfNull(
            inspectionStatusViewModel);

        ArgumentNullException.ThrowIfNull(
            abnormalListViewModel);

        ArgumentNullException.ThrowIfNull(
            notStartedListViewModel);

        ArgumentNullException.ThrowIfNull(
            auditLogViewModel);

        ArgumentNullException.ThrowIfNull(
            approvalPendingListViewModel);

        ArgumentNullException.ThrowIfNull(
            inspectionTemplateManagementViewModel);

        ArgumentNullException.ThrowIfNull(
            operatorManagementViewModel);

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        ArgumentNullException.ThrowIfNull(
            logoutRequested);


        DisplayName =
            displayName;

        _dashboardViewModel =
            dashboardViewModel;

        _equipmentManagementViewModel =
            equipmentManagementViewModel;

        _scheduleCalendarViewModel =
            scheduleCalendarViewModel;

        _inspectionStatusViewModel =
            inspectionStatusViewModel;

        _abnormalListViewModel =
            abnormalListViewModel;

        _notStartedListViewModel =
            notStartedListViewModel;

        _auditLogViewModel =
            auditLogViewModel;

        _approvalPendingListViewModel =
            approvalPendingListViewModel;

        _inspectionTemplateManagementViewModel =
            inspectionTemplateManagementViewModel;

        _operatorManagementViewModel =
            operatorManagementViewModel;

        _inspectionRepository =
            inspectionRepository;

        _logoutRequested =
            logoutRequested;


        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        _operatorId =
            operatorId;

        // ========================================
        // 点検実施状況 → 点検詳細
        // ========================================

        _inspectionStatusViewModel.DetailRequested =
            OpenInspectionDetail;


        // ========================================
        // 異常一覧 → 点検詳細
        // ========================================

        _abnormalListViewModel.DetailRequested =
            OpenAbnormalDetail;


        // ========================================
        // 未実施一覧 → 点検詳細
        // ========================================

        _notStartedListViewModel.DetailRequested =
            OpenNotStartedDetail;


        // ========================================
        // 完了・承認待ち一覧 → 点検詳細
        // ========================================

        _approvalPendingListViewModel.DetailRequested =
            OpenApprovalPendingDetail;


        // ========================================
        // 初期画面
        // ========================================

        CurrentContent =
            _dashboardViewModel;

        SelectedMenu =
            AdminMenuItem.Dashboard;
    }


    // ============================================
    // Header
    // ============================================

    public string DisplayName { get; }


    // ============================================
    // Current Content
    // ============================================

    [ObservableProperty]
    private ViewModelBase currentContent = null!;


    // ============================================
    // Selected Menu
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsEquipmentManagementSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsInspectionTemplateManagementSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsScheduleCalendarSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsInspectionStatusSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsAbnormalListSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsOperatorManagementSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsNotStartedListSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsApprovalPendingSelected))]
    [NotifyPropertyChangedFor(
        nameof(IsAuditLogSelected))]
    private AdminMenuItem selectedMenu;


    // ============================================
    // Logout Dialog
    // ============================================

    [ObservableProperty]
    private bool isLogoutDialogOpen;


    // ============================================
    // Menu Selection State
    // ============================================

    public bool IsDashboardSelected =>
        SelectedMenu ==
        AdminMenuItem.Dashboard;

    public bool IsEquipmentManagementSelected =>
        SelectedMenu ==
        AdminMenuItem.EquipmentManagement;

    public bool
        IsInspectionTemplateManagementSelected =>
            SelectedMenu ==
            AdminMenuItem
                .InspectionTemplateManagement;

    public bool IsScheduleCalendarSelected =>
        SelectedMenu ==
        AdminMenuItem.ScheduleCalendar;

    public bool IsInspectionStatusSelected =>
        SelectedMenu ==
        AdminMenuItem.InspectionStatus;

    public bool IsAbnormalListSelected =>
        SelectedMenu ==
        AdminMenuItem.AbnormalList;

    public bool IsOperatorManagementSelected =>
        SelectedMenu ==
        AdminMenuItem.OperatorManagement;

    public bool IsNotStartedListSelected =>
        SelectedMenu ==
        AdminMenuItem.NotStartedList;

    public bool IsApprovalPendingSelected =>
        SelectedMenu ==
        AdminMenuItem.ApprovalPending;

    public bool IsAuditLogSelected =>
        SelectedMenu ==
        AdminMenuItem.AuditLog;


    // ============================================
    // Dashboard
    // ============================================

    [RelayCommand]
    private void OpenDashboard()
    {
        CurrentContent =
            _dashboardViewModel;

        SelectedMenu =
            AdminMenuItem.Dashboard;
    }


    // ============================================
    // Equipment Management
    // ============================================

    [RelayCommand]
    private void OpenEquipmentManagement()
    {
        CurrentContent =
            _equipmentManagementViewModel;

        SelectedMenu =
            AdminMenuItem.EquipmentManagement;
    }


    // ============================================
    // Schedule Calendar
    // ============================================

    [RelayCommand]
    private void OpenScheduleCalendar()
    {
        CurrentContent =
            _scheduleCalendarViewModel;

        SelectedMenu =
            AdminMenuItem.ScheduleCalendar;
    }


    // ============================================
    // Inspection Status
    // ============================================

    [RelayCommand]
    private void OpenInspectionStatus()
    {
        CurrentContent =
            _inspectionStatusViewModel;

        SelectedMenu =
            AdminMenuItem.InspectionStatus;
    }


    // ============================================
    // Abnormal List
    // ============================================

    [RelayCommand]
    private void OpenAbnormalList()
    {
        CurrentContent =
            _abnormalListViewModel;

        SelectedMenu =
            AdminMenuItem.AbnormalList;
    }


    // ============================================
    // Inspection Status → Detail
    // ============================================

    private void OpenInspectionDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            new InspectionDetailViewModel(
                _inspectionRepository,
                scheduleId);

        /*
         * 点検実施状況から詳細を開いた場合は、
         * 戻るボタンで点検実施状況へ戻る。
         */
        detailViewModel.BackRequested =
            OpenInspectionStatus;

        CurrentContent =
            detailViewModel;

        SelectedMenu =
            AdminMenuItem.InspectionStatus;
    }


    // ============================================
    // Abnormal List → Detail
    // ============================================

    private void OpenAbnormalDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            new InspectionDetailViewModel(
                _inspectionRepository,
                scheduleId);

        /*
         * 異常一覧から詳細を開いた場合は、
         * 戻るボタンで異常一覧へ戻る。
         */
        detailViewModel.BackRequested =
            OpenAbnormalList;

        CurrentContent =
            detailViewModel;

        SelectedMenu =
            AdminMenuItem.AbnormalList;
    }


    // ============================================
    // Inspection Template Management
    // ============================================

    [RelayCommand]
    private void OpenInspectionTemplateManagement()
    {
        CurrentContent =
            _inspectionTemplateManagementViewModel;

        SelectedMenu =
            AdminMenuItem
                .InspectionTemplateManagement;
    }


    // ============================================
    // Operator Management
    // ============================================

    [RelayCommand]
    private void OpenOperatorManagement()
    {
        CurrentContent =
            _operatorManagementViewModel;

        SelectedMenu =
            AdminMenuItem.OperatorManagement;
    }


    // ============================================
    // Not Started List
    // ============================================

    [RelayCommand]
    private void OpenNotStartedList()
    {
        CurrentContent =
            _notStartedListViewModel;

        SelectedMenu =
            AdminMenuItem.NotStartedList;
    }


    // ============================================
    // Not Started List → Detail
    // ============================================

    private void OpenNotStartedDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            new InspectionDetailViewModel(
                _inspectionRepository,
                scheduleId);

        /*
         * 未実施一覧から開いた場合は、
         * 戻るボタンで未実施一覧へ戻す。
         */
        detailViewModel.BackRequested =
            OpenNotStartedList;

        CurrentContent =
            detailViewModel;

        SelectedMenu =
            AdminMenuItem.NotStartedList;
    }


    // ============================================
    // Approval Pending List
    // ============================================

    [RelayCommand]
    private void OpenApprovalPending()
    {
        /*
         * 承認や差し戻し後に一覧へ戻ったとき、
         * 最新の状態を取得する。
         */
        _approvalPendingListViewModel
            .ReloadCommand
            .Execute(null);

        CurrentContent =
            _approvalPendingListViewModel;

        SelectedMenu =
            AdminMenuItem.ApprovalPending;
    }


    // ============================================
    // Approval Pending List → Detail
    // ============================================

    private void OpenApprovalPendingDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            new ApprovalPendingDetailViewModel(
                _inspectionRepository,
                scheduleId,
                _operatorId);

        detailViewModel.BackRequested =
            OpenApprovalPending;

        CurrentContent =
            detailViewModel;

        SelectedMenu =
            AdminMenuItem.ApprovalPending;
    }

    // ============================================
    // Audit Log
    // ============================================

    [RelayCommand]
    private void OpenAuditLog()
    {
        CurrentContent =
            _auditLogViewModel;

        SelectedMenu =
            AdminMenuItem.AuditLog;
    }


    // ============================================
    // Logout
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

        _logoutRequested();
    }
}