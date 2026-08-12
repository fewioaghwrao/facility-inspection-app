using CommunityToolkit.Mvvm.ComponentModel;
using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using FacilityInspection.Services.Backup;
using System;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel
    : ViewModelBase
{
    private readonly IAuthenticationService
        _authenticationService;

    private readonly CurrentUserSession
        _currentUserSession;

    private readonly InspectionTemplateRepository
        _inspectionTemplateRepository;

    private readonly OperatorRepository
        _operatorRepository;

    private readonly ScheduleRepository
        _scheduleRepository;

    private readonly InspectionRepository
        _inspectionRepository;
    private readonly AuditLogRepository
    _auditLogRepository;

    private readonly DatabaseBackupService
    _databaseBackupService;

    private readonly BackupFilePickerService
        _backupFilePickerService;

    // ============================================
    // Current Page
    // ============================================

    [ObservableProperty]
    private ViewModelBase currentPage = null!;


    // ============================================
    // Constructor
    // ============================================

    public MainViewModel(
        IAuthenticationService authenticationService,
        CurrentUserSession currentUserSession,
        InspectionTemplateRepository inspectionTemplateRepository,
        OperatorRepository operatorRepository,
        ScheduleRepository scheduleRepository,
        InspectionRepository inspectionRepository,
        AuditLogRepository auditLogRepository,
        DatabaseBackupService databaseBackupService,
        BackupFilePickerService backupFilePickerService)
    {
        ArgumentNullException.ThrowIfNull(
            authenticationService);

        ArgumentNullException.ThrowIfNull(
            currentUserSession);

        ArgumentNullException.ThrowIfNull(
            inspectionTemplateRepository);

        ArgumentNullException.ThrowIfNull(
            operatorRepository);

        ArgumentNullException.ThrowIfNull(
            scheduleRepository);

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        ArgumentNullException.ThrowIfNull(
    auditLogRepository);

        ArgumentNullException.ThrowIfNull(
    databaseBackupService);

        ArgumentNullException.ThrowIfNull(
            backupFilePickerService);

        _databaseBackupService =
            databaseBackupService;

        _backupFilePickerService =
            backupFilePickerService;

        _authenticationService =
            authenticationService;

        _currentUserSession =
            currentUserSession;

        _inspectionTemplateRepository =
            inspectionTemplateRepository;

        _operatorRepository =
            operatorRepository;

        _scheduleRepository =
            scheduleRepository;

        _inspectionRepository =
            inspectionRepository;

        _auditLogRepository =
    auditLogRepository;

        // 初期画面
        CurrentPage =
            CreateLoginViewModel();
    }


    // ============================================
    // Login
    // ============================================

    private LoginViewModel CreateLoginViewModel()
    {
        var loginViewModel =
            new LoginViewModel(
                _authenticationService);

        loginViewModel.LoginSucceeded =
            OnLoginSucceeded;

        return loginViewModel;
    }


    private void OnLoginSucceeded(
        SignedInOperator signedInOperator)
    {
        ArgumentNullException.ThrowIfNull(
            signedInOperator);

        _currentUserSession.SignIn(
            signedInOperator);

        ViewModelBase destination =
            signedInOperator.Role switch
            {
                OperatorRole.Inspector =>
                    CreateMemberShellViewModel(
                        signedInOperator),

                OperatorRole.MaintenanceManager =>
                    CreateAdminShellViewModel(
                        signedInOperator),

                _ =>
                    throw new InvalidOperationException(
                        $"未対応の権限です: " +
                        $"{signedInOperator.Role}")
            };

        NavigateTo(
            destination);
    }


    // ============================================
    // Member Shell
    // ============================================

    private MemberShellViewModel
        CreateMemberShellViewModel(
            SignedInOperator signedInOperator)
    {
        return new MemberShellViewModel(
            signedInOperator.Id,
            signedInOperator.DisplayName,
            _scheduleRepository,
            _inspectionRepository,
            Logout);
    }

    // ============================================
    // Admin Shell
    // ============================================

    private AdminShellViewModel
        CreateAdminShellViewModel(
            SignedInOperator signedInOperator)
    {
        // ----------------------------------------
        // Dashboard
        // ----------------------------------------

        var adminDashboardViewModel =
            new AdminDashboardViewModel(
                signedInOperator.DisplayName,
                _inspectionRepository);


        // ----------------------------------------
        // 設備台帳
        // ----------------------------------------

        var equipmentManagementViewModel =
            new EquipmentManagementViewModel();


        // ----------------------------------------
        // 点検予定管理
        // ----------------------------------------

        var scheduleCalendarViewModel =
            new ScheduleCalendarViewModel(
                _scheduleRepository);


        // ----------------------------------------
        // 点検実施状況
        // ----------------------------------------

        var inspectionStatusViewModel =
            new InspectionStatusViewModel(
                _inspectionRepository);


        // ----------------------------------------
        // 異常一覧
        // ----------------------------------------

        var abnormalListViewModel =
            new AbnormalListViewModel(
                _inspectionRepository);


        // ----------------------------------------
        // 未実施一覧
        // ----------------------------------------

        var notStartedListViewModel =
            new NotStartedListViewModel(
                _inspectionRepository);


        // ----------------------------------------
        // 承認待ち一覧
        // ----------------------------------------

        var approvalPendingListViewModel =
            new ApprovalPendingListViewModel(
                _inspectionRepository);


        // ----------------------------------------
        // 点検表テンプレート
        // ----------------------------------------

        var inspectionTemplateManagementViewModel =
            new InspectionTemplateManagementViewModel(
                _inspectionTemplateRepository);


        // ----------------------------------------
        // 点検担当者管理
        // ----------------------------------------

        var operatorManagementViewModel =
            new OperatorManagementViewModel(
                _operatorRepository,
                signedInOperator.Id);


        // ----------------------------------------
        // 監査ログ
        // ----------------------------------------

        var auditLogViewModel =
            new AuditLogViewModel(
                _auditLogRepository);


        // ----------------------------------------
        // バックアップ・復元
        // ----------------------------------------

        var backupRestoreViewModel =
            new BackupRestoreViewModel(
                _databaseBackupService,
                _backupFilePickerService,
                _auditLogRepository,
                signedInOperator.Id);

        /*
         * 復元後は全ViewModelを作り直して、
         * 復元されたDBからデータを再取得する。
         */
        backupRestoreViewModel.RestoreCompleted =
            () =>
                NavigateTo(
                    CreateAdminShellViewModel(
                        signedInOperator));

        // ----------------------------------------
        // Admin Shell
        // ----------------------------------------

        return new AdminShellViewModel(
            signedInOperator.Id,
            signedInOperator.DisplayName,
            adminDashboardViewModel,
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
            _inspectionRepository,
            Logout);
    }


    // ============================================
    // Navigation
    // ============================================

    public void NavigateTo(
        ViewModelBase destination)
    {
        ArgumentNullException.ThrowIfNull(
            destination);

        CurrentPage =
            destination;
    }


    // ============================================
    // Logout
    // ============================================

    public void Logout()
    {
        _currentUserSession.SignOut();

        NavigateTo(
            CreateLoginViewModel());
    }
}