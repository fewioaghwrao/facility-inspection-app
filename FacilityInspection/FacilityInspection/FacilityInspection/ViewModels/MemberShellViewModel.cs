using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public partial class MemberShellViewModel : ViewModelBase
{
    private readonly Action _logout;

    private readonly Guid
        _operatorId;

    private readonly ScheduleRepository
        _scheduleRepository;

    private readonly InspectionRepository
        _inspectionRepository;

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

    public string OperatorName { get; }

    [RelayCommand]
    private void OpenDashboard()
    {
        ShowDashboard(
            reload: true);
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
                _operatorId,
                _scheduleRepository,
                OpenInspection);
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

    private void OpenInspection(
        Guid scheduleId)
    {
        CurrentContent =
            new InspectionEntryViewModel(
                scheduleId,
                _operatorId,
                _inspectionRepository,
                ReturnFromInspection);

        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }

    private void ReturnFromInspection()
    {
        ShowDashboard(
            reload: true);
    }

    private void ShowDashboard(
        bool reload)
    {
        if (reload)
        {
            _dashboardViewModel = null;
        }

        CurrentContent =
            GetDashboardViewModel();

        SelectedMenuItem =
            MemberMenuItem.Dashboard;
    }
}
