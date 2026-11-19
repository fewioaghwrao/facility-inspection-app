using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed class InspectionEntryViewModel
    : ViewModelBase
{
    private readonly Guid
        _scheduleId;

    private readonly Guid
        _operatorId;


    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<
        Task<InspectionEntryData>>
        _startOrResumeAsync;


    private readonly Func<
        IReadOnlyCollection<
            InspectionCompletionItemData>,
        Task>
        _completeAsync;


    private readonly Action<
        InspectionEntryItemViewModel>
        _cleanupUnsavedPhotos;


    private readonly Func<
        IReadOnlyCollection<
            InspectionCompletionItemData>,
        Task>
        _saveDraftAsync;

    private readonly Action
        _backRequested;


    // ============================================
    // State
    // ============================================

    private bool _isLoading;

    private bool _isSaving;

    private string?
        _draftStatusMessage;

    private bool
        _isCompletionConfirmVisible;

    private bool
        _isCompletionSuccessVisible;

    private string?
        _errorMessage;

    private string?
        _validationMessage;

    private string?
        _completionErrorMessage;

    private string
        _scheduledDateText =
            string.Empty;

    private string
        _locationText =
            string.Empty;

    private string
        _equipmentText =
            string.Empty;

    private string
        _templateName =
            string.Empty;

    private string
        _statusText =
            string.Empty;


    private List<
        InspectionCompletionItemData>?
        _pendingCompletionItems;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

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


        _scheduleId =
            scheduleId;

        _operatorId =
            operatorId;

        _backRequested =
            backRequested;


        /*
         * optional CancellationToken を持つため
         * method groupではなくlambdaでラップする。
         */
        _startOrResumeAsync =
            () =>
                inspectionRepository
                    .StartOrResumeAsync(
                        scheduleId,
                        operatorId);


        _completeAsync =
            items =>
                inspectionRepository
                    .CompleteAsync(
                        scheduleId,
                        operatorId,
                        items);


        _saveDraftAsync =
            items =>
                inspectionRepository
                    .SaveDraftAsync(
                        scheduleId,
                        operatorId,
                        items);


        /*
         * 本番では実際の写真ファイルを削除する。
         */
        _cleanupUnsavedPhotos =
            item =>
                item.CleanupUnsavedPhotos();


        BackCommand =
            new RelayCommand(
                Back);

        ReviewCompletionCommand =
            new RelayCommand(
                ReviewCompletion);

        CancelCompletionCommand =
            new RelayCommand(
                CancelCompletion);

        ConfirmCompletionCommand =
            new AsyncRelayCommand(
                ConfirmCompletionAsync);

        FinishCompletionCommand =
            new RelayCommand(
                FinishCompletion);

        SaveDraftCommand =
            new AsyncRelayCommand(
                SaveDraftAsync);


        /*
         * 本番では従来どおり
         * 生成直後に初期化する。
         */
        _ = InitializeAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal InspectionEntryViewModel(
        Guid scheduleId,
        Guid operatorId,
        Func<Task<InspectionEntryData>>
            startOrResumeAsync,
        Func<
            IReadOnlyCollection<
                InspectionCompletionItemData>,
            Task>
            completeAsync,
        Action<InspectionEntryItemViewModel>
            cleanupUnsavedPhotos,
        Action backRequested,
        Func<
            IReadOnlyCollection<
                InspectionCompletionItemData>,
            Task>?
            saveDraftAsync = null)
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
            startOrResumeAsync);

        ArgumentNullException.ThrowIfNull(
            completeAsync);

        ArgumentNullException.ThrowIfNull(
            cleanupUnsavedPhotos);

        ArgumentNullException.ThrowIfNull(
            backRequested);


        _scheduleId =
            scheduleId;

        _operatorId =
            operatorId;

        _startOrResumeAsync =
            startOrResumeAsync;

        _completeAsync =
            completeAsync;

        _cleanupUnsavedPhotos =
            cleanupUnsavedPhotos;

        _saveDraftAsync =
            saveDraftAsync ??
            (_ => Task.CompletedTask);

        _backRequested =
            backRequested;


        BackCommand =
            new RelayCommand(
                Back);

        ReviewCompletionCommand =
            new RelayCommand(
                ReviewCompletion);

        CancelCompletionCommand =
            new RelayCommand(
                CancelCompletion);

        ConfirmCompletionCommand =
            new AsyncRelayCommand(
                ConfirmCompletionAsync);

        FinishCompletionCommand =
            new RelayCommand(
                FinishCompletion);

        SaveDraftCommand =
            new AsyncRelayCommand(
                SaveDraftAsync);


        /*
         * テストでは自動初期化しない。
         *
         * InitializeAsync()を明示的にawaitする。
         */
    }


    // ============================================
    // Basic
    // ============================================

    public string Title =>
        "点検実施";


    public string Description =>
        "点検項目を確認し、現場の状態を入力します。";


    public Guid ScheduleId =>
        _scheduleId;


    // ============================================
    // Items
    // ============================================

    public ObservableCollection<
        InspectionEntryItemViewModel>
        Items
    {
        get;
    } = [];


    // ============================================
    // Commands
    // ============================================

    public IRelayCommand
        BackCommand
    {
        get;
    }


    public IRelayCommand
        ReviewCompletionCommand
    {
        get;
    }


    public IRelayCommand
        CancelCompletionCommand
    {
        get;
    }


    public IAsyncRelayCommand
        ConfirmCompletionCommand
    {
        get;
    }


    public IAsyncRelayCommand
        SaveDraftCommand
    {
        get;
    }


    // ============================================
    // Draft
    // ============================================

    public string? DraftStatusMessage
    {
        get =>
            _draftStatusMessage;
        private set
        {
            if (SetProperty(
                    ref _draftStatusMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(
                        HasDraftStatusMessage));
            }
        }
    }


    public bool HasDraftStatusMessage =>
        !string.IsNullOrWhiteSpace(
            DraftStatusMessage);


    public IRelayCommand
        FinishCompletionCommand
    {
        get;
    }


    // ============================================
    // Loading
    // ============================================

    public bool IsLoading
    {
        get =>
            _isLoading;

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


    // ============================================
    // Saving
    // ============================================

    public bool IsSaving
    {
        get =>
            _isSaving;

        private set
        {
            if (SetProperty(
                    ref _isSaving,
                    value))
            {
                OnPropertyChanged(
                    nameof(IsNotSaving));
            }
        }
    }


    public bool IsNotSaving =>
        !IsSaving;


    // ============================================
    // Content
    // ============================================

    public bool IsContentVisible =>
        !IsLoading &&
        !HasError;


    // ============================================
    // Error
    // ============================================

    public string? ErrorMessage
    {
        get =>
            _errorMessage;

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


    // ============================================
    // Validation
    // ============================================

    public string? ValidationMessage
    {
        get =>
            _validationMessage;

        private set
        {
            if (SetProperty(
                    ref _validationMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(
                        HasValidationMessage));
            }
        }
    }


    public bool HasValidationMessage =>
        !string.IsNullOrWhiteSpace(
            ValidationMessage);


    // ============================================
    // Completion Error
    // ============================================

    public string? CompletionErrorMessage
    {
        get =>
            _completionErrorMessage;

        private set
        {
            if (SetProperty(
                    ref _completionErrorMessage,
                    value))
            {
                OnPropertyChanged(
                    nameof(
                        HasCompletionError));
            }
        }
    }


    public bool HasCompletionError =>
        !string.IsNullOrWhiteSpace(
            CompletionErrorMessage);


    // ============================================
    // Completion Dialog
    // ============================================

    public bool IsCompletionConfirmVisible
    {
        get =>
            _isCompletionConfirmVisible;

        private set =>
            SetProperty(
                ref _isCompletionConfirmVisible,
                value);
    }


    public bool IsCompletionSuccessVisible
    {
        get =>
            _isCompletionSuccessVisible;

        private set =>
            SetProperty(
                ref _isCompletionSuccessVisible,
                value);
    }


    // ============================================
    // Display
    // ============================================

    public string ScheduledDateText
    {
        get =>
            _scheduledDateText;

        private set =>
            SetProperty(
                ref _scheduledDateText,
                value);
    }


    public string LocationText
    {
        get =>
            _locationText;

        private set =>
            SetProperty(
                ref _locationText,
                value);
    }


    public string EquipmentText
    {
        get =>
            _equipmentText;

        private set =>
            SetProperty(
                ref _equipmentText,
                value);
    }


    public string TemplateName
    {
        get =>
            _templateName;

        private set =>
            SetProperty(
                ref _templateName,
                value);
    }


    public string StatusText
    {
        get =>
            _statusText;

        private set =>
            SetProperty(
                ref _statusText,
                value);
    }


    public string NextStepMessage =>
        "途中で中断する場合は「一時保存」を押してください。" +
        "入力内容を確認し、問題がなければ点検を完了します。";


    public string CompletionConfirmMessage =>
        "入力内容を保存し、点検状態を「完了・承認待ち」に変更します。";


    public string CompletionSuccessMessage =>
        "点検が完了しました。管理者の承認待ちとして保存されています。";


    // ============================================
    // Initialize
    // ============================================

    internal async Task InitializeAsync()
    {
        IsLoading =
            true;

        ErrorMessage =
            null;


        try
        {
            var data =
                await _startOrResumeAsync();


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
                GetStatusText(
                    data.Status);


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
            IsLoading =
                false;
        }
    }


    // ============================================
    // Save Draft
    // ============================================

    private async Task SaveDraftAsync()
    {
        if (IsSaving)
        {
            return;
        }

        ValidationMessage =
            null;

        DraftStatusMessage =
            null;

        var draftItems =
            new List<
                InspectionCompletionItemData>(
                Items.Count);

        var errorCount =
            0;

        foreach (var item
                 in Items)
        {
            if (item.TryCreateDraftData(
                    out var draftItem))
            {
                draftItems.Add(
                    draftItem);
            }
            else
            {
                errorCount++;
            }
        }

        if (errorCount > 0)
        {
            ValidationMessage =
                $"入力内容に {errorCount} 件のエラーがあります。" +
                "赤字の項目を修正してから一時保存してください。";

            return;
        }

        IsSaving =
            true;

        try
        {
            await _saveDraftAsync(
                draftItems);

            foreach (var item
                     in Items)
            {
                item.MarkPhotosPersisted();
            }

            DraftStatusMessage =
                $"一時保存しました（{DateTime.Now:HH:mm}）";
        }
        catch (Exception exception)
        {
            ValidationMessage =
                "一時保存できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Review Completion
    // ============================================

    private void ReviewCompletion()
    {
        ValidationMessage =
            null;

        CompletionErrorMessage =
            null;

        _pendingCompletionItems =
            null;


        var completionItems =
            new List<
                InspectionCompletionItemData>(
                Items.Count);


        var errorCount =
            0;


        foreach (var item
                 in Items)
        {
            if (item.TryCreateCompletionData(
                    out var completionItem))
            {
                completionItems.Add(
                    completionItem);
            }
            else
            {
                errorCount++;
            }
        }


        if (errorCount > 0)
        {
            ValidationMessage =
                $"入力内容に {errorCount} 件のエラーがあります。" +
                "赤字の項目を修正してください。";

            return;
        }


        _pendingCompletionItems =
            completionItems;


        IsCompletionConfirmVisible =
            true;
    }


    // ============================================
    // Cancel Completion
    // ============================================

    private void CancelCompletion()
    {
        if (IsSaving)
        {
            return;
        }


        IsCompletionConfirmVisible =
            false;


        CompletionErrorMessage =
            null;


        _pendingCompletionItems =
            null;
    }


    // ============================================
    // Confirm Completion
    // ============================================

    private async Task ConfirmCompletionAsync()
    {
        if (_pendingCompletionItems is null)
        {
            CompletionErrorMessage =
                "完了対象の入力内容を確認できませんでした。" +
                "いったんキャンセルして、もう一度入力内容を確認してください。";

            return;
        }


        IsSaving =
            true;


        CompletionErrorMessage =
            null;


        try
        {
            await _completeAsync(
                _pendingCompletionItems);


            StatusText =
                GetStatusText(
                    InspectionStatus.Completed);


            IsCompletionConfirmVisible =
                false;


            IsCompletionSuccessVisible =
                true;


            _pendingCompletionItems =
                null;
        }
        catch (Exception exception)
        {
            CompletionErrorMessage =
                "点検を完了できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Finish Completion
    // ============================================

    private void FinishCompletion()
    {
        IsCompletionSuccessVisible =
            false;


        _backRequested();
    }


    // ============================================
    // Back
    // ============================================

    private void Back()
    {
        if (IsSaving)
        {
            return;
        }


        foreach (var item
                 in Items)
        {
            /*
             * 本番:
             * item.CleanupUnsavedPhotos()
             *
             * テスト:
             * 実ファイルを触らないDelegate
             */
            _cleanupUnsavedPhotos(
                item);
        }


        _backRequested();
    }


    // ============================================
    // Status
    // ============================================

    private static string GetStatusText(
        InspectionStatus status)
    {
        return status switch
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
    }
}

