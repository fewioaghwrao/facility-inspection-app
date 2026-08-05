namespace FacilityInspection.ViewModels;

/// <summary>
/// 点検予定カレンダー画面。
/// 現時点では仮実装。
/// </summary>
public sealed class ScheduleCalendarViewModel : ViewModelBase
{
    public string Title =>
        "予定カレンダー";

    public string Description =>
        "設備の点検予定をカレンダー形式で確認します。";

    public string InformationMessage =>
        "予定カレンダー機能は今後実装予定です。";
}