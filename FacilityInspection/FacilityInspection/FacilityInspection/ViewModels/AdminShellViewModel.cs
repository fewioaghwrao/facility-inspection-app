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

    private readonly ViewModelBase
        _dashboardViewModel;

    private readonly ViewModelBase
        _equipmentManagementViewModel;

    private readonly ViewModelBase
        _scheduleCalendarViewModel;

    private readonly ViewModelBase
        _inspectionStatusViewModel;

    private readonly ViewModelBase
        _abnormalListViewModel;

    private readonly ViewModelBase
        _inspectionTemplateManagementViewModel;

    private readonly ViewModelBase
        _operatorManagementViewModel;

    private readonly ViewModelBase
        _notStartedListViewModel;

    private readonly ViewModelBase
        _auditLogViewModel;

    private readonly ViewModelBase
        _approvalPendingListViewModel;

    private readonly ViewModelBase
        _backupRestoreViewModel;

    private readonly Guid
        _operatorId;

    private readonly Action
        _refreshDashboard;

    private readonly Action
        _reloadApprovalPending;

    private readonly Func<
        Guid,
        Action,
        ViewModelBase>
        _createInspectionDetailViewModel;

    private readonly Func<
        Guid,
        Guid,
        Action,
        ViewModelBase>
        _createApprovalPendingDetailViewModel;


    // ============================================
    // Constructor
    // 本番用
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
        BackupRestoreViewModel backupRestoreViewModel,
        InspectionTemplateManagementViewModel
            inspectionTemplateManagementViewModel,
        OperatorManagementViewModel
            operatorManagementViewModel,
        InspectionRepository inspectionRepository,
        Action logoutRequested)
        : this(
            operatorId,
            displayName,
            dashboardViewModel,
            equipmentManagementViewModel,
            scheduleCalendarViewModel,
            inspectionStatusViewModel,
            abnormalListViewModel,
            notStartedListViewModel,
            auditLogViewModel,
            approvalPendingListViewModel,
            backupRestoreViewModel,
            inspectionTemplateManagementViewModel,
            operatorManagementViewModel,

            refreshDashboard:
                () =>
                    dashboardViewModel
                        .Refresh(),

            reloadApprovalPending:
                () =>
                    approvalPendingListViewModel
                        .ReloadCommand
                        .Execute(null),

            createInspectionDetailViewModel:
                (scheduleId, backRequested) =>
                {
                    var detailViewModel =
                        new InspectionDetailViewModel(
                            inspectionRepository,
                            scheduleId);

                    detailViewModel.BackRequested =
                        backRequested;

                    return detailViewModel;
                },

            createApprovalPendingDetailViewModel:
                (
                    scheduleId,
                    currentOperatorId,
                    backRequested) =>
                {
                    var detailViewModel =
                        new ApprovalPendingDetailViewModel(
                            inspectionRepository,
                            scheduleId,
                            currentOperatorId);

                    detailViewModel.BackRequested =
                        backRequested;

                    return detailViewModel;
                },

            logoutRequested)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);


        // ========================================
        // 点検実施状況 → 点検詳細
        // ========================================

        inspectionStatusViewModel.DetailRequested =
            OpenInspectionDetail;


        // ========================================
        // 異常一覧 → 点検詳細
        // ========================================

        abnormalListViewModel.DetailRequested =
            OpenAbnormalDetail;


        // ========================================
        // 未実施一覧 → 点検詳細
        // ========================================

        notStartedListViewModel.DetailRequested =
            OpenNotStartedDetail;


        // ========================================
        // 完了・承認待ち一覧 → 点検詳細
        // ========================================

        approvalPendingListViewModel.DetailRequested =
            OpenApprovalPendingDetail;


        // ========================================
        // Dashboard → 各管理画面
        // ========================================

        dashboardViewModel.InspectionStatusRequested =
            OpenInspectionStatus;

        dashboardViewModel.NotStartedRequested =
            OpenNotStartedList;

        dashboardViewModel.ApprovalPendingRequested =
            OpenApprovalPending;

        dashboardViewModel.AbnormalListRequested =
            OpenAbnormalList;
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal AdminShellViewModel(
        Guid operatorId,
        string displayName,
        ViewModelBase dashboardViewModel,
        ViewModelBase equipmentManagementViewModel,
        ViewModelBase scheduleCalendarViewModel,
        ViewModelBase inspectionStatusViewModel,
        ViewModelBase abnormalListViewModel,
        ViewModelBase notStartedListViewModel,
        ViewModelBase auditLogViewModel,
        ViewModelBase approvalPendingListViewModel,
        ViewModelBase backupRestoreViewModel,
        ViewModelBase inspectionTemplateManagementViewModel,
        ViewModelBase operatorManagementViewModel,
        Action refreshDashboard,
        Action reloadApprovalPending,
        Func<
            Guid,
            Action,
            ViewModelBase>
            createInspectionDetailViewModel,
        Func<
            Guid,
            Guid,
            Action,
            ViewModelBase>
            createApprovalPendingDetailViewModel,
        Action logoutRequested)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

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
            backupRestoreViewModel);

        ArgumentNullException.ThrowIfNull(
            inspectionTemplateManagementViewModel);

        ArgumentNullException.ThrowIfNull(
            operatorManagementViewModel);

        ArgumentNullException.ThrowIfNull(
            refreshDashboard);

        ArgumentNullException.ThrowIfNull(
            reloadApprovalPending);

        ArgumentNullException.ThrowIfNull(
            createInspectionDetailViewModel);

        ArgumentNullException.ThrowIfNull(
            createApprovalPendingDetailViewModel);

        ArgumentNullException.ThrowIfNull(
            logoutRequested);


        _operatorId =
            operatorId;

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

        _backupRestoreViewModel =
            backupRestoreViewModel;

        _inspectionTemplateManagementViewModel =
            inspectionTemplateManagementViewModel;

        _operatorManagementViewModel =
            operatorManagementViewModel;

        _refreshDashboard =
            refreshDashboard;

        _reloadApprovalPending =
            reloadApprovalPending;

        _createInspectionDetailViewModel =
            createInspectionDetailViewModel;

        _createApprovalPendingDetailViewModel =
            createApprovalPendingDetailViewModel;

        _logoutRequested =
            logoutRequested;


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

    public string DisplayName
    {
        get;
    }


    // ============================================
    // Current Content
    // ============================================

    [ObservableProperty]
    private ViewModelBase currentContent =
        null!;


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
    [NotifyPropertyChangedFor(
        nameof(IsBackupRestoreSelected))]
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

    public bool IsBackupRestoreSelected =>
        SelectedMenu ==
        AdminMenuItem.BackupRestore;


    // ============================================
    // Dashboard
    // ============================================

    [RelayCommand]
    private void OpenDashboard()
    {
        /*
         * 他画面で点検状態が変更されている可能性があるため、
         * ダッシュボードへ戻るたびに最新状態を取得する。
         */
        _refreshDashboard();

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

    internal void OpenInspectionDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            _createInspectionDetailViewModel(
                scheduleId,
                OpenInspectionStatus);

        CurrentContent =
            detailViewModel;

        SelectedMenu =
            AdminMenuItem.InspectionStatus;
    }


    // ============================================
    // Abnormal List → Detail
    // ============================================

    internal void OpenAbnormalDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            _createInspectionDetailViewModel(
                scheduleId,
                OpenAbnormalList);

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

    internal void OpenNotStartedDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            _createInspectionDetailViewModel(
                scheduleId,
                OpenNotStartedList);

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
        _reloadApprovalPending();

        CurrentContent =
            _approvalPendingListViewModel;

        SelectedMenu =
            AdminMenuItem.ApprovalPending;
    }


    // ============================================
    // Approval Pending List → Detail
    // ============================================

    internal void OpenApprovalPendingDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }

        var detailViewModel =
            _createApprovalPendingDetailViewModel(
                scheduleId,
                _operatorId,
                OpenApprovalPending);

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
    // Backup / Restore
    // ============================================

    [RelayCommand]
    private void OpenBackupRestore()
    {
        CurrentContent =
            _backupRestoreViewModel;

        SelectedMenu =
            AdminMenuItem.BackupRestore;
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