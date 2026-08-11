using Microsoft.Data.Sqlite;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Services.Backup;

public sealed record DatabaseRestoreResult(
    string SafetyBackupPath);

public sealed class DatabaseBackupService
{
    private readonly string
        _databasePath;

    public DatabaseBackupService(
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            databasePath);

        _databasePath =
            databasePath;
    }


    // ============================================
    // Backup File Name
    // ============================================

    public string CreateSuggestedBackupFileName()
    {
        return
            $"facility-inspection_" +
            $"{DateTime.Now:yyyyMMdd_HHmmss}.db";
    }


    // ============================================
    // Backup
    // ============================================

    public async Task BackupToAsync(
        Stream destinationStream,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            destinationStream);

        if (!destinationStream.CanWrite)
        {
            throw new InvalidOperationException(
                "バックアップ先へ書き込みできません。");
        }

        var temporaryBackupPath =
            Path.Combine(
                Path.GetTempPath(),
                $"facility-inspection-backup-" +
                $"{Guid.NewGuid():N}.db");

        try
        {
            /*
             * 現在DB
             *     ↓
             * SQLite Backup API
             *     ↓
             * 一時DB
             */
            await CreateSqliteBackupAsync(
                _databasePath,
                temporaryBackupPath,
                cancellationToken);

            /*
             * 一時DB
             *     ↓
             * StorageProviderで選択されたファイル
             */
            await using var sourceStream =
                new FileStream(
                    temporaryBackupPath,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.Read);

            if (destinationStream.CanSeek)
            {
                destinationStream.Position =
                    0;

                destinationStream.SetLength(
                    0);
            }

            await sourceStream.CopyToAsync(
                destinationStream,
                cancellationToken);

            await destinationStream.FlushAsync(
                cancellationToken);
        }
        finally
        {
            DeleteFileIfExists(
                temporaryBackupPath);
        }
    }


    // ============================================
    // Restore
    // ============================================

    public async Task<DatabaseRestoreResult>
        RestoreFromAsync(
            Stream backupStream,
            CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            backupStream);

        if (!backupStream.CanRead)
        {
            throw new InvalidOperationException(
                "バックアップファイルを読み込めません。");
        }

        var stagingPath =
            Path.Combine(
                Path.GetTempPath(),
                $"facility-inspection-restore-" +
                $"{Guid.NewGuid():N}.db");

        var databaseDirectory =
            Path.GetDirectoryName(
                _databasePath);

        if (string.IsNullOrWhiteSpace(
                databaseDirectory))
        {
            throw new InvalidOperationException(
                "データベース保存先を取得できません。");
        }

        var safetyDirectory =
            Path.Combine(
                databaseDirectory,
                "restore-safety");

        Directory.CreateDirectory(
            safetyDirectory);

        var safetyBackupPath =
            Path.Combine(
                safetyDirectory,
                $"facility-inspection_before_restore_" +
                $"{DateTime.Now:yyyyMMdd_HHmmss}.db");

        try
        {
            // ----------------------------------------
            // 選択されたバックアップDBを一時保存
            // ----------------------------------------

            await using (
                var stagingStream =
                    new FileStream(
                        stagingPath,
                        FileMode.Create,
                        FileAccess.Write,
                        FileShare.None))
            {
                if (backupStream.CanSeek)
                {
                    backupStream.Position =
                        0;
                }

                await backupStream.CopyToAsync(
                    stagingStream,
                    cancellationToken);
            }


            // ----------------------------------------
            // バックアップDB検証
            // ----------------------------------------

            await ValidateDatabaseAsync(
                stagingPath,
                cancellationToken);


            // ----------------------------------------
            // 現在DBを退避
            // ----------------------------------------

            await CreateSqliteBackupAsync(
                _databasePath,
                safetyBackupPath,
                cancellationToken);


            // ----------------------------------------
            // 復元
            // ----------------------------------------

            try
            {
                await RestoreDatabaseAsync(
                    stagingPath,
                    _databasePath,
                    cancellationToken);

                /*
                 * 復元後にも整合性チェック。
                 */
                await ValidateDatabaseAsync(
                    _databasePath,
                    cancellationToken);
            }
            catch
            {
                /*
                 * 復元に失敗した場合、
                 * 自動退避したDBから元に戻す。
                 */
                try
                {
                    await RestoreDatabaseAsync(
                        safetyBackupPath,
                        _databasePath,
                        CancellationToken.None);
                }
                catch
                {
                    /*
                     * 元に戻す処理まで失敗した場合は、
                     * 最初の例外を優先する。
                     *
                     * safetyBackupPath は残るため、
                     * 手動復旧可能。
                     */
                }

                throw;
            }


            return new DatabaseRestoreResult(
                safetyBackupPath);
        }
        finally
        {
            DeleteFileIfExists(
                stagingPath);
        }
    }


    // ============================================
    // SQLite Backup
    // ============================================

    private static async Task
        CreateSqliteBackupAsync(
            string sourcePath,
            string destinationPath,
            CancellationToken cancellationToken)
    {
        var destinationDirectory =
            Path.GetDirectoryName(
                destinationPath);

        if (!string.IsNullOrWhiteSpace(
                destinationDirectory))
        {
            Directory.CreateDirectory(
                destinationDirectory);
        }

        DeleteFileIfExists(
            destinationPath);

        var sourceConnectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    sourcePath,

                Mode =
                    SqliteOpenMode.ReadOnly
            }
            .ToString();

        var destinationConnectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    destinationPath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate
            }
            .ToString();

        await using var sourceConnection =
            new SqliteConnection(
                sourceConnectionString);

        await using var destinationConnection =
            new SqliteConnection(
                destinationConnectionString);

        await sourceConnection.OpenAsync(
            cancellationToken);

        await destinationConnection.OpenAsync(
            cancellationToken);

        sourceConnection.BackupDatabase(
            destinationConnection);
    }


    // ============================================
    // SQLite Restore
    // ============================================

    private static async Task
        RestoreDatabaseAsync(
            string sourceBackupPath,
            string destinationDatabasePath,
            CancellationToken cancellationToken)
    {
        var sourceConnectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    sourceBackupPath,

                Mode =
                    SqliteOpenMode.ReadOnly
            }
            .ToString();

        var destinationConnectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    destinationDatabasePath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate
            }
            .ToString();

        await using var sourceConnection =
            new SqliteConnection(
                sourceConnectionString);

        await using var destinationConnection =
            new SqliteConnection(
                destinationConnectionString);

        await sourceConnection.OpenAsync(
            cancellationToken);

        await destinationConnection.OpenAsync(
            cancellationToken);

        /*
         * バックアップDBの内容で
         * 現在DBを置き換える。
         */
        sourceConnection.BackupDatabase(
            destinationConnection);
    }


    // ============================================
    // Validation
    // ============================================

    private static async Task
        ValidateDatabaseAsync(
            string databasePath,
            CancellationToken cancellationToken)
    {
        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadOnly
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync(
            cancellationToken);


        // ----------------------------------------
        // SQLite整合性チェック
        // ----------------------------------------

        await using (
            var command =
                connection.CreateCommand())
        {
            command.CommandText =
                "PRAGMA integrity_check;";

            var result =
                await command.ExecuteScalarAsync(
                    cancellationToken);

            if (!string.Equals(
                    Convert.ToString(result),
                    "ok",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "選択されたバックアップDBに" +
                    "整合性エラーがあります。");
            }
        }


        // ----------------------------------------
        // FacilityInspection DBか確認
        // ----------------------------------------

        string[] requiredTables =
        [
            "Operators",
            "InspectionSchedules",
            "Inspections",
            "AuditLogs"
        ];

        foreach (var tableName in
                 requiredTables)
        {
            await using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $tableName;
                """;

            command.Parameters.AddWithValue(
                "$tableName",
                tableName);

            var count =
                Convert.ToInt32(
                    await command.ExecuteScalarAsync(
                        cancellationToken));

            if (count == 0)
            {
                throw new InvalidOperationException(
                    $"必要なテーブル " +
                    $"'{tableName}' がありません。" +
                    Environment.NewLine +
                    "設備点検アプリのバックアップDBを" +
                    "選択してください。");
            }
        }
    }


    // ============================================
    // Delete
    // ============================================

    private static void DeleteFileIfExists(
        string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}