using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class MemberInspectionListViewModel
    : ViewModelBase
{
    private const int PageSize = 5;


    // ============================================
    // Dependencies
    // ============================================

    private readonly Guid
        _operatorId;

    private readonly Func<
        Guid,
        Task<int>>
        _getCountForOperatorAsync;

    private readonly Func<
        Guid,
        int,
        int,
        Task<IReadOnlyList<InspectionListData>>>
        _getPageForOperatorAsync;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

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


        /*
         * Repository側にはoptional CancellationTokenがあるため
         * method groupではなくlambdaでラップする。
         */
        _getCountForOperatorAsync =
            id =>
                inspectionRepository
                    .GetCountForOperatorAsync(
                        id);


        _getPageForOperatorAsync =
            (
                id,
                pageNumber,
                pageSize) =>
                inspectionRepository
                    .GetPageForOperatorAsync(
                        id,
                        pageNumber,
                        pageSize);


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

    internal MemberInspectionListViewModel(
        Guid operatorId,
        Func<
            Guid,
            Task<int>>
            getCountForOperatorAsync,
        Func<
            Guid,
            int,
            int,
            Task<IReadOnlyList<InspectionListData>>>
            getPageForOperatorAsync)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            getCountForOperatorAsync);

        ArgumentNullException.ThrowIfNull(
            getPageForOperatorAsync);


        _operatorId =
            operatorId;

        _getCountForOperatorAsync =
            getCountForOperatorAsync;

        _getPageForOperatorAsync =
            getPageForOperatorAsync;


        /*
         * テスト用では自動ロードしない。
         */
    }


    // ============================================
    // Basic Information
    // ============================================

    public string Title =>
        "点検一覧";


    public string Description =>
        "担当している点検を確認できます。";


    // ============================================
    // Items
    // ============================================

    public ObservableCollection<
        MemberInspectionListItemViewModel>
        Items
    { get; } = [];


    // ============================================
    // Loading
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEmpty))]
    [NotifyPropertyChangedFor(
        nameof(CanPreviousPage))]
    [NotifyPropertyChangedFor(
        nameof(CanNextPage))]
    private bool isLoading;


    // ============================================
    // Error
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    // ============================================
    // Paging
    // ============================================

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


    // ============================================
    // Calculated Properties
    // ============================================

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
    // Previous Page
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
    // Next Page
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
    // Refresh
    // ============================================

    [RelayCommand]
    private async Task RefreshAsync()
    {
        PageNumber =
            1;


        await LoadAsync();
    }


    // ============================================
    // Initial / Full Load
    // ============================================

    internal async Task LoadAsync()
    {
        IsLoading =
            true;

        ErrorMessage =
            null;


        try
        {
            TotalCount =
                await _getCountForOperatorAsync(
                    _operatorId);


            /*
             * 削除等によって総件数が減り、
             * 現在ページが存在しなくなった場合に補正する。
             */
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
            IsLoading =
                false;


            RefreshCalculatedProperties();
        }
    }


    // ============================================
    // Page Load
    // ============================================

    private async Task LoadPageAsync()
    {
        IsLoading =
            true;

        ErrorMessage =
            null;


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
            IsLoading =
                false;


            RefreshCalculatedProperties();
        }
    }


    // ============================================
    // Page Load Core
    // ============================================

    private async Task LoadPageCoreAsync()
    {
        var rows =
            await _getPageForOperatorAsync(
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


    // ============================================
    // Calculated Properties Refresh
    // ============================================

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


    public string ScheduledDateText
    {
        get;
    }


    public string EquipmentText
    {
        get;
    }


    public string LocationText
    {
        get;
    }


    public string TemplateName
    {
        get;
    }


    public string StatusText
    {
        get;
    }


    public int AbnormalCount
    {
        get;
    }


    public string AbnormalText =>
        AbnormalCount > 0
            ? $"異常 {AbnormalCount} 件"
            : "異常なし";
}