using System;

namespace FacilityInspection.ViewModels;

public sealed class AdminDashboardViewModel : ViewModelBase
{
    public AdminDashboardViewModel(string displayName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);

        DisplayName = displayName;
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
}