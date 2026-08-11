using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class ApprovalPendingListViewModel
    : ViewModelBase
{
    private readonly InspectionRepository
        _inspectionRepository;

    public Action<Guid>? DetailRequested
    {
        get;
        set;
    }

    public ObservableCollection<
        ApprovalPendingListItemViewModel>
        Items
    { get; } = [];

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    public bool IsEmpty =>
        !IsLoading &&
        Items.Count == 0;

    public ApprovalPendingListViewModel(
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        _inspectionRepository =
            inspectionRepository;

        _ = LoadAsync();
    }

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadAsync();
    }

    private async Task LoadAsync()
    {
        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var rows =
                await _inspectionRepository
                    .GetApprovalPendingAsync();

            Items.Clear();

            foreach (var row in rows)
            {
                if (row.InspectionId is not Guid
                    inspectionId)
                {
                    continue;
                }

                Items.Add(
                    new ApprovalPendingListItemViewModel(
                        row.ScheduleId,
                        inspectionId,
                        row.ScheduledDate,
                        row.FactorySiteName,
                        row.LocationName,
                        row.EquipmentCode,
                        row.EquipmentName,
                        row.TemplateName,
                        row.OperatorName,
                        row.ResultCount,
                        row.AbnormalCount,
                        row.PhotoCount,
                        OpenDetail));
            }

            OnPropertyChanged(
                nameof(IsEmpty));
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"承認待ち一覧を取得できませんでした。" +
                $"{Environment.NewLine}" +
                exception.Message;
        }
        finally
        {
            IsLoading = false;

            OnPropertyChanged(
                nameof(IsEmpty));
        }
    }

    private void OpenDetail(
        Guid scheduleId)
    {
        DetailRequested?.Invoke(
            scheduleId);
    }
}