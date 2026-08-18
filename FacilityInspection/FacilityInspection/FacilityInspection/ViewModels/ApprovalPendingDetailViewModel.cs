using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class ApprovalPendingDetailViewModel
    : ViewModelBase
{
    private readonly Guid
        _operatorId;

    private readonly Func<
        Task<InspectionDetailData?>>
        _loadDetailAsync;

    private readonly Func<Task>
        _approveAsync;

    private readonly Func<
        string,
        Task>
        _returnAsync;


    // ============================================
    // Navigation
    // ============================================

    /// <summary>
    /// 詳細画面から承認待ち一覧へ戻る要求。
    /// </summary>
    public Action? BackRequested
    {
        get;
        set;
    }


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public ApprovalPendingDetailViewModel(
        InspectionRepository inspectionRepository,
        Guid scheduleId,
        Guid operatorId)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        ScheduleId =
            scheduleId;

        _operatorId =
            operatorId;

        _loadDetailAsync =
            () =>
                inspectionRepository
                    .GetDetailAsync(
                        scheduleId);

        _approveAsync =
            () =>
                inspectionRepository
                    .ApproveAsync(
                        scheduleId,
                        operatorId);

        _returnAsync =
            reason =>
                inspectionRepository
                    .ReturnAsync(
                        scheduleId,
                        reason,
                        operatorId);

        _ = LoadAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal ApprovalPendingDetailViewModel(
        Guid scheduleId,
        Guid operatorId,
        Func<
            Task<InspectionDetailData?>>
            loadDetailAsync,
        Func<Task> approveAsync,
        Func<
            string,
            Task>
            returnAsync)
    {
        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            loadDetailAsync);

        ArgumentNullException.ThrowIfNull(
            approveAsync);

        ArgumentNullException.ThrowIfNull(
            returnAsync);

        ScheduleId =
            scheduleId;

        _operatorId =
            operatorId;

        _loadDetailAsync =
            loadDetailAsync;

        _approveAsync =
            approveAsync;

        _returnAsync =
            returnAsync;
    }


    // ============================================
    // Basic
    // ============================================

    public Guid ScheduleId
    {
        get;
    }

    public string Title =>
        "点検承認";

    public string Description =>
        "完了した点検内容を確認し、承認または差し戻しを行います。";


    // ============================================
    // State
    // ============================================

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    private string? operationMessage;

    [ObservableProperty]
    private string? operationErrorMessage;


    // ============================================
    // Inspection
    // ============================================

    [ObservableProperty]
    private Guid? inspectionId;

    [ObservableProperty]
    private DateOnly scheduledDate;

    [ObservableProperty]
    private string factorySiteName =
        string.Empty;

    [ObservableProperty]
    private string locationName =
        string.Empty;

    [ObservableProperty]
    private string equipmentCode =
        string.Empty;

    [ObservableProperty]
    private string equipmentName =
        string.Empty;

    [ObservableProperty]
    private string templateName =
        string.Empty;

    [ObservableProperty]
    private string operatorName =
        string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(StatusText))]
    [NotifyPropertyChangedFor(
        nameof(StatusBackground))]
    [NotifyPropertyChangedFor(
        nameof(StatusForeground))]
    [NotifyPropertyChangedFor(
        nameof(CanReview))]
    private InspectionStatus status =
        InspectionStatus.NotStarted;


    // ============================================
    // Return Dialog
    // ============================================

    [ObservableProperty]
    private bool isReturnDialogOpen;

    [ObservableProperty]
    private string returnReason =
        string.Empty;


    // ============================================
    // Detail Collections
    // ============================================

    public ObservableCollection<
        InspectionResultDetailItemViewModel>
        Results
    { get; } = [];

    public ObservableCollection<
        InspectionPhotoDetailItemViewModel>
        Photos
    { get; } = [];

    public ObservableCollection<
        InspectionPhotoDetailItemViewModel>
        GeneralPhotos
    { get; } = [];


    // ============================================
    // Calculated Properties
    // ============================================

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    public bool HasInspection =>
        InspectionId.HasValue;

    public bool HasResults =>
        Results.Count > 0;

    public bool HasPhotos =>
        Photos.Count > 0;

    public bool HasGeneralPhotos =>
        GeneralPhotos.Count > 0;

    /// <summary>
    /// 承認・差し戻し可能か。
    /// Completed の場合のみ可能。
    /// </summary>
    public bool CanReview =>
        InspectionId.HasValue &&
        Status ==
        InspectionStatus.Completed &&
        !IsLoading;

    public string ScheduledDateText =>
        ScheduledDate == default
            ? "-"
            : ScheduledDate.ToString(
                "yyyy/MM/dd");

    public string LocationDisplayName =>
        string.IsNullOrWhiteSpace(
            FactorySiteName)
            ? "-"
            : $"{FactorySiteName} / {LocationName}";

    public string EquipmentDisplayName =>
        string.IsNullOrWhiteSpace(
            EquipmentCode)
            ? "-"
            : $"{EquipmentCode}  {EquipmentName}";

    public string ResultCountText =>
        $"{Results.Count}項目";

    public int AbnormalCount =>
        Results.Count(
            x => x.IsAbnormal);

    public string AbnormalCountText =>
        AbnormalCount == 0
            ? "異常なし"
            : $"異常 {AbnormalCount}件";

    public string PhotoCountText =>
        $"{Photos.Count}枚";


    // ============================================
    // Status Display
    // ============================================

    public string StatusText =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "未実施",

            InspectionStatus.InProgress =>
                "実施中",

            InspectionStatus.Completed =>
                "完了・承認待ち",

            InspectionStatus.Approved =>
                "承認済み",

            InspectionStatus.Returned =>
                "差し戻し",

            _ =>
                Status.ToString()
        };

    public string StatusBackground =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "#F1F5F9",

            InspectionStatus.InProgress =>
                "#DBEAFE",

            InspectionStatus.Completed =>
                "#FFEDD5",

            InspectionStatus.Approved =>
                "#DCFCE7",

            InspectionStatus.Returned =>
                "#FEE2E2",

            _ =>
                "#F1F5F9"
        };

    public string StatusForeground =>
        Status switch
        {
            InspectionStatus.NotStarted =>
                "#475569",

            InspectionStatus.InProgress =>
                "#1D4ED8",

            InspectionStatus.Completed =>
                "#C2410C",

            InspectionStatus.Approved =>
                "#15803D",

            InspectionStatus.Returned =>
                "#B91C1C",

            _ =>
                "#475569"
        };


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

            OperationMessage =
                null;

            OperationErrorMessage =
                null;

            var detail =
                await _loadDetailAsync();

            if (detail is null)
            {
                ErrorMessage =
                    "点検実施データが見つかりません。";

                return;
            }

            InspectionId =
                detail.InspectionId;

            ScheduledDate =
                detail.ScheduledDate;

            FactorySiteName =
                detail.FactorySiteName;

            LocationName =
                detail.LocationName;

            EquipmentCode =
                detail.EquipmentCode;

            EquipmentName =
                detail.EquipmentName;

            TemplateName =
                detail.TemplateName;

            OperatorName =
                detail.OperatorName;

            Status =
                detail.Status;


            Results.Clear();

            foreach (var result in
                     detail.Results)
            {
                Results.Add(
                    new InspectionResultDetailItemViewModel(
                        result));
            }


            Photos.Clear();

            GeneralPhotos.Clear();

            foreach (var photo in
                     detail.Photos)
            {
                var item =
                    new InspectionPhotoDetailItemViewModel(
                        photo);

                Photos.Add(
                    item);

                if (item.IsGeneralPhoto)
                {
                    GeneralPhotos.Add(
                        item);
                }
            }

            NotifyCalculatedProperties();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "承認対象の点検詳細を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;

            OnPropertyChanged(
                nameof(CanReview));
        }
    }


    // ============================================
    // Approve
    // ============================================

    [RelayCommand]
    private async Task ApproveAsync()
    {
        if (!CanReview)
        {
            return;
        }

        try
        {
            IsLoading =
                true;

            OperationMessage =
                null;

            OperationErrorMessage =
                null;

            await _approveAsync();

            OperationMessage =
                "点検を承認しました。";

            /*
             * 承認済みになったため、
             * 承認待ち一覧へ戻す。
             */
            BackRequested?.Invoke();
        }
        catch (Exception exception)
        {
            OperationErrorMessage =
                "点検を承認できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;

            OnPropertyChanged(
                nameof(CanReview));
        }
    }


    // ============================================
    // Return
    // ============================================

    [RelayCommand]
    private void OpenReturnDialog()
    {
        if (!CanReview)
        {
            return;
        }

        ReturnReason =
            string.Empty;

        OperationErrorMessage =
            null;

        IsReturnDialogOpen =
            true;
    }


    [RelayCommand]
    private void CancelReturn()
    {
        IsReturnDialogOpen =
            false;

        ReturnReason =
            string.Empty;

        OperationErrorMessage =
            null;
    }


    [RelayCommand]
    private async Task ConfirmReturnAsync()
    {
        if (!CanReview)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(
                ReturnReason))
        {
            OperationErrorMessage =
                "差し戻し理由を入力してください。";

            return;
        }

        try
        {
            IsLoading =
                true;

            OperationMessage =
                null;

            OperationErrorMessage =
                null;

            await _returnAsync(
                ReturnReason);

            IsReturnDialogOpen =
                false;

            OperationMessage =
                "点検を差し戻しました。";

            /*
             * 差し戻し済みになったため、
             * 承認待ち一覧へ戻す。
             */
            BackRequested?.Invoke();
        }
        catch (Exception exception)
        {
            OperationErrorMessage =
                "点検を差し戻せませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;

            OnPropertyChanged(
                nameof(CanReview));
        }
    }


    // ============================================
    // Back
    // ============================================

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }


    // ============================================
    // Calculated Property Notification
    // ============================================

    private void NotifyCalculatedProperties()
    {
        OnPropertyChanged(
            nameof(HasInspection));

        OnPropertyChanged(
            nameof(HasResults));

        OnPropertyChanged(
            nameof(HasPhotos));

        OnPropertyChanged(
            nameof(HasGeneralPhotos));

        OnPropertyChanged(
            nameof(ScheduledDateText));

        OnPropertyChanged(
            nameof(LocationDisplayName));

        OnPropertyChanged(
            nameof(EquipmentDisplayName));

        OnPropertyChanged(
            nameof(ResultCountText));

        OnPropertyChanged(
            nameof(AbnormalCount));

        OnPropertyChanged(
            nameof(AbnormalCountText));

        OnPropertyChanged(
            nameof(PhotoCountText));

        OnPropertyChanged(
            nameof(CanReview));
    }
}