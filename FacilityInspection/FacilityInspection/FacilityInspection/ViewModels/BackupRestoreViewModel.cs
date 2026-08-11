using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Services.Backup;
using System;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class BackupRestoreViewModel
    : ViewModelBase
{
    private static readonly Guid
        DatabaseAuditEntityId =
            Guid.Parse(
                "B63D80E2-E10A-4E40-8B93-4A3E2C887001");

    private readonly DatabaseBackupService
        _databaseBackupService;

    private readonly BackupFilePickerService
        _filePickerService;

    private readonly AuditLogRepository
        _auditLogRepository;

    private readonly Guid
        _operatorId;

    private IStorageFile?
        _selectedRestoreFile;


    // ============================================
    // Navigation
    // ============================================

    /// <summary>
    /// 復元完了後に画面全体を再生成する。
    /// </summary>
    public Action? RestoreCompleted
    {
        get;
        set;
    }


    // ============================================
    // Constructor
    // ============================================

    public BackupRestoreViewModel(
        DatabaseBackupService databaseBackupService,
        BackupFilePickerService filePickerService,
        AuditLogRepository auditLogRepository,
        Guid operatorId)
    {
        ArgumentNullException.ThrowIfNull(
            databaseBackupService);

        ArgumentNullException.ThrowIfNull(
            filePickerService);

        ArgumentNullException.ThrowIfNull(
            auditLogRepository);

        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        _databaseBackupService =
            databaseBackupService;

        _filePickerService =
            filePickerService;

        _auditLogRepository =
            auditLogRepository;

        _operatorId =
            operatorId;
    }


    // ============================================
    // Header
    // ============================================

    public string Title =>
        "バックアップ・復元";

    public string Description =>
        "設備点検データベースのバックアップ作成と復元を行います。";


    // ============================================
    // State
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(CanOperate))]
    private bool isBusy;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasMessage))]
    private string? operationMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    private bool isRestoreConfirmDialogOpen;

    [ObservableProperty]
    private string selectedRestoreFileName =
        string.Empty;

    public bool CanOperate =>
        !IsBusy;

    public bool HasMessage =>
        !string.IsNullOrWhiteSpace(
            OperationMessage);

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    // ============================================
    // Backup
    // ============================================

    [RelayCommand]
    private async Task BackupAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            ErrorMessage =
                null;

            OperationMessage =
                null;

            var suggestedFileName =
                _databaseBackupService
                    .CreateSuggestedBackupFileName();

            var destinationFile =
                await _filePickerService
                    .PickBackupDestinationAsync(
                        suggestedFileName);

            if (destinationFile is null)
            {
                return;
            }

            IsBusy =
                true;

            await using var stream =
                await destinationFile
                    .OpenWriteAsync();

            await _databaseBackupService
                .BackupToAsync(
                    stream);

            OperationMessage =
                $"バックアップを作成しました。" +
                Environment.NewLine +
                destinationFile.Name;

            await WriteAuditLogSafelyAsync(
                AuditActionType.Backup,
                beforeValue: null,
                afterValue:
                    destinationFile.Name,
                reason:
                    "データベースバックアップ作成");
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "バックアップを作成できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsBusy =
                false;
        }
    }


    // ============================================
    // Select Restore File
    // ============================================

    [RelayCommand]
    private async Task SelectRestoreFileAsync()
    {
        if (IsBusy)
        {
            return;
        }

        try
        {
            ErrorMessage =
                null;

            OperationMessage =
                null;

            var file =
                await _filePickerService
                    .PickRestoreSourceAsync();

            if (file is null)
            {
                return;
            }

            _selectedRestoreFile =
                file;

            SelectedRestoreFileName =
                file.Name;

            IsRestoreConfirmDialogOpen =
                true;
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "バックアップファイルを選択できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
    }


    // ============================================
    // Cancel Restore
    // ============================================

    [RelayCommand]
    private void CancelRestore()
    {
        IsRestoreConfirmDialogOpen =
            false;

        _selectedRestoreFile =
            null;

        SelectedRestoreFileName =
            string.Empty;
    }


    // ============================================
    // Restore
    // ============================================

    [RelayCommand]
    private async Task ConfirmRestoreAsync()
    {
        if (IsBusy ||
            _selectedRestoreFile is null)
        {
            return;
        }

        var restoreFile =
            _selectedRestoreFile;

        try
        {
            IsBusy =
                true;

            ErrorMessage =
                null;

            OperationMessage =
                null;

            await using var stream =
                await restoreFile
                    .OpenReadAsync();

            var result =
                await _databaseBackupService
                    .RestoreFromAsync(
                        stream);

            IsRestoreConfirmDialogOpen =
                false;

            OperationMessage =
                "データベースを復元しました。";

            /*
             * Restoreログは復元後のDBへ記録する。
             */
            await WriteAuditLogSafelyAsync(
                AuditActionType.Restore,
                beforeValue:
                    "CurrentDatabase",
                afterValue:
                    restoreFile.Name,
                reason:
                    $"復元前退避DB: " +
                    $"{result.SafetyBackupPath}");

            _selectedRestoreFile =
                null;

            SelectedRestoreFileName =
                string.Empty;

            /*
             * Repository/ViewModelが保持している
             * 表示データを全て作り直す。
             */
            RestoreCompleted?.Invoke();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "データベースを復元できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsBusy =
                false;
        }
    }


    // ============================================
    // Audit Log
    // ============================================

    private async Task WriteAuditLogSafelyAsync(
        AuditActionType actionType,
        string? beforeValue,
        string? afterValue,
        string? reason)
    {
        try
        {
            await _auditLogRepository
                .AddAsync(
                    _operatorId,
                    actionType,
                    AuditEntityType.Database,
                    DatabaseAuditEntityId,
                    beforeValue,
                    afterValue,
                    reason);
        }
        catch (Exception exception)
        {
            /*
             * バックアップ/復元そのものが成功している場合、
             * AuditLog失敗によって成功扱いを覆さない。
             */
            OperationMessage =
                (OperationMessage ?? string.Empty) +
                Environment.NewLine +
                "※操作履歴を記録できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
    }
}