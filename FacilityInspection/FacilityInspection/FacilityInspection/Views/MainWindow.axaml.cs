using Avalonia.Controls;
using FacilityInspection.ViewModels;

namespace FacilityInspection.Views;

public partial class MainWindow : Window
{
    /// <summary>
    /// 未保存変更の確認で「移動する」が選ばれ、閉じてよい状態か。
    /// </summary>
    private bool _closeConfirmed;

    public MainWindow()
    {
        InitializeComponent();
    }

    protected override void OnClosing(
        WindowClosingEventArgs e)
    {
        if (!_closeConfirmed &&
            DataContext is MainViewModel
            {
                ActiveNavigationGuard:
                {
                    HasUnsavedChanges: true
                } guard
            })
        {
            /*
             * 点検入力中に保存していない変更がある場合は、
             * いったん閉じるのを取り消し、点検画面の確認ダイアログを表示する。
             */
            e.Cancel = true;

            guard.RequestLeave(
                () =>
                {
                    _closeConfirmed = true;
                    Close();
                });

            return;
        }

        base.OnClosing(e);
    }
}
