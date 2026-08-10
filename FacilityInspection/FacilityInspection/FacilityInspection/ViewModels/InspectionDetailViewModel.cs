using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionDetailViewModel
    : ViewModelBase
{
    private readonly InspectionRepository
        _inspectionRepository;

    /// <summary>
    /// 詳細画面から一覧へ戻る要求。
    /// </summary>
    public Action? BackRequested
    {
        get;
        set;
    }

    public InspectionDetailViewModel(
        InspectionRepository inspectionRepository,
        Guid scheduleId)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        _inspectionRepository =
            inspectionRepository;

        ScheduleId =
            scheduleId;

        _ = LoadAsync();
    }

    public Guid ScheduleId { get; }

    public string Title =>
        "点検実施詳細";

    public string Description =>
        "点検の実施内容、異常、写真を確認します。";

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

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
    [NotifyPropertyChangedFor(nameof(StatusText))]
    [NotifyPropertyChangedFor(nameof(StatusBackground))]
    [NotifyPropertyChangedFor(nameof(StatusForeground))]
    private InspectionStatus status =
        InspectionStatus.NotStarted;

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
        Results.Count(x =>
            x.IsAbnormal);

    public string AbnormalCountText =>
        AbnormalCount == 0
            ? "異常なし"
            : $"異常 {AbnormalCount}件";

    public string PhotoCountText =>
        $"{Photos.Count}枚";

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

            var detail =
                await _inspectionRepository
                    .GetDetailAsync(
                        ScheduleId);

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

                Photos.Add(item);

                if (item.IsGeneralPhoto)
                {
                    GeneralPhotos.Add(item);
                }
            }

            NotifyCalculatedProperties();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検実施詳細を読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void Back()
    {
        BackRequested?.Invoke();
    }

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
    }
}