using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;


// ============================================
// 操作種別フィルター
// ============================================

public sealed record AuditActionFilterOption(
    string Label,
    AuditActionType? Value);


// ============================================
// 対象種別フィルター
// ============================================

public sealed record AuditEntityFilterOption(
    string Label,
    AuditEntityType? Value);


// ============================================
// 操作履歴 ViewModel
// ============================================

public sealed partial class AuditLogViewModel
    : ViewModelBase
{
    private const int PageSize = 10;

    private readonly AuditLogRepository
        _auditLogRepository;

    private IReadOnlyList<AuditLogListData>
        _allItems = [];

    private List<AuditLogListData>
        _filteredItems = [];


    // ============================================
    // Constructor
    // ============================================

    public AuditLogViewModel(
        AuditLogRepository auditLogRepository)
    {
        ArgumentNullException.ThrowIfNull(
            auditLogRepository);

        _auditLogRepository =
            auditLogRepository;


        // ----------------------------------------
        // 操作種別
        // ----------------------------------------

        ActionFilterOptions.Add(
            new AuditActionFilterOption(
                "すべて",
                null));

        foreach (var actionType in
                 Enum.GetValues<AuditActionType>())
        {
            ActionFilterOptions.Add(
                new AuditActionFilterOption(
                    AuditLogListItemViewModel
                        .GetActionTypeText(
                            actionType),
                    actionType));
        }

        SelectedActionFilter =
            ActionFilterOptions[0];


        // ----------------------------------------
        // 対象種別
        // ----------------------------------------

        EntityFilterOptions.Add(
            new AuditEntityFilterOption(
                "すべて",
                null));

        foreach (var entityType in
                 Enum.GetValues<AuditEntityType>())
        {
            EntityFilterOptions.Add(
                new AuditEntityFilterOption(
                    AuditLogListItemViewModel
                        .GetEntityTypeText(
                            entityType),
                    entityType));
        }

        SelectedEntityFilter =
            EntityFilterOptions[0];


        _ = LoadAsync();
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
        "操作履歴";


    public string Description =>
        "システム内で実行された主要な操作を時系列で確認できます。";


    // ============================================
    // Items
    // ============================================

    public ObservableCollection<
        AuditLogListItemViewModel>
        Items
    {
        get;
    } = [];


    // ============================================
    // Action Filter
    // ============================================

    public ObservableCollection<
        AuditActionFilterOption>
        ActionFilterOptions
    {
        get;
    } = [];


    [ObservableProperty]
    private AuditActionFilterOption?
        selectedActionFilter;


    partial void OnSelectedActionFilterChanged(
        AuditActionFilterOption? value)
    {
        ApplyFilter();
    }


    // ============================================
    // Entity Filter
    // ============================================

    public ObservableCollection<
        AuditEntityFilterOption>
        EntityFilterOptions
    {
        get;
    } = [];


    [ObservableProperty]
    private AuditEntityFilterOption?
        selectedEntityFilter;


    partial void OnSelectedEntityFilterChanged(
        AuditEntityFilterOption? value)
    {
        ApplyFilter();
    }


    // ============================================
    // Search
    // ============================================

    [ObservableProperty]
    private string searchText =
        string.Empty;


    partial void OnSearchTextChanged(
        string value)
    {
        ApplyFilter();
    }


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
    // Load
    // ============================================

    [RelayCommand]
    private async Task LoadAsync()
    {
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
                await _auditLogRepository
                    .GetAllAsync();

            ApplyFilter();
        }
        catch (Exception ex)
        {
            _allItems =
                [];

            _filteredItems =
                [];

            Items.Clear();

            ErrorMessage =
                "操作履歴を読み込めませんでした。" +
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
        IEnumerable<AuditLogListData>
            query = _allItems;


        // ----------------------------------------
        // 操作種別
        // ----------------------------------------

        if (SelectedActionFilter?.Value
            is AuditActionType actionType)
        {
            query =
                query.Where(x =>
                    x.ActionType ==
                    actionType);
        }


        // ----------------------------------------
        // 対象種別
        // ----------------------------------------

        if (SelectedEntityFilter?.Value
            is AuditEntityType entityType)
        {
            query =
                query.Where(x =>
                    x.EntityType ==
                    entityType);
        }


        // ----------------------------------------
        // Keyword
        // ----------------------------------------

        var keyword =
            SearchText.Trim();

        if (!string.IsNullOrWhiteSpace(
                keyword))
        {
            query =
                query.Where(x =>
                MatchesKeyword(
                    x,
                    keyword));
        }


        // Repositoryでも降順だが、
        // フィルター後も念のため日時順を保証する。
        _filteredItems =
            query
                .OrderByDescending(x =>
                    x.OccurredAtUtc)
                .ToList();


        CurrentPage =
            1;

        ApplyPage();
    }


    // ============================================
    // Keyword
    // ============================================

    private static bool MatchesKeyword(
        AuditLogListData source,
        string keyword)
    {
        var actionText =
            AuditLogListItemViewModel
                .GetActionTypeText(
                    source.ActionType);

        var entityText =
            AuditLogListItemViewModel
                .GetEntityTypeText(
                    source.EntityType);

        return
            source.OperatorName.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase) ||

            actionText.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase) ||

            entityText.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase) ||

            source.EntityId
                .ToString()
                .Contains(
                    keyword,
                    StringComparison.OrdinalIgnoreCase) ||

            (source.Reason?.Contains(
                keyword,
                StringComparison.OrdinalIgnoreCase)
                ?? false);
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
                new AuditLogListItemViewModel(
                    source,
                    OpenDetail));
        }

        NotifyListProperties();
    }


    // ============================================
    // Detail
    // ============================================

    private void OpenDetail(
        Guid auditLogId)
    {
        if (auditLogId == Guid.Empty)
        {
            return;
        }

        DetailRequested?.Invoke(
            auditLogId);
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
    // Reset Filter
    // ============================================

    [RelayCommand]
    private void ResetFilter()
    {
        SearchText =
            string.Empty;

        SelectedActionFilter =
            ActionFilterOptions
                .FirstOrDefault();

        SelectedEntityFilter =
            EntityFilterOptions
                .FirstOrDefault();

        ApplyFilter();
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
