using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class AdminDashboardViewModel : ViewModelBase
{
    private readonly Action _logoutRequested;

    public AdminDashboardViewModel(
        string displayName,
        Action logoutRequested)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentNullException.ThrowIfNull(logoutRequested);

        DisplayName = displayName;
        _logoutRequested = logoutRequested;
    }

    public string DisplayName { get; }

    public string WelcomeMessage =>
        $"{DisplayName}さん、お疲れさまです。";

    public string CurrentDateText =>
        DateTime.Now.ToString("yyyy年M月d日");

    // 現在はモック値
    public int EquipmentCount => 30;

    public int TodayInspectionCount => 8;

    public int CompletedInspectionCount => 5;

    public int AlertCount => 2;

    public int OperatorCount => 5;

    [ObservableProperty]
    private bool isLogoutDialogOpen;

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