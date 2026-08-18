using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AdminShellViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new TestContext(
                        operatorId:
                            Guid.Empty));

        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithBlankDisplayName_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new TestContext(
                        displayName:
                            "   "));

        // Assert
        Assert.Equal(
            "displayName",
            exception.ParamName);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_SetsDashboardAsInitialContent()
    {
        // Arrange & Act
        var context =
            new TestContext();

        var sut =
            context.Sut;

        // Assert
        Assert.Equal(
            "管理者",
            sut.DisplayName);

        Assert.Same(
            context.Dashboard,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.Dashboard,
            sut.SelectedMenu);

        Assert.True(
            sut.IsDashboardSelected);

        Assert.False(
            sut.IsLogoutDialogOpen);

        AssertMenuSelection(
            sut,
            AdminMenuItem.Dashboard);
    }


    // ============================================
    // Menu Navigation
    // ============================================

    [Theory]
    [InlineData(
        AdminMenuItem.Dashboard)]
    [InlineData(
        AdminMenuItem.InspectionStatus)]
    [InlineData(
        AdminMenuItem.AbnormalList)]
    [InlineData(
        AdminMenuItem.NotStartedList)]
    [InlineData(
        AdminMenuItem.ApprovalPending)]
    [InlineData(
        AdminMenuItem.EquipmentManagement)]
    [InlineData(
        AdminMenuItem.ScheduleCalendar)]
    [InlineData(
        AdminMenuItem.InspectionTemplateManagement)]
    [InlineData(
        AdminMenuItem.OperatorManagement)]
    [InlineData(
        AdminMenuItem.AuditLog)]
    [InlineData(
        AdminMenuItem.BackupRestore)]
    public void MenuCommand_ChangesCurrentContentAndSelectedMenu(
        AdminMenuItem menu)
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        // Act
        ExecuteMenuCommand(
            sut,
            menu);

        // Assert
        Assert.Same(
            context.GetViewModel(
                menu),
            sut.CurrentContent);

        Assert.Equal(
            menu,
            sut.SelectedMenu);

        AssertMenuSelection(
            sut,
            menu);
    }


    // ============================================
    // Dashboard Refresh
    // ============================================

    [Fact]
    public void OpenDashboardCommand_RefreshesDashboard()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenEquipmentManagementCommand
            .Execute(null);

        Assert.Equal(
            0,
            context.RefreshDashboardCallCount);

        // Act
        sut.OpenDashboardCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            context.RefreshDashboardCallCount);

        Assert.Same(
            context.Dashboard,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.Dashboard,
            sut.SelectedMenu);
    }


    // ============================================
    // Approval Pending Reload
    // ============================================

    [Fact]
    public void OpenApprovalPendingCommand_ReloadsApprovalPendingList()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        // Act
        sut.OpenApprovalPendingCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            context.ReloadApprovalPendingCallCount);

        Assert.Same(
            context.ApprovalPending,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.ApprovalPending,
            sut.SelectedMenu);
    }


    // ============================================
    // Inspection Status → Detail
    // ============================================

    [Fact]
    public void OpenInspectionDetail_OpensDetailAndKeepsInspectionStatusSelected()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        var scheduleId =
            Guid.NewGuid();

        // Act
        sut.OpenInspectionDetail(
            scheduleId);

        // Assert
        Assert.Equal(
            scheduleId,
            context.LastInspectionDetailScheduleId);

        Assert.NotNull(
            context.LastInspectionDetail);

        Assert.Same(
            context.LastInspectionDetail,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.InspectionStatus,
            sut.SelectedMenu);

        Assert.True(
            sut.IsInspectionStatusSelected);

        Assert.NotNull(
            context.LastInspectionBackRequested);
    }


    [Fact]
    public void InspectionDetail_BackRequested_ReturnsToInspectionStatus()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenInspectionDetail(
            Guid.NewGuid());

        Assert.NotNull(
            context.LastInspectionBackRequested);

        // Act
        context.LastInspectionBackRequested!();

        // Assert
        Assert.Same(
            context.InspectionStatus,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.InspectionStatus,
            sut.SelectedMenu);
    }


    // ============================================
    // Abnormal List → Detail
    // ============================================

    [Fact]
    public void OpenAbnormalDetail_OpensDetailAndKeepsAbnormalListSelected()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        var scheduleId =
            Guid.NewGuid();

        // Act
        sut.OpenAbnormalDetail(
            scheduleId);

        // Assert
        Assert.Equal(
            scheduleId,
            context.LastInspectionDetailScheduleId);

        Assert.NotNull(
            context.LastInspectionDetail);

        Assert.Same(
            context.LastInspectionDetail,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.AbnormalList,
            sut.SelectedMenu);

        Assert.True(
            sut.IsAbnormalListSelected);

        Assert.NotNull(
            context.LastInspectionBackRequested);
    }


    [Fact]
    public void AbnormalDetail_BackRequested_ReturnsToAbnormalList()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenAbnormalDetail(
            Guid.NewGuid());

        Assert.NotNull(
            context.LastInspectionBackRequested);

        // Act
        context.LastInspectionBackRequested!();

        // Assert
        Assert.Same(
            context.AbnormalList,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.AbnormalList,
            sut.SelectedMenu);
    }


    // ============================================
    // Not Started → Detail
    // ============================================

    [Fact]
    public void OpenNotStartedDetail_OpensDetailAndKeepsNotStartedSelected()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        var scheduleId =
            Guid.NewGuid();

        // Act
        sut.OpenNotStartedDetail(
            scheduleId);

        // Assert
        Assert.Equal(
            scheduleId,
            context.LastInspectionDetailScheduleId);

        Assert.NotNull(
            context.LastInspectionDetail);

        Assert.Same(
            context.LastInspectionDetail,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.NotStartedList,
            sut.SelectedMenu);

        Assert.True(
            sut.IsNotStartedListSelected);

        Assert.NotNull(
            context.LastInspectionBackRequested);
    }


    [Fact]
    public void NotStartedDetail_BackRequested_ReturnsToNotStartedList()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenNotStartedDetail(
            Guid.NewGuid());

        Assert.NotNull(
            context.LastInspectionBackRequested);

        // Act
        context.LastInspectionBackRequested!();

        // Assert
        Assert.Same(
            context.NotStarted,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.NotStartedList,
            sut.SelectedMenu);
    }


    // ============================================
    // Approval Pending → Detail
    // ============================================

    [Fact]
    public void OpenApprovalPendingDetail_OpensApprovalDetail()
    {
        // Arrange
        var operatorId =
            Guid.NewGuid();

        var context =
            new TestContext(
                operatorId:
                    operatorId);

        var sut =
            context.Sut;

        var scheduleId =
            Guid.NewGuid();

        // Act
        sut.OpenApprovalPendingDetail(
            scheduleId);

        // Assert
        Assert.Equal(
            scheduleId,
            context.LastApprovalDetailScheduleId);

        Assert.Equal(
            operatorId,
            context.LastApprovalDetailOperatorId);

        Assert.NotNull(
            context.LastApprovalDetail);

        Assert.Same(
            context.LastApprovalDetail,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.ApprovalPending,
            sut.SelectedMenu);

        Assert.True(
            sut.IsApprovalPendingSelected);

        Assert.NotNull(
            context.LastApprovalBackRequested);
    }


    [Fact]
    public void ApprovalPendingDetail_BackRequested_ReturnsToListAndReloads()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenApprovalPendingDetail(
            Guid.NewGuid());

        Assert.Equal(
            0,
            context.ReloadApprovalPendingCallCount);

        Assert.NotNull(
            context.LastApprovalBackRequested);

        // Act
        context.LastApprovalBackRequested!();

        // Assert
        Assert.Equal(
            1,
            context.ReloadApprovalPendingCallCount);

        Assert.Same(
            context.ApprovalPending,
            sut.CurrentContent);

        Assert.Equal(
            AdminMenuItem.ApprovalPending,
            sut.SelectedMenu);
    }


    // ============================================
    // Empty ScheduleId
    // ============================================

    [Theory]
    [InlineData(
        AdminMenuItem.InspectionStatus)]
    [InlineData(
        AdminMenuItem.AbnormalList)]
    [InlineData(
        AdminMenuItem.NotStartedList)]
    [InlineData(
        AdminMenuItem.ApprovalPending)]
    public void OpenDetail_WithEmptyScheduleId_DoesNothing(
        AdminMenuItem source)
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        var initialContent =
            sut.CurrentContent;

        var initialMenu =
            sut.SelectedMenu;

        // Act
        switch (source)
        {
            case AdminMenuItem.InspectionStatus:
                sut.OpenInspectionDetail(
                    Guid.Empty);
                break;

            case AdminMenuItem.AbnormalList:
                sut.OpenAbnormalDetail(
                    Guid.Empty);
                break;

            case AdminMenuItem.NotStartedList:
                sut.OpenNotStartedDetail(
                    Guid.Empty);
                break;

            case AdminMenuItem.ApprovalPending:
                sut.OpenApprovalPendingDetail(
                    Guid.Empty);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(source),
                    source,
                    null);
        }

        // Assert
        Assert.Same(
            initialContent,
            sut.CurrentContent);

        Assert.Equal(
            initialMenu,
            sut.SelectedMenu);

        Assert.Null(
            context.LastInspectionDetail);

        Assert.Null(
            context.LastApprovalDetail);
    }


    // ============================================
    // Logout Dialog
    // ============================================

    [Fact]
    public void OpenLogoutDialogCommand_OpensDialog()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        // Act
        sut.OpenLogoutDialogCommand
            .Execute(null);

        // Assert
        Assert.True(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            0,
            context.LogoutCallCount);
    }


    [Fact]
    public void CancelLogoutCommand_ClosesDialogWithoutLogout()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenLogoutDialogCommand
            .Execute(null);

        Assert.True(
            sut.IsLogoutDialogOpen);

        // Act
        sut.CancelLogoutCommand
            .Execute(null);

        // Assert
        Assert.False(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            0,
            context.LogoutCallCount);
    }


    [Fact]
    public void ConfirmLogoutCommand_ClosesDialogAndRequestsLogout()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        sut.OpenLogoutDialogCommand
            .Execute(null);

        // Act
        sut.ConfirmLogoutCommand
            .Execute(null);

        // Assert
        Assert.False(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            1,
            context.LogoutCallCount);
    }


    [Fact]
    public void ConfirmLogoutCommand_InvokesLogoutOncePerExecution()
    {
        // Arrange
        var context =
            new TestContext();

        var sut =
            context.Sut;

        // Act
        sut.ConfirmLogoutCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            context.LogoutCallCount);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static void ExecuteMenuCommand(
        AdminShellViewModel sut,
        AdminMenuItem menu)
    {
        switch (menu)
        {
            case AdminMenuItem.Dashboard:
                sut.OpenDashboardCommand
                    .Execute(null);
                break;

            case AdminMenuItem.InspectionStatus:
                sut.OpenInspectionStatusCommand
                    .Execute(null);
                break;

            case AdminMenuItem.AbnormalList:
                sut.OpenAbnormalListCommand
                    .Execute(null);
                break;

            case AdminMenuItem.NotStartedList:
                sut.OpenNotStartedListCommand
                    .Execute(null);
                break;

            case AdminMenuItem.ApprovalPending:
                sut.OpenApprovalPendingCommand
                    .Execute(null);
                break;

            case AdminMenuItem.EquipmentManagement:
                sut.OpenEquipmentManagementCommand
                    .Execute(null);
                break;

            case AdminMenuItem.ScheduleCalendar:
                sut.OpenScheduleCalendarCommand
                    .Execute(null);
                break;

            case AdminMenuItem
                .InspectionTemplateManagement:
                sut.OpenInspectionTemplateManagementCommand
                    .Execute(null);
                break;

            case AdminMenuItem.OperatorManagement:
                sut.OpenOperatorManagementCommand
                    .Execute(null);
                break;

            case AdminMenuItem.AuditLog:
                sut.OpenAuditLogCommand
                    .Execute(null);
                break;

            case AdminMenuItem.BackupRestore:
                sut.OpenBackupRestoreCommand
                    .Execute(null);
                break;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(menu),
                    menu,
                    null);
        }
    }


    private static void AssertMenuSelection(
        AdminShellViewModel sut,
        AdminMenuItem expected)
    {
        Assert.Equal(
            expected == AdminMenuItem.Dashboard,
            sut.IsDashboardSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.EquipmentManagement,
            sut.IsEquipmentManagementSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem
                .InspectionTemplateManagement,
            sut.IsInspectionTemplateManagementSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.ScheduleCalendar,
            sut.IsScheduleCalendarSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.InspectionStatus,
            sut.IsInspectionStatusSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.AbnormalList,
            sut.IsAbnormalListSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.OperatorManagement,
            sut.IsOperatorManagementSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.NotStartedList,
            sut.IsNotStartedListSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.ApprovalPending,
            sut.IsApprovalPendingSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.AuditLog,
            sut.IsAuditLogSelected);

        Assert.Equal(
            expected ==
            AdminMenuItem.BackupRestore,
            sut.IsBackupRestoreSelected);
    }


    // ============================================
    // Test Context
    // ============================================

    private sealed class TestContext
    {
        public TestContext(
            Guid? operatorId = null,
            string displayName = "管理者")
        {
            OperatorId =
                operatorId ??
                Guid.NewGuid();

            Dashboard =
                new TestViewModel();

            EquipmentManagement =
                new TestViewModel();

            ScheduleCalendar =
                new TestViewModel();

            InspectionStatus =
                new TestViewModel();

            AbnormalList =
                new TestViewModel();

            NotStarted =
                new TestViewModel();

            AuditLog =
                new TestViewModel();

            ApprovalPending =
                new TestViewModel();

            BackupRestore =
                new TestViewModel();

            InspectionTemplateManagement =
                new TestViewModel();

            OperatorManagement =
                new TestViewModel();


            Sut =
                new AdminShellViewModel(
                    OperatorId,
                    displayName,

                    Dashboard,
                    EquipmentManagement,
                    ScheduleCalendar,
                    InspectionStatus,
                    AbnormalList,
                    NotStarted,
                    AuditLog,
                    ApprovalPending,
                    BackupRestore,
                    InspectionTemplateManagement,
                    OperatorManagement,

                    refreshDashboard:
                        () =>
                            RefreshDashboardCallCount++,

                    reloadApprovalPending:
                        () =>
                            ReloadApprovalPendingCallCount++,

                    createInspectionDetailViewModel:
                        (
                            scheduleId,
                            backRequested) =>
                        {
                            LastInspectionDetailScheduleId =
                                scheduleId;

                            LastInspectionBackRequested =
                                backRequested;

                            LastInspectionDetail =
                                new TestViewModel();

                            return LastInspectionDetail;
                        },

                    createApprovalPendingDetailViewModel:
                        (
                            scheduleId,
                            currentOperatorId,
                            backRequested) =>
                        {
                            LastApprovalDetailScheduleId =
                                scheduleId;

                            LastApprovalDetailOperatorId =
                                currentOperatorId;

                            LastApprovalBackRequested =
                                backRequested;

                            LastApprovalDetail =
                                new TestViewModel();

                            return LastApprovalDetail;
                        },

                    logoutRequested:
                        () =>
                            LogoutCallCount++);
        }


        public Guid OperatorId
        {
            get;
        }


        public AdminShellViewModel Sut
        {
            get;
        }


        // ========================================
        // Screen ViewModels
        // ========================================

        public TestViewModel Dashboard
        {
            get;
        }

        public TestViewModel EquipmentManagement
        {
            get;
        }

        public TestViewModel ScheduleCalendar
        {
            get;
        }

        public TestViewModel InspectionStatus
        {
            get;
        }

        public TestViewModel AbnormalList
        {
            get;
        }

        public TestViewModel NotStarted
        {
            get;
        }

        public TestViewModel AuditLog
        {
            get;
        }

        public TestViewModel ApprovalPending
        {
            get;
        }

        public TestViewModel BackupRestore
        {
            get;
        }

        public TestViewModel
            InspectionTemplateManagement
        {
            get;
        }

        public TestViewModel OperatorManagement
        {
            get;
        }


        // ========================================
        // Calls
        // ========================================

        public int RefreshDashboardCallCount
        {
            get;
            private set;
        }

        public int ReloadApprovalPendingCallCount
        {
            get;
            private set;
        }

        public int LogoutCallCount
        {
            get;
            private set;
        }


        // ========================================
        // Inspection Detail Factory
        // ========================================

        public Guid?
            LastInspectionDetailScheduleId
        {
            get;
            private set;
        }

        public TestViewModel?
            LastInspectionDetail
        {
            get;
            private set;
        }

        public Action?
            LastInspectionBackRequested
        {
            get;
            private set;
        }


        // ========================================
        // Approval Detail Factory
        // ========================================

        public Guid?
            LastApprovalDetailScheduleId
        {
            get;
            private set;
        }

        public Guid?
            LastApprovalDetailOperatorId
        {
            get;
            private set;
        }

        public TestViewModel?
            LastApprovalDetail
        {
            get;
            private set;
        }

        public Action?
            LastApprovalBackRequested
        {
            get;
            private set;
        }


        // ========================================
        // Get Screen
        // ========================================

        public ViewModelBase GetViewModel(
            AdminMenuItem menu)
        {
            return menu switch
            {
                AdminMenuItem.Dashboard =>
                    Dashboard,

                AdminMenuItem.InspectionStatus =>
                    InspectionStatus,

                AdminMenuItem.AbnormalList =>
                    AbnormalList,

                AdminMenuItem.NotStartedList =>
                    NotStarted,

                AdminMenuItem.ApprovalPending =>
                    ApprovalPending,

                AdminMenuItem.EquipmentManagement =>
                    EquipmentManagement,

                AdminMenuItem.ScheduleCalendar =>
                    ScheduleCalendar,

                AdminMenuItem
                    .InspectionTemplateManagement =>
                        InspectionTemplateManagement,

                AdminMenuItem.OperatorManagement =>
                    OperatorManagement,

                AdminMenuItem.AuditLog =>
                    AuditLog,

                AdminMenuItem.BackupRestore =>
                    BackupRestore,

                _ =>
                    throw new ArgumentOutOfRangeException(
                        nameof(menu),
                        menu,
                        null)
            };
        }
    }


    private sealed class TestViewModel
        : ViewModelBase
    {
    }
}
