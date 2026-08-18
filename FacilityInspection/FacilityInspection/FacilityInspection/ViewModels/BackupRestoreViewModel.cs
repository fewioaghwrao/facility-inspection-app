using Avalonia.Platform.Storage;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Services.Backup;
using System;
using System.IO;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;


// ============================================
// Backup / Restore File
//
// IStorageFileをViewModel内部で直接保持せず、
// テスト可能な最小構造へ変換する。
// ============================================

internal sealed record BackupRestoreFile(
    string Name,
    Func<Task<Stream>> OpenStreamAsync);


// ============================================
// Backup / Restore ViewModel
// ============================================

public sealed partial class BackupRestoreViewModel
    : ViewModelBase
{
    private static readonly Guid
        DatabaseAuditEntityId =
            Guid.Parse(
                "B63D80E2-E10A-4E40-8B93-4A3E2C887001");


    private readonly Guid
        _operatorId;


    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<string>
        _createSuggestedBackupFileName;

    private readonly Func<
        string,
        Task<BackupRestoreFile?>>
        _pickBackupDestinationAsync;

    private readonly Func<
        Task<BackupRestoreFile?>>
        _pickRestoreSourceAsync;

    private readonly Func<
        Stream,
        Task>
        _backupToAsync;

    private readonly Func<
        Stream,
        Task<DatabaseRestoreResult>>
        _restoreFromAsync;

    private readonly Func<
        Guid,
        AuditActionType,
        AuditEntityType,
        Guid,
        string?,
        string?,
        string?,
        Task>
        _writeAuditLogAsync;


    private BackupRestoreFile?
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
    // 本番用
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

        _operatorId =
            operatorId;


        // ----------------------------------------
        // Backup Service
        // ----------------------------------------

        _createSuggestedBackupFileName =
            () =>
                databaseBackupService
                    .CreateSuggestedBackupFileName();


        _backupToAsync =
            stream =>
                databaseBackupService
                    .BackupToAsync(
                        stream);


        _restoreFromAsync =
            stream =>
                databaseBackupService
                    .RestoreFromAsync(
                        stream);


        // ----------------------------------------
        // File Picker
        // ----------------------------------------

        _pickBackupDestinationAsync =
            async suggestedFileName =>
            {
                var file =
                    await filePickerService
                        .PickBackupDestinationAsync(
                            suggestedFileName);

                if (file is null)
                {
                    return null;
                }

                return new BackupRestoreFile(
                    file.Name,
                    async () =>
                        await file
                            .OpenWriteAsync());
            };


        _pickRestoreSourceAsync =
            async () =>
            {
                var file =
                    await filePickerService
                        .PickRestoreSourceAsync();

                if (file is null)
                {
                    return null;
                }

                return new BackupRestoreFile(
                    file.Name,
                    async () =>
                        await file
                            .OpenReadAsync());
            };


        // ----------------------------------------
        // Audit Log
        // ----------------------------------------

        _writeAuditLogAsync =
            (
                currentOperatorId,
                actionType,
                entityType,
                entityId,
                beforeValue,
                afterValue,
                reason) =>
                auditLogRepository
                    .AddAsync(
                        currentOperatorId,
                        actionType,
                        entityType,
                        entityId,
                        beforeValue,
                        afterValue,
                        reason);
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal BackupRestoreViewModel(
        Guid operatorId,
        Func<string>
            createSuggestedBackupFileName,
        Func<
            string,
            Task<BackupRestoreFile?>>
            pickBackupDestinationAsync,
        Func<
            Task<BackupRestoreFile?>>
            pickRestoreSourceAsync,
        Func<
            Stream,
            Task>
            backupToAsync,
        Func<
            Stream,
            Task<DatabaseRestoreResult>>
            restoreFromAsync,
        Func<
            Guid,
            AuditActionType,
            AuditEntityType,
            Guid,
            string?,
            string?,
            string?,
            Task>
            writeAuditLogAsync)
    {
        if (operatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "操作担当者IDを指定してください。",
                nameof(operatorId));
        }

        ArgumentNullException.ThrowIfNull(
            createSuggestedBackupFileName);

        ArgumentNullException.ThrowIfNull(
            pickBackupDestinationAsync);

        ArgumentNullException.ThrowIfNull(
            pickRestoreSourceAsync);

        ArgumentNullException.ThrowIfNull(
            backupToAsync);

        ArgumentNullException.ThrowIfNull(
            restoreFromAsync);

        ArgumentNullException.ThrowIfNull(
            writeAuditLogAsync);


        _operatorId =
            operatorId;

        _createSuggestedBackupFileName =
            createSuggestedBackupFileName;

        _pickBackupDestinationAsync =
            pickBackupDestinationAsync;

        _pickRestoreSourceAsync =
            pickRestoreSourceAsync;

        _backupToAsync =
            backupToAsync;

        _restoreFromAsync =
            restoreFromAsync;

        _writeAuditLogAsync =
            writeAuditLogAsync;
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
                _createSuggestedBackupFileName();


            var destinationFile =
                await _pickBackupDestinationAsync(
                    suggestedFileName);


            if (destinationFile is null)
            {
                return;
            }


            IsBusy =
                true;


            await using var stream =
                await destinationFile
                    .OpenStreamAsync();


            await _backupToAsync(
                stream);


            OperationMessage =
                "バックアップを作成しました。" +
                Environment.NewLine +
                destinationFile.Name;


            await WriteAuditLogSafelyAsync(
                AuditActionType.Backup,
                beforeValue:
                    null,
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
                await _pickRestoreSourceAsync();


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
                    .OpenStreamAsync();


            var result =
                await _restoreFromAsync(
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
                    "復元前退避DB: " +
                    result.SafetyBackupPath);


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
            await _writeAuditLogAsync(
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
                (OperationMessage ??
                 string.Empty) +
                Environment.NewLine +
                "※操作履歴を記録できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
    }
}