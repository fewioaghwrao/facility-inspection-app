using FacilityInspection.Services.Backup;
using Microsoft.Data.Sqlite;
using System.Text;
using Xunit;

namespace FacilityInspection.Tests.Services.Backup;

public sealed class DatabaseBackupServiceTests
    : IDisposable
{
    private readonly string _testDirectory;

    private readonly string _databasePath;


    public DatabaseBackupServiceTests()
    {
        _testDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "FacilityInspection.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            _testDirectory);

        _databasePath =
            Path.Combine(
                _testDirectory,
                "facility-inspection.db");
    }


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithValidDatabasePath_Succeeds()
    {
        // Act
        var exception =
            Record.Exception(
                () => new DatabaseBackupService(
                    _databasePath));

        // Assert
        Assert.Null(
            exception);
    }


    [Fact]
    public void Constructor_WithNullDatabasePath_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () => new DatabaseBackupService(
                    null!));

        // Assert
        Assert.Equal(
            "databasePath",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public void Constructor_WithEmptyOrWhitespaceDatabasePath_ThrowsArgumentException(
        string databasePath)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () => new DatabaseBackupService(
                    databasePath));

        // Assert
        Assert.Equal(
            "databasePath",
            exception.ParamName);
    }


    // ============================================
    // Suggested backup file name
    // ============================================

    [Fact]
    public void CreateSuggestedBackupFileName_ReturnsExpectedFormat()
    {
        // Arrange
        var service =
            new DatabaseBackupService(
                _databasePath);

        // Act
        var fileName =
            service.CreateSuggestedBackupFileName();

        // Assert
        Assert.Matches(
            @"^facility-inspection_\d{8}_\d{6}\.db$",
            fileName);
    }


    // ============================================
    // Backup
    // ============================================

    [Fact]
    public async Task BackupToAsync_WithValidDatabase_CreatesValidBackup()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "original-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var destinationStream =
            new MemoryStream();

        // Act
        await service.BackupToAsync(
            destinationStream);

        // Assert
        Assert.True(
            destinationStream.Length > 0);

        var backupPath =
            Path.Combine(
                _testDirectory,
                "backup.db");

        await SaveStreamToFileAsync(
            destinationStream,
            backupPath);

        var value =
            await ReadTestValueAsync(
                backupPath);

        Assert.Equal(
            "original-data",
            value);
    }


    [Fact]
    public async Task BackupToAsync_CreatesSQLiteDatabaseFile()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "original-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var destinationStream =
            new MemoryStream();

        // Act
        await service.BackupToAsync(
            destinationStream);

        // Assert
        destinationStream.Position =
            0;

        var headerBytes =
            new byte[16];

        var readCount =
            await destinationStream.ReadAsync(
                headerBytes);

        var header =
            Encoding.ASCII.GetString(
                headerBytes,
                0,
                readCount);

        Assert.Equal(
            "SQLite format 3\0",
            header);
    }


    [Fact]
    public async Task BackupToAsync_WhenDestinationContainsData_ReplacesExistingContent()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "backup-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var destinationStream =
            new MemoryStream();

        var oldData =
            new byte[100_000];

        await destinationStream.WriteAsync(
            oldData);

        destinationStream.Position =
            50_000;

        // Act
        await service.BackupToAsync(
            destinationStream);

        // Assert
        var backupPath =
            Path.Combine(
                _testDirectory,
                "replaced-backup.db");

        await SaveStreamToFileAsync(
            destinationStream,
            backupPath);

        var value =
            await ReadTestValueAsync(
                backupPath);

        Assert.Equal(
            "backup-data",
            value);
    }


    [Fact]
    public async Task BackupToAsync_WithNullDestinationStream_ThrowsArgumentNullException()
    {
        // Arrange
        var service =
            new DatabaseBackupService(
                _databasePath);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                ArgumentNullException>(
                () => service.BackupToAsync(
                    null!));

        // Assert
        Assert.Equal(
            "destinationStream",
            exception.ParamName);
    }


    [Fact]
    public async Task BackupToAsync_WithNonWritableStream_ThrowsInvalidOperationException()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "original-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var destinationStream =
            new MemoryStream(
                new byte[10],
                writable:
                    false);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.BackupToAsync(
                    destinationStream));

        // Assert
        Assert.Equal(
            "バックアップ先へ書き込みできません。",
            exception.Message);
    }


    // ============================================
    // Restore
    // ============================================

    [Fact]
    public async Task RestoreFromAsync_WithValidBackup_RestoresDatabase()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var backupPath =
            Path.Combine(
                _testDirectory,
                "restore-source.db");

        await CreateCompatibleDatabaseAsync(
            backupPath,
            "backup-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new FileStream(
                backupPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        // Act
        var result =
            await service.RestoreFromAsync(
                backupStream);

        // Assert
        var restoredValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "backup-data",
            restoredValue);

        Assert.False(
            string.IsNullOrWhiteSpace(
                result.SafetyBackupPath));
    }


    [Fact]
    public async Task RestoreFromAsync_WithValidBackup_CreatesSafetyBackup()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "before-restore");

        var backupPath =
            Path.Combine(
                _testDirectory,
                "restore-source.db");

        await CreateCompatibleDatabaseAsync(
            backupPath,
            "restored-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new FileStream(
                backupPath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        // Act
        var result =
            await service.RestoreFromAsync(
                backupStream);

        // Assert
        Assert.True(
            File.Exists(
                result.SafetyBackupPath));

        var safetyValue =
            await ReadTestValueAsync(
                result.SafetyBackupPath);

        Assert.Equal(
            "before-restore",
            safetyValue);

        var restoredValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "restored-data",
            restoredValue);
    }


    [Fact]
    public async Task RestoreFromAsync_AfterBackup_RestoresOriginalDatabaseState()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "original-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new MemoryStream();

        await service.BackupToAsync(
            backupStream);

        await UpdateTestValueAsync(
            _databasePath,
            "changed-data");

        var changedValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "changed-data",
            changedValue);

        // Act
        await service.RestoreFromAsync(
            backupStream);

        // Assert
        var restoredValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "original-data",
            restoredValue);
    }


    [Fact]
    public async Task RestoreFromAsync_WhenBackupStreamPositionIsAtEnd_RestoresFromBeginning()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var backupPath =
            Path.Combine(
                _testDirectory,
                "restore-source.db");

        await CreateCompatibleDatabaseAsync(
            backupPath,
            "backup-data");

        await using var backupStream =
            new MemoryStream(
                await File.ReadAllBytesAsync(
                    backupPath));

        backupStream.Position =
            backupStream.Length;

        var service =
            new DatabaseBackupService(
                _databasePath);

        // Act
        await service.RestoreFromAsync(
            backupStream);

        // Assert
        var restoredValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "backup-data",
            restoredValue);
    }


    [Fact]
    public async Task RestoreFromAsync_WithNullBackupStream_ThrowsArgumentNullException()
    {
        // Arrange
        var service =
            new DatabaseBackupService(
                _databasePath);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                ArgumentNullException>(
                () => service.RestoreFromAsync(
                    null!));

        // Assert
        Assert.Equal(
            "backupStream",
            exception.ParamName);
    }


    [Fact]
    public async Task RestoreFromAsync_WithNonReadableStream_ThrowsInvalidOperationException()
    {
        // Arrange
        var service =
            new DatabaseBackupService(
                _databasePath);

        var streamPath =
            Path.Combine(
                _testDirectory,
                "write-only.tmp");

        await using var backupStream =
            new FileStream(
                streamPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.RestoreFromAsync(
                    backupStream));

        // Assert
        Assert.Equal(
            "バックアップファイルを読み込めません。",
            exception.Message);
    }


    // ============================================
    // Invalid backup
    // ============================================

    [Fact]
    public async Task RestoreFromAsync_WithCorruptedDatabase_ThrowsSqliteException()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        var invalidData =
            Encoding.UTF8.GetBytes(
                "This is not a SQLite database.");

        await using var backupStream =
            new MemoryStream(
                invalidData);

        // Act & Assert
        await Assert.ThrowsAsync<
            SqliteException>(
                () => service.RestoreFromAsync(
                    backupStream));
    }


    [Fact]
    public async Task RestoreFromAsync_WithCorruptedDatabase_DoesNotChangeCurrentDatabase()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var service =
            new DatabaseBackupService(
                _databasePath);

        var invalidData =
            Encoding.UTF8.GetBytes(
                "This is not a SQLite database.");

        await using var backupStream =
            new MemoryStream(
                invalidData);

        // Act
        await Assert.ThrowsAsync<
            SqliteException>(
                () => service.RestoreFromAsync(
                    backupStream));

        // Assert
        var currentValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "current-data",
            currentValue);
    }


    // ============================================
    // Wrong SQLite database
    // ============================================

    [Fact]
    public async Task RestoreFromAsync_WithNonFacilityInspectionDatabase_ThrowsInvalidOperationException()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var wrongDatabasePath =
            Path.Combine(
                _testDirectory,
                "wrong-database.db");

        await CreateUnrelatedDatabaseAsync(
            wrongDatabasePath);

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new FileStream(
                wrongDatabasePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.RestoreFromAsync(
                    backupStream));

        // Assert
        Assert.Contains(
            "必要なテーブル 'Operators' がありません。",
            exception.Message);

        Assert.Contains(
            "設備点検アプリのバックアップDBを選択してください。",
            exception.Message);
    }


    [Fact]
    public async Task RestoreFromAsync_WithNonFacilityInspectionDatabase_DoesNotChangeCurrentDatabase()
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var wrongDatabasePath =
            Path.Combine(
                _testDirectory,
                "wrong-database.db");

        await CreateUnrelatedDatabaseAsync(
            wrongDatabasePath);

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new FileStream(
                wrongDatabasePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        // Act
        await Assert.ThrowsAsync<
            InvalidOperationException>(
                () => service.RestoreFromAsync(
                    backupStream));

        // Assert
        var currentValue =
            await ReadTestValueAsync(
                _databasePath);

        Assert.Equal(
            "current-data",
            currentValue);
    }


    // ============================================
    // Required table validation
    // ============================================

    [Theory]
    [InlineData("Operators")]
    [InlineData("InspectionSchedules")]
    [InlineData("Inspections")]
    [InlineData("AuditLogs")]
    public async Task RestoreFromAsync_WhenRequiredTableIsMissing_ThrowsInvalidOperationException(
        string missingTable)
    {
        // Arrange
        await CreateCompatibleDatabaseAsync(
            _databasePath,
            "current-data");

        var incompleteDatabasePath =
            Path.Combine(
                _testDirectory,
                $"missing-{missingTable}.db");

        await CreateDatabaseMissingTableAsync(
            incompleteDatabasePath,
            missingTable);

        var service =
            new DatabaseBackupService(
                _databasePath);

        await using var backupStream =
            new FileStream(
                incompleteDatabasePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read);

        // Act
        var exception =
            await Assert.ThrowsAsync<
                InvalidOperationException>(
                () => service.RestoreFromAsync(
                    backupStream));

        // Assert
        Assert.Contains(
            $"必要なテーブル '{missingTable}' がありません。",
            exception.Message);
    }


    // ============================================
    // Helpers
    // ============================================

    private static async Task
        CreateCompatibleDatabaseAsync(
            string databasePath,
            string testValue)
    {
        DeleteDatabaseFiles(
            databasePath);

        var directory =
            Path.GetDirectoryName(
                databasePath);

        if (!string.IsNullOrWhiteSpace(
                directory))
        {
            Directory.CreateDirectory(
                directory);
        }

        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate,

                Pooling =
                    false
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            CREATE TABLE Operators
            (
                Id TEXT PRIMARY KEY
            );

            CREATE TABLE InspectionSchedules
            (
                Id TEXT PRIMARY KEY
            );

            CREATE TABLE Inspections
            (
                Id TEXT PRIMARY KEY
            );

            CREATE TABLE AuditLogs
            (
                Id TEXT PRIMARY KEY
            );

            CREATE TABLE TestData
            (
                Id INTEGER PRIMARY KEY,
                Value TEXT NOT NULL
            );

            INSERT INTO TestData
                (Id, Value)
            VALUES
                (1, $value);
            """;

        command.Parameters.AddWithValue(
            "$value",
            testValue);

        await command.ExecuteNonQueryAsync();
    }


    private static async Task
        CreateUnrelatedDatabaseAsync(
            string databasePath)
    {
        DeleteDatabaseFiles(
            databasePath);

        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate,

                Pooling =
                    false
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            CREATE TABLE UnrelatedData
            (
                Id INTEGER PRIMARY KEY,
                Value TEXT
            );

            INSERT INTO UnrelatedData
                (Value)
            VALUES
                ('other application');
            """;

        await command.ExecuteNonQueryAsync();
    }


    private static async Task
        CreateDatabaseMissingTableAsync(
            string databasePath,
            string missingTable)
    {
        DeleteDatabaseFiles(
            databasePath);

        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadWriteCreate,

                Pooling =
                    false
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync();

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
            if (string.Equals(
                    tableName,
                    missingTable,
                    StringComparison.Ordinal))
            {
                continue;
            }

            await using var command =
                connection.CreateCommand();

            command.CommandText =
                $"""
                 CREATE TABLE "{tableName}"
                 (
                     Id TEXT PRIMARY KEY
                 );
                 """;

            await command.ExecuteNonQueryAsync();
        }
    }


    private static async Task<string>
        ReadTestValueAsync(
            string databasePath)
    {
        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadOnly,

                Pooling =
                    false
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            SELECT Value
            FROM TestData
            WHERE Id = 1;
            """;

        var result =
            await command.ExecuteScalarAsync();

        return Convert.ToString(
                   result)
               ?? string.Empty;
    }


    private static async Task
        UpdateTestValueAsync(
            string databasePath,
            string newValue)
    {
        var connectionString =
            new SqliteConnectionStringBuilder
            {
                DataSource =
                    databasePath,

                Mode =
                    SqliteOpenMode.ReadWrite,

                Pooling =
                    false
            }
            .ToString();

        await using var connection =
            new SqliteConnection(
                connectionString);

        await connection.OpenAsync();

        await using var command =
            connection.CreateCommand();

        command.CommandText =
            """
            UPDATE TestData
            SET Value = $value
            WHERE Id = 1;
            """;

        command.Parameters.AddWithValue(
            "$value",
            newValue);

        await command.ExecuteNonQueryAsync();
    }


    private static async Task
        SaveStreamToFileAsync(
            Stream stream,
            string destinationPath)
    {
        if (stream.CanSeek)
        {
            stream.Position =
                0;
        }

        await using var fileStream =
            new FileStream(
                destinationPath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);

        await stream.CopyToAsync(
            fileStream);

        await fileStream.FlushAsync();
    }


    private static void DeleteDatabaseFiles(
        string databasePath)
    {
        DeleteFileIfExists(
            databasePath);

        DeleteFileIfExists(
            databasePath +
            "-wal");

        DeleteFileIfExists(
            databasePath +
            "-shm");
    }


    private static void DeleteFileIfExists(
        string path)
    {
        if (File.Exists(
                path))
        {
            File.Delete(
                path);
        }
    }


    // ============================================
    // Cleanup
    // ============================================

    public void Dispose()
    {
        GC.SuppressFinalize(
            this);

        try
        {
            if (Directory.Exists(
                    _testDirectory))
            {
                Directory.Delete(
                    _testDirectory,
                    recursive:
                        true);
            }
        }
        catch
        {
            // テスト終了後の一時ファイル削除失敗は
            // テスト結果へ影響させない。
        }
    }
}