using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public partial class MemberShellViewModel : ViewModelBase
{
    private readonly Action _logout;

    private readonly ScheduleRepository
        _scheduleRepository;

    private MemberDashboardViewModel?
        _dashboardViewModel;

    private EquipmentManagementViewModel?
        _equipmentManagementViewModel;

    private ScheduleCalendarViewModel?
        _scheduleCalendarViewModel;

    [ObservableProperty]
    private ViewModelBase currentContent;

    [ObservableProperty]
    private bool isLogoutDialogOpen;

    [ObservableProperty]
    private MemberMenuItem selectedMenuItem;

    public MemberShellViewModel(
        string operatorName,
        ScheduleRepository scheduleRepository,
        Action logout)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            operatorName);

        ArgumentNullException.ThrowIfNull(
            scheduleRepository);

        ArgumentNullException.ThrowIfNull(
            logout);

        OperatorName = operatorName;

        _scheduleRepository =
            scheduleRepository;

        _logout = logout;

        currentContent =
            GetDashboardViewModel();

        selectedMenuItem =
            MemberMenuItem.Dashboard;
    }

    public string OperatorName { get; }

    [RelayCommand]
    private void OpenDashboard()
    {
        CurrentContent =
            GetDashboardViewModel();

        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }

    [RelayCommand]
    private void OpenEquipmentManagement()
    {
        CurrentContent =
            GetEquipmentManagementViewModel();

        SelectedMenuItem =
            MemberMenuItem.EquipmentManagement;
    }

    [RelayCommand]
    private void OpenScheduleCalendar()
    {
        CurrentContent =
            GetScheduleCalendarViewModel();

        SelectedMenuItem =
            MemberMenuItem.ScheduleCalendar;
    }

    [RelayCommand]
    private void OpenLogoutDialog()
    {
        IsLogoutDialogOpen = true;
    }

    [RelayCommand]
    private void CancelLogout()
    {
        IsLogoutDialogOpen = false;
    }

    [RelayCommand]
    private void ConfirmLogout()
    {
        IsLogoutDialogOpen = false;

        _logout();
    }

    private MemberDashboardViewModel
        GetDashboardViewModel()
    {
        return _dashboardViewModel ??=
            new MemberDashboardViewModel(
                OpenScheduleCalendar);
    }

    private EquipmentManagementViewModel
        GetEquipmentManagementViewModel()
    {
        return _equipmentManagementViewModel ??=
            new EquipmentManagementViewModel();
    }

    private ScheduleCalendarViewModel
        GetScheduleCalendarViewModel()
    {
        return _scheduleCalendarViewModel ??=
            new ScheduleCalendarViewModel(
                _scheduleRepository);
    }
}
