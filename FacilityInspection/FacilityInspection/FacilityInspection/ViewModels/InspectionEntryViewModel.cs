using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed class InspectionEntryViewModel : ViewModelBase
{
    private readonly Guid _scheduleId;
    private readonly Guid _operatorId;
    private readonly InspectionRepository _inspectionRepository;
    private readonly Action _backRequested;

    private bool _isLoading;
    private string? _errorMessage;
    private string _scheduledDateText = string.Empty;
    private string _locationText = string.Empty;
    private string _equipmentText = string.Empty;
    private string _templateName = string.Empty;
    private string _statusText = string.Empty;

    public InspectionEntryViewModel(
        Guid scheduleId,
        Guid operatorId,
        InspectionRepository inspectionRepository,
        Action backRequested)
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
                "点検担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        ArgumentNullException.ThrowIfNull(
            backRequested);

        _scheduleId = scheduleId;
        _operatorId = operatorId;
        _inspectionRepository = inspectionRepository;
        _backRequested = backRequested;

        BackCommand =
            new RelayCommand(
                Back);

        _ = InitializeAsync();
    }

    public string Title =>
        "点検実施";

    public string Description =>
        "点検項目を確認し、現場の状態を入力します。";

    public ObservableCollection<
        InspectionEntryItemViewModel>
        Items
    { get; } = [];

    public IRelayCommand BackCommand { get; }

    public bool IsLoading
    {
        get => _isLoading;

        private set
        {
            if (SetProperty(
                    ref _isLoading,
                    value))
            {
                OnPropertyChanged(
                    nameof(IsContentVisible));
            }
        }
    }

    public bool IsContentVisible =>
        !IsLoading &&
        !HasError;

    public string? ErrorMessage
    {
        get => _errorMessage;

        private set
        {
            if (SetProperty(
                    ref _errorMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(HasError));

                OnPropertyChanged(
                    nameof(IsContentVisible));
            }
        }
    }

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);

    public string ScheduledDateText
    {
        get => _scheduledDateText;

        private set => SetProperty(
            ref _scheduledDateText,
            value);
    }

    public string LocationText
    {
        get => _locationText;

        private set => SetProperty(
            ref _locationText,
            value);
    }

    public string EquipmentText
    {
        get => _equipmentText;

        private set => SetProperty(
            ref _equipmentText,
            value);
    }

    public string TemplateName
    {
        get => _templateName;

        private set => SetProperty(
            ref _templateName,
            value);
    }

    public string StatusText
    {
        get => _statusText;

        private set => SetProperty(
            ref _statusText,
            value);
    }

    public string NextStepMessage =>
        "入力内容の確認・点検完了保存は次工程で接続します。";

    private async Task InitializeAsync()
    {
        IsLoading = true;
        ErrorMessage = null;

        try
        {
            var data =
                await _inspectionRepository
                    .StartOrResumeAsync(
                        _scheduleId,
                        _operatorId);

            ScheduledDateText =
                $"{data.ScheduledDate.Year}年" +
                $"{data.ScheduledDate.Month}月" +
                $"{data.ScheduledDate.Day}日";

            LocationText =
                $"{data.FactorySiteName} / " +
                $"{data.LocationName}";

            EquipmentText =
                $"{data.EquipmentCode} " +
                $"{data.EquipmentName}";

            TemplateName =
                data.TemplateName;

            StatusText =
                data.Status switch
                {
                    InspectionStatus.InProgress =>
                            "実施中",

                    InspectionStatus.Completed =>
                            "完了・承認待ち",

                    InspectionStatus.Returned =>
                            "差し戻し",

                    InspectionStatus.Approved =>
                            "承認済み",

                    _ =>
                        "未実施"
                };

            Items.Clear();

            foreach (var item
                     in data.Items)
            {
                Items.Add(
                    new InspectionEntryItemViewModel(
                        item));
            }
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検を開始できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading = false;
        }
    }

    private void Back()
    {
        _backRequested();
    }
}
