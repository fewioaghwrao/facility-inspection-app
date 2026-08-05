using CommunityToolkit.Mvvm.Input;
using System;

namespace FacilityInspection.ViewModels;

public sealed class MemberDashboardViewModel : ViewModelBase
{
    public MemberDashboardViewModel(
        Action openScheduleCalendar)
    {
        ArgumentNullException.ThrowIfNull(
            openScheduleCalendar);

        OpenScheduleCalendarCommand =
            new RelayCommand(
                openScheduleCalendar);
    }

    public string Title =>
        "点検担当者ダッシュボード";

    public string Description =>
        "本日の点検予定と点検状況を確認します。";

    public int TodayScheduleCount => 0;

    public int InProgressCount => 0;

    public int CompletedCount => 0;

    public int AbnormalityCount => 0;

    public string InformationMessage =>
        "点検予定・チェックリスト機能は、今後の実装で接続します。";

    public IRelayCommand OpenScheduleCalendarCommand { get; }
}