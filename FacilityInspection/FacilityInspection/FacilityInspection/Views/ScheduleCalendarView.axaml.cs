using Avalonia.Controls;
using Avalonia.Controls.Primitives;

namespace FacilityInspection.Views;

public partial class ScheduleCalendarView : UserControl
{
    private const double CompactBreakpoint = 900;

    private bool? _isCompactLayout;

    public ScheduleCalendarView()
    {
        InitializeComponent();

        SizeChanged += OnSizeChanged;
    }

    private void OnSizeChanged(
        object? sender,
        SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(
            e.NewSize.Width);
    }

    private void ApplyResponsiveLayout(
        double width)
    {
        var useCompactLayout =
            width <= CompactBreakpoint;

        if (_isCompactLayout ==
            useCompactLayout)
        {
            return;
        }

        _isCompactLayout =
            useCompactLayout;

        if (useCompactLayout)
        {
            ApplyCompactLayout();
            return;
        }

        ApplyWideLayout();
    }

    /// <summary>
    /// 狭い画面では、カレンダーと予定一覧を上下に並べる。
    /// 画面全体をスクロール対象にする。
    /// </summary>
    private void ApplyCompactLayout()
    {
        PageScrollViewer
            .VerticalScrollBarVisibility =
                ScrollBarVisibility.Auto;

        // 外側のPageScrollViewerにスクロールを任せる
        DayScheduleScrollViewer
            .VerticalScrollBarVisibility =
                ScrollBarVisibility.Disabled;

        ScheduleLayout.ColumnDefinitions =
            new ColumnDefinitions("*");

        // ScrollViewer内では * ではなくAutoを使う
        ScheduleLayout.RowDefinitions =
            new RowDefinitions("Auto,Auto");

        ScheduleLayout.ColumnSpacing = 0;
        ScheduleLayout.RowSpacing = 14;

        Grid.SetRow(
            CalendarCard,
            0);

        Grid.SetColumn(
            CalendarCard,
            0);

        Grid.SetRow(
            DayScheduleCard,
            1);

        Grid.SetColumn(
            DayScheduleCard,
            0);
    }

    /// <summary>
    /// 広い画面では、カレンダーと予定一覧を左右に並べる。
    /// 予定一覧カードの内部だけをスクロールさせる。
    /// </summary>
    private void ApplyWideLayout()
    {
        PageScrollViewer
            .VerticalScrollBarVisibility =
                ScrollBarVisibility.Disabled;

        DayScheduleScrollViewer
            .VerticalScrollBarVisibility =
                ScrollBarVisibility.Auto;

        ScheduleLayout.ColumnDefinitions =
            new ColumnDefinitions(
                "1.2*,0.8*");

        ScheduleLayout.RowDefinitions =
            new RowDefinitions("*");

        ScheduleLayout.ColumnSpacing = 18;
        ScheduleLayout.RowSpacing = 0;

        Grid.SetRow(
            CalendarCard,
            0);

        Grid.SetColumn(
            CalendarCard,
            0);

        Grid.SetRow(
            DayScheduleCard,
            0);

        Grid.SetColumn(
            DayScheduleCard,
            1);
    }
}