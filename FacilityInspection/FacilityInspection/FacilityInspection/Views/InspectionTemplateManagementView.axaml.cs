using Avalonia.Controls;

namespace FacilityInspection.Views;

public partial class InspectionTemplateManagementView : UserControl
{
    /// <summary>
    /// 一覧と詳細を上下配置へ切り替える境界幅。
    /// </summary>
    private const double CompactBreakpoint = 760;

    /// <summary>
    /// 現在コンパクト表示かどうか。
    /// 同じレイアウトを何度も設定しないために保持する。
    /// </summary>
    private bool? _isCompactLayout;

    public InspectionTemplateManagementView()
    {
        InitializeComponent();

        SizeChanged += OnSizeChanged;
    }

    /// <summary>
    /// 画面サイズが変更されたときにレイアウトを切り替える。
    /// </summary>
    private void OnSizeChanged(
        object? sender,
        SizeChangedEventArgs e)
    {
        ApplyResponsiveLayout(e.NewSize.Width);
    }

    /// <summary>
    /// 表示領域の幅に応じて一覧と詳細の配置を変更する。
    /// </summary>
    private void ApplyResponsiveLayout(double width)
    {
        var useCompactLayout = width <= CompactBreakpoint;

        // 現在と同じレイアウトなら再設定しない
        if (_isCompactLayout == useCompactLayout)
        {
            return;
        }

        _isCompactLayout = useCompactLayout;

        if (useCompactLayout)
        {
            ApplyCompactLayout();
            return;
        }

        ApplyWideLayout();
    }

    /// <summary>
    /// 狭い画面用：一覧と詳細を上下に配置する。
    /// </summary>
    private void ApplyCompactLayout()
    {
        TemplateLayout.ColumnDefinitions =
            new ColumnDefinitions("*");

        TemplateLayout.RowDefinitions =
            new RowDefinitions("280,*");

        TemplateLayout.ColumnSpacing = 0;
        TemplateLayout.RowSpacing = 12;

        Grid.SetRow(TemplateListCard, 0);
        Grid.SetColumn(TemplateListCard, 0);

        Grid.SetRow(TemplateDetailCard, 1);
        Grid.SetColumn(TemplateDetailCard, 0);
    }

    /// <summary>
    /// 広い画面用：一覧と詳細を左右に配置する。
    /// </summary>
    private void ApplyWideLayout()
    {
        TemplateLayout.ColumnDefinitions =
            new ColumnDefinitions("300,*");

        TemplateLayout.RowDefinitions =
            new RowDefinitions("*");

        TemplateLayout.ColumnSpacing = 18;
        TemplateLayout.RowSpacing = 0;

        Grid.SetRow(TemplateListCard, 0);
        Grid.SetColumn(TemplateListCard, 0);

        Grid.SetRow(TemplateDetailCard, 0);
        Grid.SetColumn(TemplateDetailCard, 1);
    }
}