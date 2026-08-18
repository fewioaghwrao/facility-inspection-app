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
    // ============================================
    // Session
    // ============================================

    private readonly Action<SignedInOperator>
        _signIn;

    private readonly Action
        _signOut;


    // ============================================
    // ViewModel Factories
    // ============================================

    private readonly Func<
        Action<SignedInOperator>,
        ViewModelBase>
        _createLoginViewModel;

    private readonly Func<
        SignedInOperator,
        Action,
        ViewModelBase>
        _createMemberShellViewModel;

    private readonly Func<
        SignedInOperator,
        Action,
        Action,
        ViewModelBase>
        _createAdminShellViewModel;


    // ============================================
    // Current Page
    // ============================================

    [ObservableProperty]
    private ViewModelBase
        currentPage = null!;


    // ============================================
    // Constructor
    // 本番用
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


        // ========================================
        // Session
        // ========================================

        _signIn =
            currentUserSession.SignIn;

        _signOut =
            currentUserSession.SignOut;


        // ========================================
        // Login Factory
        // ========================================

        _createLoginViewModel =
            loginSucceeded =>
            {
                var loginViewModel =
                    new LoginViewModel(
                        authenticationService);

                loginViewModel.LoginSucceeded =
                    loginSucceeded;

                return loginViewModel;
            };


        // ========================================
        // Member Shell Factory
        // ========================================

        _createMemberShellViewModel =
            (
                signedInOperator,
                logout) =>
            {
                return new MemberShellViewModel(
                    signedInOperator.Id,
                    signedInOperator.DisplayName,
                    scheduleRepository,
                    inspectionRepository,
                    logout);
            };


        // ========================================
        // Admin Shell Factory
        // ========================================

        _createAdminShellViewModel =
            (
                signedInOperator,
                logout,
                restoreCompleted) =>
            {
                // --------------------------------
                // Dashboard
                // --------------------------------

                var adminDashboardViewModel =
                    new AdminDashboardViewModel(
                        signedInOperator.DisplayName,
                        inspectionRepository);


                // --------------------------------
                // 設備台帳
                // --------------------------------

                var equipmentManagementViewModel =
                    new EquipmentManagementViewModel();


                // --------------------------------
                // 点検予定管理
                // --------------------------------

                var scheduleCalendarViewModel =
                    new ScheduleCalendarViewModel(
                        scheduleRepository);


                // --------------------------------
                // 点検実施状況
                // --------------------------------

                var inspectionStatusViewModel =
                    new InspectionStatusViewModel(
                        inspectionRepository);


                // --------------------------------
                // 異常一覧
                // --------------------------------

                var abnormalListViewModel =
                    new AbnormalListViewModel(
                        inspectionRepository);


                // --------------------------------
                // 未実施一覧
                // --------------------------------

                var notStartedListViewModel =
                    new NotStartedListViewModel(
                        inspectionRepository);


                // --------------------------------
                // 承認待ち一覧
                // --------------------------------

                var approvalPendingListViewModel =
                    new ApprovalPendingListViewModel(
                        inspectionRepository);


                // --------------------------------
                // 点検表テンプレート
                // --------------------------------

                var inspectionTemplateManagementViewModel =
                    new InspectionTemplateManagementViewModel(
                        inspectionTemplateRepository);


                // --------------------------------
                // 点検担当者管理
                // --------------------------------

                var operatorManagementViewModel =
                    new OperatorManagementViewModel(
                        operatorRepository,
                        signedInOperator.Id);


                // --------------------------------
                // 監査ログ
                // --------------------------------

                var auditLogViewModel =
                    new AuditLogViewModel(
                        auditLogRepository);


                // --------------------------------
                // バックアップ・復元
                // --------------------------------

                var backupRestoreViewModel =
                    new BackupRestoreViewModel(
                        databaseBackupService,
                        backupFilePickerService,
                        auditLogRepository,
                        signedInOperator.Id);


                /*
                 * 復元後はMainViewModelから渡された
                 * callbackを実行する。
                 *
                 * MainViewModel側では
                 * AdminShell全体を新しく作り直す。
                 */
                backupRestoreViewModel.RestoreCompleted =
                    restoreCompleted;


                // --------------------------------
                // Admin Shell
                // --------------------------------

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
                    inspectionRepository,
                    logout);
            };


        // ========================================
        // Initial Page
        // ========================================

        CurrentPage =
            CreateLoginViewModel();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal MainViewModel(
        Action<SignedInOperator> signIn,
        Action signOut,
        Func<
            Action<SignedInOperator>,
            ViewModelBase>
            createLoginViewModel,
        Func<
            SignedInOperator,
            Action,
            ViewModelBase>
            createMemberShellViewModel,
        Func<
            SignedInOperator,
            Action,
            Action,
            ViewModelBase>
            createAdminShellViewModel)
    {
        ArgumentNullException.ThrowIfNull(
            signIn);

        ArgumentNullException.ThrowIfNull(
            signOut);

        ArgumentNullException.ThrowIfNull(
            createLoginViewModel);

        ArgumentNullException.ThrowIfNull(
            createMemberShellViewModel);

        ArgumentNullException.ThrowIfNull(
            createAdminShellViewModel);


        _signIn =
            signIn;

        _signOut =
            signOut;

        _createLoginViewModel =
            createLoginViewModel;

        _createMemberShellViewModel =
            createMemberShellViewModel;

        _createAdminShellViewModel =
            createAdminShellViewModel;


        /*
         * テスト用でもMainViewModelの仕様として
         * 初期画面はLogin画面。
         *
         * 子ViewModel側の自動ロードは
         * Factoryによって回避できる。
         */
        CurrentPage =
            CreateLoginViewModel();
    }


    // ============================================
    // Login
    // ============================================

    private ViewModelBase
        CreateLoginViewModel()
    {
        return _createLoginViewModel(
            OnLoginSucceeded);
    }


    private void OnLoginSucceeded(
        SignedInOperator signedInOperator)
    {
        ArgumentNullException.ThrowIfNull(
            signedInOperator);


        _signIn(
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
                        "未対応の権限です: " +
                        signedInOperator.Role)
            };


        NavigateTo(
            destination);
    }


    // ============================================
    // Member Shell
    // ============================================

    private ViewModelBase
        CreateMemberShellViewModel(
            SignedInOperator signedInOperator)
    {
        return _createMemberShellViewModel(
            signedInOperator,
            Logout);
    }


    // ============================================
    // Admin Shell
    // ============================================

    private ViewModelBase
        CreateAdminShellViewModel(
            SignedInOperator signedInOperator)
    {
        /*
         * restoreCompletedが呼ばれた場合、
         * 同じログインユーザーでAdminShell全体を再生成する。
         */
        void RestoreCompleted()
        {
            NavigateTo(
                CreateAdminShellViewModel(
                    signedInOperator));
        }


        return _createAdminShellViewModel(
            signedInOperator,
            Logout,
            RestoreCompleted);
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
        _signOut();


        NavigateTo(
            CreateLoginViewModel());
    }
}