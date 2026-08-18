using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Services.Backup;
using FacilityInspection.ViewModels;
using System;
using System.IO;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class BackupRestoreViewModelTests
{
    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

    private static readonly Guid
        ExpectedDatabaseAuditEntityId =
            Guid.Parse(
                "B63D80E2-E10A-4E40-8B93-4A3E2C887001");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    CreateViewModel(
                        operatorId:
                            Guid.Empty));

        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullBackupAction_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new BackupRestoreViewModel(
                        OperatorId,
                        () =>
                            "backup.db",
                        _ =>
                            Task.FromResult<
                                BackupRestoreFile?>(
                                null),
                        () =>
                            Task.FromResult<
                                BackupRestoreFile?>(
                                null),
                        null!,
                        _ =>
                            Task.FromResult(
                                new DatabaseRestoreResult(
                                    "safety.db")),
                        (
                            _,
                            _,
                            _,
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask));

        // Assert
        Assert.Equal(
            "backupToAsync",
            exception.ParamName);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_SetsInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel();

        // Assert
        Assert.Equal(
            "バックアップ・復元",
            sut.Title);

        Assert.Equal(
            "設備点検データベースのバックアップ作成と復元を行います。",
            sut.Description);

        Assert.False(
            sut.IsBusy);

        Assert.True(
            sut.CanOperate);

        Assert.Null(
            sut.OperationMessage);

        Assert.False(
            sut.HasMessage);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.SelectedRestoreFileName);
    }


    // ============================================
    // Backup - Busy
    // ============================================

    [Fact]
    public async Task BackupCommand_WhenBusy_DoesNothing()
    {
        // Arrange
        var pickerCallCount =
            0;

        var backupCallCount =
            0;

        var sut =
            CreateViewModel(
                pickBackupDestinationAsync:
                    _ =>
                    {
                        pickerCallCount++;

                        return Task.FromResult<
                            BackupRestoreFile?>(
                            null);
                    },
                backupToAsync:
                    _ =>
                    {
                        backupCallCount++;

                        return Task.CompletedTask;
                    });

        sut.IsBusy =
            true;

        // Act
        await sut.BackupCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            pickerCallCount);

        Assert.Equal(
            0,
            backupCallCount);
    }


    // ============================================
    // Backup - Cancel
    // ============================================

    [Fact]
    public async Task BackupCommand_WhenFileSelectionIsCancelled_DoesNothing()
    {
        // Arrange
        var backupCallCount =
            0;

        var sut =
            CreateViewModel(
                pickBackupDestinationAsync:
                    _ =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            null),
                backupToAsync:
                    _ =>
                    {
                        backupCallCount++;

                        return Task.CompletedTask;
                    });

        // Act
        await sut.BackupCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            backupCallCount);

        Assert.Null(
            sut.OperationMessage);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Backup - Success
    // ============================================

    [Fact]
    public async Task BackupCommand_WhenSuccessful_CreatesBackupAndAuditLog()
    {
        // Arrange
        var stream =
            new MemoryStream();

        string? receivedSuggestedFileName =
            null;

        AuditLogCall? auditLog =
            null;

        var sut =
            CreateViewModel(
                createSuggestedBackupFileName:
                    () =>
                        "facility-inspection_test.db",

                pickBackupDestinationAsync:
                    suggestedFileName =>
                    {
                        receivedSuggestedFileName =
                            suggestedFileName;

                        return Task.FromResult<
                            BackupRestoreFile?>(
                            new BackupRestoreFile(
                                "selected-backup.db",
                                () =>
                                    Task.FromResult<
                                        Stream>(
                                        stream)));
                    },

                backupToAsync:
                    async destination =>
                    {
                        await destination.WriteAsync(
                            new byte[]
                            {
                                1,
                                2,
                                3
                            });
                    },

                writeAuditLogAsync:
                    (
                        operatorId,
                        actionType,
                        entityType,
                        entityId,
                        beforeValue,
                        afterValue,
                        reason) =>
                    {
                        auditLog =
                            new AuditLogCall(
                                operatorId,
                                actionType,
                                entityType,
                                entityId,
                                beforeValue,
                                afterValue,
                                reason);

                        return Task.CompletedTask;
                    });

        // Act
        await sut.BackupCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            "facility-inspection_test.db",
            receivedSuggestedFileName);

        Assert.Equal(
            new byte[]
            {
                1,
                2,
                3
            },
            stream.ToArray());

        Assert.Equal(
            "バックアップを作成しました。" +
            Environment.NewLine +
            "selected-backup.db",
            sut.OperationMessage);

        Assert.True(
            sut.HasMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsBusy);

        Assert.True(
            sut.CanOperate);


        Assert.NotNull(
            auditLog);

        Assert.Equal(
            OperatorId,
            auditLog.OperatorId);

        Assert.Equal(
            AuditActionType.Backup,
            auditLog.ActionType);

        Assert.Equal(
            AuditEntityType.Database,
            auditLog.EntityType);

        Assert.Equal(
            ExpectedDatabaseAuditEntityId,
            auditLog.EntityId);

        Assert.Null(
            auditLog.BeforeValue);

        Assert.Equal(
            "selected-backup.db",
            auditLog.AfterValue);

        Assert.Equal(
            "データベースバックアップ作成",
            auditLog.Reason);
    }


    // ============================================
    // Backup - Loading State
    // ============================================

    [Fact]
    public async Task BackupCommand_WhileBackupIsRunning_SetsBusy()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource();

        var sut =
            CreateViewModel(
                pickBackupDestinationAsync:
                    _ =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateBackupFile(
                                "backup.db")),

                backupToAsync:
                    async _ =>
                        await completionSource.Task);

        // Act
        var task =
            sut.BackupCommand
                .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.IsBusy);

        Assert.False(
            sut.CanOperate);


        completionSource.SetResult();

        await task;


        Assert.False(
            sut.IsBusy);

        Assert.True(
            sut.CanOperate);
    }


    // ============================================
    // Backup - Failure
    // ============================================

    [Fact]
    public async Task BackupCommand_WhenBackupFails_SetsError()
    {
        // Arrange
        var auditCallCount =
            0;

        var sut =
            CreateViewModel(
                pickBackupDestinationAsync:
                    _ =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateBackupFile(
                                "backup.db")),

                backupToAsync:
                    _ =>
                        throw new InvalidOperationException(
                            "バックアップテストエラー"),

                writeAuditLogAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) =>
                    {
                        auditCallCount++;

                        return Task.CompletedTask;
                    });

        // Act
        await sut.BackupCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "バックアップを作成できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "バックアップテストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.HasMessage);

        Assert.Equal(
            0,
            auditCallCount);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Backup - Audit Failure
    // ============================================

    [Fact]
    public async Task BackupCommand_WhenAuditLogFails_KeepsBackupSuccessful()
    {
        // Arrange
        var sut =
            CreateViewModel(
                pickBackupDestinationAsync:
                    _ =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateBackupFile(
                                "backup.db")),

                writeAuditLogAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) =>
                        throw new InvalidOperationException(
                            "監査ログテストエラー"));

        // Act
        await sut.BackupCommand
            .ExecuteAsync(null);

        // Assert
        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.True(
            sut.HasMessage);

        Assert.NotNull(
            sut.OperationMessage);

        Assert.Contains(
            "バックアップを作成しました。",
            sut.OperationMessage);

        Assert.Contains(
            "※操作履歴を記録できませんでした。",
            sut.OperationMessage);

        Assert.Contains(
            "監査ログテストエラー",
            sut.OperationMessage);
    }


    // ============================================
    // Restore File Selection
    // ============================================

    [Fact]
    public async Task SelectRestoreFileCommand_WhenFileSelected_OpensDialog()
    {
        // Arrange
        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateRestoreFile(
                                "restore.db")));

        // Act
        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            "restore.db",
            sut.SelectedRestoreFileName);

        Assert.True(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Null(
            sut.OperationMessage);
    }


    [Fact]
    public async Task SelectRestoreFileCommand_WhenSelectionCancelled_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            null));

        // Act
        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);

        // Assert
        Assert.False(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.SelectedRestoreFileName);

        Assert.False(
            sut.HasError);
    }


    [Fact]
    public async Task SelectRestoreFileCommand_WhenPickerFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        throw new InvalidOperationException(
                            "選択テストエラー"));

        // Act
        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "バックアップファイルを選択できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "選択テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsRestoreConfirmDialogOpen);
    }


    // ============================================
    // Cancel Restore
    // ============================================

    [Fact]
    public async Task CancelRestoreCommand_ClearsSelectedRestoreFile()
    {
        // Arrange
        var restoreCallCount =
            0;

        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateRestoreFile(
                                "restore.db")),

                restoreFromAsync:
                    _ =>
                    {
                        restoreCallCount++;

                        return Task.FromResult(
                            new DatabaseRestoreResult(
                                "safety.db"));
                    });

        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);

        Assert.True(
            sut.IsRestoreConfirmDialogOpen);

        // Act
        sut.CancelRestoreCommand
            .Execute(null);

        // Assert
        Assert.False(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.SelectedRestoreFileName);


        /*
         * 選択情報そのものも破棄されたか確認。
         */
        await sut.ConfirmRestoreCommand
            .ExecuteAsync(null);

        Assert.Equal(
            0,
            restoreCallCount);
    }


    // ============================================
    // Restore - No File
    // ============================================

    [Fact]
    public async Task ConfirmRestoreCommand_WithoutSelectedFile_DoesNothing()
    {
        // Arrange
        var restoreCallCount =
            0;

        var sut =
            CreateViewModel(
                restoreFromAsync:
                    _ =>
                    {
                        restoreCallCount++;

                        return Task.FromResult(
                            new DatabaseRestoreResult(
                                "safety.db"));
                    });

        // Act
        await sut.ConfirmRestoreCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            restoreCallCount);

        Assert.Null(
            sut.OperationMessage);

        Assert.Null(
            sut.ErrorMessage);
    }


    // ============================================
    // Restore - Success
    // ============================================

    [Fact]
    public async Task ConfirmRestoreCommand_WhenSuccessful_RestoresAndRequestsRefresh()
    {
        // Arrange
        var restoreStream =
            new MemoryStream(
                new byte[]
                {
                    10,
                    20,
                    30
                });

        var receivedBytes =
            Array.Empty<byte>();

        AuditLogCall? auditLog =
            null;

        var restoreCompletedCallCount =
            0;


        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            new BackupRestoreFile(
                                "restore.db",
                                () =>
                                    Task.FromResult<
                                        Stream>(
                                        restoreStream))),

                restoreFromAsync:
                    async source =>
                    {
                        using var copy =
                            new MemoryStream();

                        await source.CopyToAsync(
                            copy);

                        receivedBytes =
                            copy.ToArray();

                        return new DatabaseRestoreResult(
                            "C:\\data\\restore-safety\\before.db");
                    },

                writeAuditLogAsync:
                    (
                        operatorId,
                        actionType,
                        entityType,
                        entityId,
                        beforeValue,
                        afterValue,
                        reason) =>
                    {
                        auditLog =
                            new AuditLogCall(
                                operatorId,
                                actionType,
                                entityType,
                                entityId,
                                beforeValue,
                                afterValue,
                                reason);

                        return Task.CompletedTask;
                    });


        sut.RestoreCompleted =
            () =>
                restoreCompletedCallCount++;


        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);


        // Act
        await sut.ConfirmRestoreCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            new byte[]
            {
                10,
                20,
                30
            },
            receivedBytes);

        Assert.Equal(
            "データベースを復元しました。",
            sut.OperationMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsBusy);

        Assert.False(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.SelectedRestoreFileName);

        Assert.Equal(
            1,
            restoreCompletedCallCount);


        Assert.NotNull(
            auditLog);

        Assert.Equal(
            OperatorId,
            auditLog.OperatorId);

        Assert.Equal(
            AuditActionType.Restore,
            auditLog.ActionType);

        Assert.Equal(
            AuditEntityType.Database,
            auditLog.EntityType);

        Assert.Equal(
            ExpectedDatabaseAuditEntityId,
            auditLog.EntityId);

        Assert.Equal(
            "CurrentDatabase",
            auditLog.BeforeValue);

        Assert.Equal(
            "restore.db",
            auditLog.AfterValue);

        Assert.Equal(
            "復元前退避DB: " +
            "C:\\data\\restore-safety\\before.db",
            auditLog.Reason);
    }


    // ============================================
    // Restore - Failure
    // ============================================

    [Fact]
    public async Task ConfirmRestoreCommand_WhenRestoreFails_KeepsSelectionAndSetsError()
    {
        // Arrange
        var restoreCompletedCallCount =
            0;

        var auditCallCount =
            0;


        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateRestoreFile(
                                "restore.db")),

                restoreFromAsync:
                    _ =>
                        throw new InvalidOperationException(
                            "復元テストエラー"),

                writeAuditLogAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) =>
                    {
                        auditCallCount++;

                        return Task.CompletedTask;
                    });


        sut.RestoreCompleted =
            () =>
                restoreCompletedCallCount++;


        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);


        // Act
        await sut.ConfirmRestoreCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "データベースを復元できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "復元テストエラー",
            sut.ErrorMessage);

        Assert.Equal(
            0,
            auditCallCount);

        Assert.Equal(
            0,
            restoreCompletedCallCount);

        /*
         * 失敗時は再実行できるよう
         * 選択状態を保持する現行仕様。
         */
        Assert.True(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            "restore.db",
            sut.SelectedRestoreFileName);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Restore - Audit Failure
    // ============================================

    [Fact]
    public async Task ConfirmRestoreCommand_WhenAuditLogFails_KeepsRestoreSuccessful()
    {
        // Arrange
        var restoreCompletedCallCount =
            0;

        var sut =
            CreateViewModel(
                pickRestoreSourceAsync:
                    () =>
                        Task.FromResult<
                            BackupRestoreFile?>(
                            CreateRestoreFile(
                                "restore.db")),

                restoreFromAsync:
                    _ =>
                        Task.FromResult(
                            new DatabaseRestoreResult(
                                "safety.db")),

                writeAuditLogAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _,
                        _,
                        _) =>
                        throw new InvalidOperationException(
                            "監査ログ失敗"));


        sut.RestoreCompleted =
            () =>
                restoreCompletedCallCount++;


        await sut.SelectRestoreFileCommand
            .ExecuteAsync(null);


        // Act
        await sut.ConfirmRestoreCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.HasError);

        Assert.True(
            sut.HasMessage);

        Assert.NotNull(
            sut.OperationMessage);

        Assert.Contains(
            "データベースを復元しました。",
            sut.OperationMessage);

        Assert.Contains(
            "※操作履歴を記録できませんでした。",
            sut.OperationMessage);

        Assert.Contains(
            "監査ログ失敗",
            sut.OperationMessage);

        Assert.False(
            sut.IsRestoreConfirmDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.SelectedRestoreFileName);

        /*
         * AuditLogだけ失敗しても
         * RestoreCompletedは実行する。
         */
        Assert.Equal(
            1,
            restoreCompletedCallCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static BackupRestoreViewModel
        CreateViewModel(
            Guid? operatorId = null,
            Func<string>?
                createSuggestedBackupFileName = null,
            Func<
                string,
                Task<BackupRestoreFile?>>?
                pickBackupDestinationAsync = null,
            Func<
                Task<BackupRestoreFile?>>?
                pickRestoreSourceAsync = null,
            Func<
                Stream,
                Task>?
                backupToAsync = null,
            Func<
                Stream,
                Task<DatabaseRestoreResult>>?
                restoreFromAsync = null,
            Func<
                Guid,
                AuditActionType,
                AuditEntityType,
                Guid,
                string?,
                string?,
                string?,
                Task>?
                writeAuditLogAsync = null)
    {
        return new BackupRestoreViewModel(
            operatorId ??
                OperatorId,

            createSuggestedBackupFileName ??
                (() =>
                    "facility-inspection_test.db"),

            pickBackupDestinationAsync ??
                (_ =>
                    Task.FromResult<
                        BackupRestoreFile?>(
                        null)),

            pickRestoreSourceAsync ??
                (() =>
                    Task.FromResult<
                        BackupRestoreFile?>(
                        null)),

            backupToAsync ??
                (_ =>
                    Task.CompletedTask),

            restoreFromAsync ??
                (_ =>
                    Task.FromResult(
                        new DatabaseRestoreResult(
                            "safety.db"))),

            writeAuditLogAsync ??
                ((
                    _,
                    _,
                    _,
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask));
    }


    private static BackupRestoreFile
        CreateBackupFile(
            string name)
    {
        return new BackupRestoreFile(
            name,
            () =>
                Task.FromResult<
                    Stream>(
                    new MemoryStream()));
    }


    private static BackupRestoreFile
        CreateRestoreFile(
            string name)
    {
        return new BackupRestoreFile(
            name,
            () =>
                Task.FromResult<
                    Stream>(
                    new MemoryStream(
                        new byte[]
                        {
                            1,
                            2,
                            3
                        })));
    }


    // ============================================
    // Audit Capture
    // ============================================

    private sealed record AuditLogCall(
        Guid OperatorId,
        AuditActionType ActionType,
        AuditEntityType EntityType,
        Guid EntityId,
        string? BeforeValue,
        string? AfterValue,
        string? Reason);
}