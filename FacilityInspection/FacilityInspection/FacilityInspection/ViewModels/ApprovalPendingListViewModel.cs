using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class ApprovalPendingListViewModel
    : ViewModelBase
{
    private readonly Func<
        Task<IReadOnlyList<InspectionListData>>>
        _loadApprovalPendingAsync;


    // ============================================
    // Navigation
    // ============================================

    public Action<Guid>? DetailRequested
    {
        get;
        set;
    }


    // ============================================
    // Items
    // ============================================

    public ObservableCollection<
        ApprovalPendingListItemViewModel>
        Items
    { get; } = [];


    // ============================================
    // State
    // ============================================

    [ObservableProperty]
    private bool isLoading;

    [ObservableProperty]
    private string? errorMessage;

    public bool IsEmpty =>
        !IsLoading &&
        Items.Count == 0;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public ApprovalPendingListViewModel(
        InspectionRepository inspectionRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionRepository);

        _loadApprovalPendingAsync =
            () =>
                inspectionRepository
                    .GetApprovalPendingAsync();

        _ = LoadAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal ApprovalPendingListViewModel(
        Func<
            Task<IReadOnlyList<InspectionListData>>>
            loadApprovalPendingAsync)
    {
        ArgumentNullException.ThrowIfNull(
            loadApprovalPendingAsync);

        _loadApprovalPendingAsync =
            loadApprovalPendingAsync;
    }


    // ============================================
    // Reload
    // ============================================

    [RelayCommand]
    private async Task ReloadAsync()
    {
        await LoadAsync();
    }


    // ============================================
    // Load
    // ============================================

    private async Task LoadAsync()
    {
        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;

            var rows =
                await _loadApprovalPendingAsync();

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
                "承認待ち一覧を取得できませんでした。" +
                Environment.NewLine +
                exception.Message;
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
    // Detail
    // ============================================

    private void OpenDetail(
        Guid scheduleId)
    {
        DetailRequested?.Invoke(
            scheduleId);
    }
}