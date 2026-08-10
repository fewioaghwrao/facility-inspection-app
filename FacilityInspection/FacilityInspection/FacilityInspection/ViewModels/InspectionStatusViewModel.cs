using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionStatusViewModel
    : ViewModelBase
{
    private readonly InspectionRepository
        _inspectionRepository;

    private IReadOnlyList<InspectionListData>
        _allInspections = [];

    public InspectionStatusViewModel(
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        _inspectionRepository =
            inspectionRepository;

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "すべて",
                null));

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "未実施",
                InspectionStatus.NotStarted));

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "実施中",
                InspectionStatus.InProgress));

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "完了・承認待ち",
                InspectionStatus.Completed));

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "承認済み",
                InspectionStatus.Approved));

        StatusFilters.Add(
            new InspectionStatusFilterOptionViewModel(
                "差し戻し",
                InspectionStatus.Returned));

        SelectedStatusFilter =
            StatusFilters.First();

        _ = LoadAsync();
    }

    public string Title =>
        "点検実施状況";

    public string Description =>
        "点検の実施状況、異常件数、登録写真を確認します。";

    public ObservableCollection<
        InspectionStatusListItemViewModel>
        Items
    { get; } = [];

    public ObservableCollection<
        InspectionStatusFilterOptionViewModel>
        StatusFilters
    { get; } = [];

    [ObservableProperty]
    private string searchText = string.Empty;

    [ObservableProperty]
    private InspectionStatusFilterOptionViewModel?
        selectedStatusFilter;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;



    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    public bool IsEmpty =>
        !IsLoading &&
        Items.Count == 0;

    public string CountText =>
        $"{_filteredInspections.Count}件";

    partial void OnSearchTextChanged(
        string value)
    {
        ApplyFilter();
    }

    partial void OnSelectedStatusFilterChanged(
        InspectionStatusFilterOptionViewModel? value)
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

            _allInspections =
                await _inspectionRepository
                    .GetAllAsync();

            ApplyFilter();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検実施状況を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
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
        IEnumerable<InspectionListData> query =
            _allInspections;

        if (SelectedStatusFilter?.Status is
            InspectionStatus selectedStatus)
        {
            query = query.Where(x =>
                x.Status == selectedStatus);
        }

        var keyword =
            SearchText.Trim();

        if (!string.IsNullOrWhiteSpace(keyword))
        {
            query = query.Where(x =>
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

        _filteredInspections =
            query.ToList();

        CurrentPage = 1;

        ApplyPage();
    }

    private const int PageSize = 5;

    private List<InspectionListData> _filteredInspections = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PageText))]
    [NotifyPropertyChangedFor(nameof(HasPreviousPage))]
    [NotifyPropertyChangedFor(nameof(HasNextPage))]
    private int currentPage = 1;

    public int TotalPages =>
        Math.Max(
            1,
            (int)Math.Ceiling(
                _filteredInspections.Count /
                (double)PageSize));

    public string PageText =>
        $"{CurrentPage} / {TotalPages}";

    public bool HasPreviousPage =>
        CurrentPage > 1;

    public bool HasNextPage =>
        CurrentPage < TotalPages;

    private void ApplyPage()
    {
        Items.Clear();

        var pageItems =
            _filteredInspections
                .Skip((CurrentPage - 1) * PageSize)
                .Take(PageSize);

        foreach (var inspection in pageItems)
        {
            Items.Add(
                new InspectionStatusListItemViewModel(
                    inspection));
        }

        OnPropertyChanged(nameof(IsEmpty));
        OnPropertyChanged(nameof(CountText));
        OnPropertyChanged(nameof(PageText));
        OnPropertyChanged(nameof(TotalPages));
        OnPropertyChanged(nameof(HasPreviousPage));
        OnPropertyChanged(nameof(HasNextPage));
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

public sealed class
    InspectionStatusFilterOptionViewModel
{
    public InspectionStatusFilterOptionViewModel(
        string displayName,
        InspectionStatus? status)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            displayName);

        DisplayName = displayName;
        Status = status;
    }

    public string DisplayName { get; }

    public InspectionStatus? Status { get; }
}