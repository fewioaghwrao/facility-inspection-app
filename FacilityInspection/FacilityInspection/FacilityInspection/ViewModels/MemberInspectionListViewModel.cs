using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class MemberInspectionListViewModel
    : ViewModelBase
{
    private const int PageSize = 5;

    private readonly Guid
        _operatorId;

    private readonly InspectionRepository
        _inspectionRepository;


    public MemberInspectionListViewModel(
        Guid operatorId,
        InspectionRepository inspectionRepository)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        _operatorId =
            operatorId;

        _inspectionRepository =
            inspectionRepository;

        _ = LoadAsync();
    }


    public string Title =>
        "点検一覧";

    public string Description =>
        "担当している点検を確認できます。";


    public ObservableCollection<
        MemberInspectionListItemViewModel>
        Items
    { get; } = [];


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEmpty))]
    [NotifyPropertyChangedFor(
        nameof(CanPreviousPage))]
    [NotifyPropertyChangedFor(
        nameof(CanNextPage))]
    private bool isLoading;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(PageText))]
    [NotifyPropertyChangedFor(
        nameof(CanPreviousPage))]
    [NotifyPropertyChangedFor(
        nameof(CanNextPage))]
    private int pageNumber = 1;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(TotalPages))]
    [NotifyPropertyChangedFor(
        nameof(PageText))]
    [NotifyPropertyChangedFor(
        nameof(CanNextPage))]
    private int totalCount;


    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    public bool IsEmpty =>
        !IsLoading &&
        Items.Count == 0;


    public int TotalPages =>
        Math.Max(
            1,
            (int)Math.Ceiling(
                TotalCount /
                (double)PageSize));


    public string PageText =>
        $"{PageNumber} / {TotalPages}";


    public string TotalCountText =>
        $"全 {TotalCount} 件";


    public bool CanPreviousPage =>
        !IsLoading &&
        PageNumber > 1;


    public bool CanNextPage =>
        !IsLoading &&
        PageNumber < TotalPages;


    // ============================================
    // 前ページ
    // ============================================

    [RelayCommand]
    private async Task PreviousPageAsync()
    {
        if (!CanPreviousPage)
        {
            return;
        }

        PageNumber--;

        await LoadPageAsync();
    }


    // ============================================
    // 次ページ
    // ============================================

    [RelayCommand]
    private async Task NextPageAsync()
    {
        if (!CanNextPage)
        {
            return;
        }

        PageNumber++;

        await LoadPageAsync();
    }


    // ============================================
    // 更新
    // ============================================

    [RelayCommand]
    private async Task RefreshAsync()
    {
        PageNumber = 1;

        await LoadAsync();
    }


    // ============================================
    // 初期読込
    // ============================================

    private async Task LoadAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            TotalCount =
                await _inspectionRepository
                    .GetCountForOperatorAsync(
                        _operatorId);

            if (PageNumber > TotalPages)
            {
                PageNumber =
                    TotalPages;
            }

            await LoadPageCoreAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検一覧を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading = false;

            RefreshCalculatedProperties();
        }
    }


    // ============================================
    // ページ切替
    // ============================================

    private async Task LoadPageAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            await LoadPageCoreAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検一覧を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading = false;

            RefreshCalculatedProperties();
        }
    }


    private async Task LoadPageCoreAsync()
    {
        var rows =
            await _inspectionRepository
                .GetPageForOperatorAsync(
                    _operatorId,
                    PageNumber,
                    PageSize);

        Items.Clear();

        foreach (var row in rows)
        {
            Items.Add(
                new MemberInspectionListItemViewModel(
                    row));
        }

        RefreshCalculatedProperties();
    }


    private void RefreshCalculatedProperties()
    {
        OnPropertyChanged(
            nameof(IsEmpty));

        OnPropertyChanged(
            nameof(TotalPages));

        OnPropertyChanged(
            nameof(PageText));

        OnPropertyChanged(
            nameof(TotalCountText));

        OnPropertyChanged(
            nameof(CanPreviousPage));

        OnPropertyChanged(
            nameof(CanNextPage));
    }
}


// ============================================
// 一覧1行
// ============================================

public sealed class MemberInspectionListItemViewModel
{
    public MemberInspectionListItemViewModel(
        InspectionListData data)
    {
        ArgumentNullException.ThrowIfNull(
            data);

        ScheduledDateText =
            $"{data.ScheduledDate.Year}/" +
            $"{data.ScheduledDate.Month:00}/" +
            $"{data.ScheduledDate.Day:00}";

        EquipmentText =
            $"{data.EquipmentCode} " +
            $"{data.EquipmentName}";

        LocationText =
            $"{data.FactorySiteName} / " +
            $"{data.LocationName}";

        TemplateName =
            data.TemplateName;

        StatusText =
            data.Status switch
            {
                InspectionStatus.NotStarted =>
                    "未実施",

                InspectionStatus.InProgress =>
                    "実施中",

                InspectionStatus.Completed =>
                    "完了・承認待ち",

                InspectionStatus.Returned =>
                    "差し戻し",

                InspectionStatus.Approved =>
                    "承認済み",

                _ =>
                    "-"
            };

        AbnormalCount =
            data.AbnormalCount;
    }


    public string ScheduledDateText { get; }

    public string EquipmentText { get; }

    public string LocationText { get; }

    public string TemplateName { get; }

    public string StatusText { get; }

    public int AbnormalCount { get; }

    public string AbnormalText =>
        AbnormalCount > 0
            ? $"異常 {AbnormalCount} 件"
            : "異常なし";
}
