using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class AdminShellViewModel : ViewModelBase
{
    private readonly Action _logoutRequested;

    private readonly AdminDashboardViewModel _dashboardViewModel;
    private readonly EquipmentManagementViewModel _equipmentManagementViewModel;
    private readonly ScheduleCalendarViewModel _scheduleCalendarViewModel;
    private readonly InspectionTemplateManagementViewModel
        _inspectionTemplateManagementViewModel;

    public AdminShellViewModel(
        string displayName,
        AdminDashboardViewModel dashboardViewModel,
        EquipmentManagementViewModel equipmentManagementViewModel,
        ScheduleCalendarViewModel scheduleCalendarViewModel,
        InspectionTemplateManagementViewModel inspectionTemplateManagementViewModel,
        Action logoutRequested)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(dashboardViewModel);
        ArgumentNullException.ThrowIfNull(equipmentManagementViewModel);
        ArgumentNullException.ThrowIfNull(scheduleCalendarViewModel);
        ArgumentNullException.ThrowIfNull(inspectionTemplateManagementViewModel);
        ArgumentNullException.ThrowIfNull(logoutRequested);

        DisplayName = displayName;

        _dashboardViewModel = dashboardViewModel;
        _equipmentManagementViewModel = equipmentManagementViewModel;
        _scheduleCalendarViewModel = scheduleCalendarViewModel;
        _inspectionTemplateManagementViewModel =
            inspectionTemplateManagementViewModel;
        _logoutRequested = logoutRequested;

        CurrentContent = _dashboardViewModel;
        SelectedMenu = AdminMenuItem.Dashboard;
    }

    public string DisplayName { get; }

    [ObservableProperty]
    private ViewModelBase currentContent = null!;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDashboardSelected))]
    [NotifyPropertyChangedFor(nameof(IsEquipmentManagementSelected))]
    [NotifyPropertyChangedFor(nameof(IsInspectionTemplateManagementSelected))]
    [NotifyPropertyChangedFor(nameof(IsScheduleCalendarSelected))]
    private AdminMenuItem selectedMenu;

    [ObservableProperty]
    private bool isLogoutDialogOpen;

    public bool IsDashboardSelected =>
        SelectedMenu == AdminMenuItem.Dashboard;

    public bool IsEquipmentManagementSelected =>
        SelectedMenu == AdminMenuItem.EquipmentManagement;

    public bool IsInspectionTemplateManagementSelected =>
        SelectedMenu == AdminMenuItem.InspectionTemplateManagement;

    public bool IsScheduleCalendarSelected =>
        SelectedMenu == AdminMenuItem.ScheduleCalendar;

    [RelayCommand]
    private void OpenDashboard()
    {
        CurrentContent = _dashboardViewModel;
        SelectedMenu = AdminMenuItem.Dashboard;
    }

    [RelayCommand]
    private void OpenEquipmentManagement()
    {
        CurrentContent = _equipmentManagementViewModel;
        SelectedMenu = AdminMenuItem.EquipmentManagement;
    }

    [RelayCommand]
    private void OpenInspectionTemplateManagement()
    {
        CurrentContent = _inspectionTemplateManagementViewModel;
        SelectedMenu = AdminMenuItem.InspectionTemplateManagement;
    }

    [RelayCommand]
    private void OpenScheduleCalendar()
    {
        CurrentContent = _scheduleCalendarViewModel;
        SelectedMenu = AdminMenuItem.ScheduleCalendar;
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
        _logoutRequested();
    }
}