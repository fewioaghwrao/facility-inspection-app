using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class NotStartedListViewModel
    : ViewModelBase
{
    private const int PageSize = 5;


    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<
        Task<IReadOnlyList<InspectionListData>>>
        _loadNotStartedAsync;


    // ============================================
    // Data
    // ============================================

    private IReadOnlyList<InspectionListData>
        _allItems = [];

    private List<InspectionListData>
        _filteredItems = [];


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public NotStartedListViewModel(
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);


        /*
         * Repository側にはoptional CancellationTokenがあるため、
         * lambdaでラップする。
         */
        _loadNotStartedAsync =
            () =>
                inspectionRepository
                    .GetNotStartedAsync();


        /*
         * 本番では従来どおり
         * コンストラクタ生成後に自動ロードする。
         */
        _ = LoadAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal NotStartedListViewModel(
        Func<
            Task<IReadOnlyList<InspectionListData>>>
            loadNotStartedAsync)
    {
        ArgumentNullException.ThrowIfNull(
            loadNotStartedAsync);


        _loadNotStartedAsync =
            loadNotStartedAsync;


        /*
         * テスト用では自動ロードしない。
         */
    }


    // ============================================
    // Navigation
    // ============================================

    public Action<Guid>? DetailRequested
    {
        get;
        set;
    }


    // ============================================
    // Header
    // ============================================

    public string Title =>
        "未実施一覧";


    public string Description =>
        "点検予定のうち、まだ点検が開始されていない項目を一覧表示します。";


    // ============================================
    // Items
    // ============================================

    public ObservableCollection<
        NotStartedListItemViewModel>
        Items
    {
        get;
    } = [];


    // ============================================
    // Search
    // ============================================

    [ObservableProperty]
    private string searchText =
        string.Empty;


    // ============================================
    // Loading
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEmpty))]
    private bool isLoading;


    // ============================================
    // Error
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    // ============================================
    // Empty
    // ============================================

    public bool IsEmpty =>
        !IsLoading &&
        _filteredItems.Count == 0;


    // ============================================
    // Count
    // ============================================

    public string CountText =>
        $"{_filteredItems.Count}件";


    // ============================================
    // Paging
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(PageText))]
    [NotifyPropertyChangedFor(
        nameof(HasPreviousPage))]
    [NotifyPropertyChangedFor(
        nameof(HasNextPage))]
    private int currentPage = 1;


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


    // ============================================
    // Search Changed
    // ============================================

    partial void OnSearchTextChanged(
        string value)
    {
        ApplyFilter();
    }


    // ============================================
    // Load
    // ============================================

    [RelayCommand]
    internal async Task LoadAsync()
    {
        /*
         * 多重読込防止。
         */
        if (IsLoading)
        {
            return;
        }


        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;


            _allItems =
                await _loadNotStartedAsync();


            ApplyFilter();
        }
        catch (Exception ex)
        {
            _allItems =
                [];

            _filteredItems =
                [];

            Items.Clear();


            /*
             * エラー時に
             * "2 / 1" 等にならないよう1ページ目へ戻す。
             */
            CurrentPage =
                1;


            ErrorMessage =
                "未実施一覧を読み込めませんでした。" +
                Environment.NewLine +
                ex.Message;


            NotifyListProperties();
        }
        finally
        {
            IsLoading =
                false;


            OnPropertyChanged(
                nameof(IsEmpty));
        }
    }


    // ============================================
    // Filter
    // ============================================

    private void ApplyFilter()
    {
        IEnumerable<InspectionListData>
            query =
                _allItems;


        var keyword =
            SearchText.Trim();


        if (!string.IsNullOrWhiteSpace(
                keyword))
        {
            query =
                query.Where(
                    x =>
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

                        x.TemplateName.Contains(
                            keyword,
                            StringComparison.OrdinalIgnoreCase) ||

                        x.OperatorName.Contains(
                            keyword,
                            StringComparison.OrdinalIgnoreCase));
        }


        _filteredItems =
            query.ToList();


        /*
         * 検索条件が変わったら
         * 必ず1ページ目へ戻す。
         */
        CurrentPage =
            1;


        ApplyPage();
    }


    // ============================================
    // Apply Page
    // ============================================

    private void ApplyPage()
    {
        Items.Clear();


        var skip =
            (CurrentPage - 1) *
            PageSize;


        foreach (var source in
                 _filteredItems
                     .Skip(skip)
                     .Take(PageSize))
        {
            Items.Add(
                new NotStartedListItemViewModel(
                    source,
                    OpenDetail));
        }


        NotifyListProperties();
    }


    // ============================================
    // Detail
    // ============================================

    private void OpenDetail(
        Guid scheduleId)
    {
        if (scheduleId == Guid.Empty)
        {
            return;
        }


        DetailRequested?.Invoke(
            scheduleId);
    }


    // ============================================
    // Previous Page
    // ============================================

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


    // ============================================
    // Next Page
    // ============================================

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


    // ============================================
    // Notify
    // ============================================

    private void NotifyListProperties()
    {
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

        OnPropertyChanged(
            nameof(IsEmpty));
    }
}