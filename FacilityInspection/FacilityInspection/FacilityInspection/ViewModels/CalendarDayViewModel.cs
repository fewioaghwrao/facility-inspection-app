using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;

namespace FacilityInspection.ViewModels;

public sealed partial class CalendarDayViewModel
    : ObservableObject
{
    private readonly Action<CalendarDayViewModel>
        _selected;

    public CalendarDayViewModel(
        DateOnly date,
        bool isCurrentMonth,
        int totalScheduleCount,
        int overdueCount,
        int completedCount,
        Action<CalendarDayViewModel> selected)
    {
        ArgumentNullException.ThrowIfNull(selected);

        Date = date;
        IsCurrentMonth = isCurrentMonth;
        TotalScheduleCount = totalScheduleCount;
        OverdueCount = overdueCount;
        CompletedCount = completedCount;
        IsToday =
            date ==
            DateOnly.FromDateTime(DateTime.Today);

        _selected = selected;
    }

    public DateOnly Date { get; }

    public int DayNumber => Date.Day;

    public bool IsCurrentMonth { get; }

    public bool IsToday { get; }

    public int TotalScheduleCount { get; }

    public int OverdueCount { get; }

    public int CompletedCount { get; }

    public bool HasSchedules =>
        TotalScheduleCount > 0;

    public string ScheduleCountText =>
        HasSchedules
            ? $"{TotalScheduleCount}件"
            : string.Empty;

    public string SummaryText
    {
        get
        {
            if (OverdueCount > 0)
            {
                return $"期限超過 {OverdueCount}";
            }

            if (CompletedCount > 0)
            {
                return $"完了 {CompletedCount}";
            }

            return HasSchedules
                ? "点検予定"
                : string.Empty;
        }
    }

    public string BackgroundColor =>
        IsSelected
            ? "#DBEAFE"
            : IsCurrentMonth
                ? "#FFFFFF"
                : "#F8FAFC";

    public string BorderColor =>
        IsSelected
            ? "#2563EB"
            : IsToday
                ? "#60A5FA"
                : "#E2E8F0";

    public string DayForeground =>
        IsCurrentMonth
            ? "#0F172A"
            : "#94A3B8";

    public string SummaryForeground =>
        OverdueCount > 0
            ? "#DC2626"
            : CompletedCount > 0
                ? "#15803D"
                : "#2563EB";

    [ObservableProperty]
    private bool isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundColor));
        OnPropertyChanged(nameof(BorderColor));
    }

    [RelayCommand]
    private void Select()
    {
        _selected(this);
    }
}
