using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class AbnormalListViewModel
    : ViewModelBase
{
    private const int PageSize = 5;

    private readonly InspectionRepository
        _inspectionRepository;

    private IReadOnlyList<AbnormalResultListData>
        _allItems = [];

    private List<AbnormalResultListData>
        _filteredItems = [];

    public AbnormalListViewModel(
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        _inspectionRepository =
            inspectionRepository;

        _ = LoadAsync();
    }

    public Action<Guid>? DetailRequested
    {
        get;
        set;
    }

    public string Title =>
        "異常一覧";

    public string Description =>
        "点検結果で異常と判定された項目を一覧表示します。";

    public ObservableCollection<
        AbnormalListItemViewModel>
        Items
    { get; } = [];

    [ObservableProperty]
    private string searchText =
        string.Empty;

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageText))]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    private int currentPage = 1;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    public bool IsEmpty =>
        !IsLoading &&
        _filteredItems.Count == 0;

    public string CountText =>
        $"{_filteredItems.Count}件";

    public int TotalPages =>
        Math.Max(
            1,
            (int)Math.Ceiling(
                _filteredItems.Count /
                (double)PageSize));

    public string PageText =>
        $"{CurrentPage} / {TotalPages}";

    public bool HasPreviousPage =>
        CurrentPage > 1;

    public bool HasNextPage =>
        CurrentPage < TotalPages;

    partial void OnSearchTextChanged(
        string value)
    {
        ApplyFilter();
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            _allItems =
                await _inspectionRepository
                    .GetAbnormalResultsAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            ErrorMessage =
                "異常一覧を読み込めませんでした。" +
                Environment.NewLine +
                ex.Message;
        }
        finally
        {
            IsLoading = false;

            OnPropertyChanged(
                nameof(IsEmpty));
        }
    }

    private void ApplyFilter()
    {
        IEnumerable<AbnormalResultListData>
            query = _allItems;

        var keyword =
            SearchText.Trim();

        if (!string.IsNullOrWhiteSpace(
                keyword))
        {
            query =
                query.Where(x =>
                    x.FactorySiteName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    x.LocationName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    x.EquipmentCode.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    x.EquipmentName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    x.ItemName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    x.OperatorName.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase) ||

                    (x.Comment?.Contains(
                        keyword,
                        StringComparison.OrdinalIgnoreCase)
                        ?? false));
        }

        _filteredItems =
            query.ToList();

        CurrentPage = 1;

        ApplyPage();
    }

    private void ApplyPage()
    {
        Items.Clear();

        foreach (var source in
                 _filteredItems
                     .Skip(
                         (CurrentPage - 1) *
                         PageSize)
                     .Take(PageSize))
        {
            Items.Add(
                new AbnormalListItemViewModel(
                    source,
                    OpenDetail));
        }

        OnPropertyChanged(
            nameof(IsEmpty));

        OnPropertyChanged(
            nameof(CountText));

        OnPropertyChanged(
            nameof(TotalPages));

        OnPropertyChanged(
            nameof(PageText));

        OnPropertyChanged(
            nameof(HasPreviousPage));

        OnPropertyChanged(
            nameof(HasNextPage));
    }

    private void OpenDetail(
        Guid scheduleId)
    {
        DetailRequested?.Invoke(
            scheduleId);
    }

    [RelayCommand]
    private void PreviousPage()
    {
        if (!HasPreviousPage)
        {
            return;
        }

        CurrentPage--;

        ApplyPage();
    }

    [RelayCommand]
    private void NextPage()
    {
        if (!HasNextPage)
        {
            return;
        }

        CurrentPage++;

        ApplyPage();
    }
}