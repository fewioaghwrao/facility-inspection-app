using CommunityToolkit.Mvvm.ComponentModel;
using System;

namespace FacilityInspection.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase currentPage;

    public MainViewModel()
    {
        // 移行途中なので、現在は設備台帳を初期表示する。
        // ログイン画面作成後はLoginViewModelへ変更する。
        currentPage =
            new EquipmentManagementViewModel();
    }

    public void NavigateTo(
        ViewModelBase destination)
    {
        ArgumentNullException.ThrowIfNull(destination);

        CurrentPage = destination;
    }
}