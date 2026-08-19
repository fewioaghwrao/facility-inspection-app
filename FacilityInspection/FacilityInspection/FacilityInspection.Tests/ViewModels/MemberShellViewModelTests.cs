using FacilityInspection.ViewModels;
using System;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class MemberShellViewModelTests
{
    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor Validation
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberShellViewModel(
                        Guid.Empty,
                        "点検担当者A",
                        () =>
                        {
                        },
                        (_, _) =>
                            new StubViewModel(
                                "Dashboard"),
                        _ =>
                            new StubViewModel(
                                "InspectionList"),
                        () =>
                            new StubViewModel(
                                "Calendar"),
                        (_, _, _) =>
                            new StubViewModel(
                                "Entry")));


        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);

        Assert.Contains(
            "点検担当者IDを指定してください。",
            exception.Message);
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankOperatorName_ThrowsArgumentException(
        string operatorName)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberShellViewModel(
                        OperatorId,
                        operatorName,
                        () =>
                        {
                        },
                        (_, _) =>
                            new StubViewModel(
                                "Dashboard"),
                        _ =>
                            new StubViewModel(
                                "InspectionList"),
                        () =>
                            new StubViewModel(
                                "Calendar"),
                        (_, _, _) =>
                            new StubViewModel(
                                "Entry")));


        // Assert
        Assert.Equal(
            "operatorName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullLogout_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberShellViewModel(
                        OperatorId,
                        "点検担当者A",
                        null!,
                        (_, _) =>
                            new StubViewModel(
                                "Dashboard"),
                        _ =>
                            new StubViewModel(
                                "InspectionList"),
                        () =>
                            new StubViewModel(
                                "Calendar"),
                        (_, _, _) =>
                            new StubViewModel(
                                "Entry")));


        // Assert
        Assert.Equal(
            "logout",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullDashboardFactory_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberShellViewModel(
                        OperatorId,
                        "点検担当者A",
                        () =>
                        {
                        },
                        null!,
                        _ =>
                            new StubViewModel(
                                "InspectionList"),
                        () =>
                            new StubViewModel(
                                "Calendar"),
                        (_, _, _) =>
                            new StubViewModel(
                                "Entry")));


        // Assert
        Assert.Equal(
            "createDashboardViewModel",
            exception.ParamName);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_InitializesDashboardAndMenuState()
    {
        // Arrange & Act
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Assert
        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);

        Assert.Equal(
            1,
            recorder.DashboardFactoryCallCount);

        Assert.Equal(
            OperatorId,
            recorder.DashboardOperatorId);

        Assert.NotNull(
            recorder.DashboardOpenInspection);

        Assert.Same(
            recorder.LatestDashboard,
            sut.CurrentContent);


        Assert.Equal(
            MemberMenuItem.Dashboard,
            sut.SelectedMenuItem);

        Assert.True(
            sut.IsDashboardSelected);

        Assert.False(
            sut.IsInspectionListSelected);

        Assert.False(
            sut.IsLogoutDialogOpen);


        Assert.Equal(
            0,
            recorder.InspectionListFactoryCallCount);

        Assert.Equal(
            0,
            recorder.ScheduleCalendarFactoryCallCount);

        Assert.Equal(
            0,
            recorder.InspectionEntryFactoryCallCount);
    }


    // ============================================
    // Dashboard
    // ============================================

    [Fact]
    public void OpenDashboardCommand_RecreatesDashboard()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        var firstDashboard =
            sut.CurrentContent;


        Assert.Equal(
            1,
            recorder.DashboardFactoryCallCount);


        // Act
        sut.OpenDashboardCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            recorder.DashboardFactoryCallCount);

        Assert.NotSame(
            firstDashboard,
            sut.CurrentContent);

        Assert.Same(
            recorder.LatestDashboard,
            sut.CurrentContent);

        Assert.Equal(
            MemberMenuItem.Dashboard,
            sut.SelectedMenuItem);

        Assert.True(
            sut.IsDashboardSelected);

        Assert.False(
            sut.IsInspectionListSelected);
    }


    // ============================================
    // Inspection List
    // ============================================

    [Fact]
    public void OpenInspectionListCommand_NavigatesToInspectionList()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Act
        sut.OpenInspectionListCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            1,
            recorder.InspectionListFactoryCallCount);

        Assert.Equal(
            OperatorId,
            recorder.InspectionListOperatorId);

        Assert.Same(
            recorder.LatestInspectionList,
            sut.CurrentContent);

        Assert.Equal(
            MemberMenuItem.InspectionList,
            sut.SelectedMenuItem);

        Assert.False(
            sut.IsDashboardSelected);

        Assert.True(
            sut.IsInspectionListSelected);
    }


    [Fact]
    public void OpenInspectionListCommand_RecreatesInspectionListEveryTime()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Act - first
        sut.OpenInspectionListCommand
            .Execute(null);


        var firstList =
            sut.CurrentContent;


        // Act - second
        sut.OpenInspectionListCommand
            .Execute(null);


        var secondList =
            sut.CurrentContent;


        // Assert
        Assert.Equal(
            2,
            recorder.InspectionListFactoryCallCount);

        Assert.NotSame(
            firstList,
            secondList);

        Assert.Same(
            recorder.LatestInspectionList,
            secondList);

        Assert.Equal(
            MemberMenuItem.InspectionList,
            sut.SelectedMenuItem);
    }


    // ============================================
    // Schedule Calendar
    // ============================================

    [Fact]
    public void OpenScheduleCalendarCommand_NavigatesToScheduleCalendar()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Act
        sut.OpenScheduleCalendarCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            1,
            recorder.ScheduleCalendarFactoryCallCount);

        Assert.Same(
            recorder.LatestScheduleCalendar,
            sut.CurrentContent);

        Assert.Equal(
            MemberMenuItem.ScheduleCalendar,
            sut.SelectedMenuItem);

        Assert.False(
            sut.IsDashboardSelected);

        Assert.False(
            sut.IsInspectionListSelected);
    }


    [Fact]
    public void OpenScheduleCalendarCommand_ReusesCachedScheduleCalendar()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Act - first
        sut.OpenScheduleCalendarCommand
            .Execute(null);


        var firstCalendar =
            sut.CurrentContent;


        // Navigate away
        sut.OpenInspectionListCommand
            .Execute(null);


        // Act - second
        sut.OpenScheduleCalendarCommand
            .Execute(null);


        var secondCalendar =
            sut.CurrentContent;


        // Assert
        Assert.Equal(
            1,
            recorder.ScheduleCalendarFactoryCallCount);

        Assert.Same(
            firstCalendar,
            secondCalendar);

        Assert.Same(
            recorder.LatestScheduleCalendar,
            secondCalendar);
    }


    // ============================================
    // Open Inspection
    // ============================================

    [Fact]
    public void DashboardOpenInspection_NavigatesToInspectionEntryAndPassesIds()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        var scheduleId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


        Assert.NotNull(
            recorder.DashboardOpenInspection);


        // Act
        recorder.DashboardOpenInspection!(
            scheduleId);


        // Assert
        Assert.Equal(
            1,
            recorder.InspectionEntryFactoryCallCount);

        Assert.Equal(
            scheduleId,
            recorder.EntryScheduleId);

        Assert.Equal(
            OperatorId,
            recorder.EntryOperatorId);

        Assert.NotNull(
            recorder.EntryBack);

        Assert.Same(
            recorder.LatestInspectionEntry,
            sut.CurrentContent);


        /*
         * 点検実施はDashboardから開始した扱いなので
         * 左メニューはDashboard選択状態。
         */
        Assert.Equal(
            MemberMenuItem.Dashboard,
            sut.SelectedMenuItem);

        Assert.True(
            sut.IsDashboardSelected);

        Assert.False(
            sut.IsInspectionListSelected);
    }


    // ============================================
    // Return From Inspection
    // ============================================

    [Fact]
    public void InspectionEntryBack_RecreatesDashboard()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        var firstDashboard =
            sut.CurrentContent;


        recorder.DashboardOpenInspection!(
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"));


        Assert.Equal(
            1,
            recorder.DashboardFactoryCallCount);

        Assert.Equal(
            1,
            recorder.InspectionEntryFactoryCallCount);

        Assert.NotNull(
            recorder.EntryBack);

        Assert.Same(
            recorder.LatestInspectionEntry,
            sut.CurrentContent);


        // Act
        recorder.EntryBack!();


        // Assert
        Assert.Equal(
            2,
            recorder.DashboardFactoryCallCount);

        Assert.NotSame(
            firstDashboard,
            sut.CurrentContent);

        Assert.Same(
            recorder.LatestDashboard,
            sut.CurrentContent);

        Assert.Equal(
            MemberMenuItem.Dashboard,
            sut.SelectedMenuItem);

        Assert.True(
            sut.IsDashboardSelected);
    }


    // ============================================
    // Logout Dialog
    // ============================================

    [Fact]
    public void OpenLogoutDialogCommand_OpensDialog()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        Assert.False(
            sut.IsLogoutDialogOpen);


        // Act
        sut.OpenLogoutDialogCommand
            .Execute(null);


        // Assert
        Assert.True(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            0,
            recorder.LogoutCallCount);
    }


    [Fact]
    public void CancelLogoutCommand_ClosesDialogWithoutLogout()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


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
            recorder.LogoutCallCount);
    }


    [Fact]
    public void ConfirmLogoutCommand_ClosesDialogAndInvokesLogout()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        sut.OpenLogoutDialogCommand
            .Execute(null);


        Assert.True(
            sut.IsLogoutDialogOpen);


        // Act
        sut.ConfirmLogoutCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsLogoutDialogOpen);

        Assert.Equal(
            1,
            recorder.LogoutCallCount);
    }


    // ============================================
    // Test ViewModel
    // ============================================

    private sealed class StubViewModel
        : ViewModelBase
    {
        public StubViewModel(
            string name)
        {
            Name =
                name;
        }


        public string Name
        {
            get;
        }
    }


    // ============================================
    // Factory Recorder
    // ============================================

    private sealed class FactoryRecorder
    {
        private int
            _dashboardNumber;

        private int
            _inspectionListNumber;

        private int
            _calendarNumber;

        private int
            _inspectionEntryNumber;


        // ----------------------------------------
        // Calls
        // ----------------------------------------

        public int DashboardFactoryCallCount
        {
            get;
            private set;
        }


        public int InspectionListFactoryCallCount
        {
            get;
            private set;
        }


        public int ScheduleCalendarFactoryCallCount
        {
            get;
            private set;
        }


        public int InspectionEntryFactoryCallCount
        {
            get;
            private set;
        }


        public int LogoutCallCount
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Captured Arguments
        // ----------------------------------------

        public Guid?
            DashboardOperatorId
        {
            get;
            private set;
        }


        public Guid?
            InspectionListOperatorId
        {
            get;
            private set;
        }


        public Guid?
            EntryScheduleId
        {
            get;
            private set;
        }


        public Guid?
            EntryOperatorId
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Captured Callbacks
        // ----------------------------------------

        public Action<Guid>?
            DashboardOpenInspection
        {
            get;
            private set;
        }


        public Action?
            EntryBack
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Latest ViewModels
        // ----------------------------------------

        public StubViewModel?
            LatestDashboard
        {
            get;
            private set;
        }


        public StubViewModel?
            LatestInspectionList
        {
            get;
            private set;
        }


        public StubViewModel?
            LatestScheduleCalendar
        {
            get;
            private set;
        }


        public StubViewModel?
            LatestInspectionEntry
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Create SUT
        // ----------------------------------------

        public MemberShellViewModel
            CreateViewModel()
        {
            return new MemberShellViewModel(
                operatorId:
                    OperatorId,

                operatorName:
                    "点検担当者A",

                logout:
                    () =>
                    {
                        LogoutCallCount++;
                    },

                createDashboardViewModel:
                    (
                        operatorId,
                        openInspection) =>
                    {
                        DashboardFactoryCallCount++;

                        DashboardOperatorId =
                            operatorId;

                        DashboardOpenInspection =
                            openInspection;


                        _dashboardNumber++;


                        LatestDashboard =
                            new StubViewModel(
                                $"Dashboard-{_dashboardNumber}");


                        return LatestDashboard;
                    },

                createInspectionListViewModel:
                    operatorId =>
                    {
                        InspectionListFactoryCallCount++;

                        InspectionListOperatorId =
                            operatorId;


                        _inspectionListNumber++;


                        LatestInspectionList =
                            new StubViewModel(
                                $"InspectionList-{_inspectionListNumber}");


                        return LatestInspectionList;
                    },

                createScheduleCalendarViewModel:
                    () =>
                    {
                        ScheduleCalendarFactoryCallCount++;


                        _calendarNumber++;


                        LatestScheduleCalendar =
                            new StubViewModel(
                                $"Calendar-{_calendarNumber}");


                        return LatestScheduleCalendar;
                    },

                createInspectionEntryViewModel:
                    (
                        scheduleId,
                        operatorId,
                        back) =>
                    {
                        InspectionEntryFactoryCallCount++;

                        EntryScheduleId =
                            scheduleId;

                        EntryOperatorId =
                            operatorId;

                        EntryBack =
                            back;


                        _inspectionEntryNumber++;


                        LatestInspectionEntry =
                            new StubViewModel(
                                $"InspectionEntry-{_inspectionEntryNumber}");


                        return LatestInspectionEntry;
                    });
        }
    }
}