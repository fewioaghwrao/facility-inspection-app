using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FacilityInspection.Tests.Data;

public sealed class DatabaseInitializerTests
    : IDisposable
{
    private readonly string _testDirectory;

    private readonly string _databasePath;


    public DatabaseInitializerTests()
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


    public void Dispose()
    {
        SqliteConnection.ClearAllPools();

        try
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(
                    _testDirectory,
                    recursive: true);
            }
        }
        catch (IOException)
        {
            // 一時フォルダーの削除失敗はテスト結果に影響させない
        }
    }


    // ============================================
    // 新規DB
    // ============================================

    [Fact]
    public void Initialize_新規DB_全マイグレーションが適用される()
    {
        // Act
        DatabaseInitializer.Initialize(
            _databasePath);

        // Assert
        using var dbContext =
            new InspectionDbContext(
                _databasePath);

        Assert.Equal(
            dbContext.Database.GetMigrations(),
            dbContext.Database.GetAppliedMigrations());

        Assert.Empty(
            dbContext.Database.GetPendingMigrations());
    }


    [Fact]
    public void Initialize_新規DB_HasDataの初期マスターが登録される()
    {
        // Act
        DatabaseInitializer.Initialize(
            _databasePath);

        // Assert
        using var dbContext =
            new InspectionDbContext(
                _databasePath);

        Assert.Equal(
            2,
            dbContext.FactorySites.Count());
    }


    [Fact]
    public void Initialize_保存先フォルダーが無い_フォルダーを作成してDBを作成する()
    {
        // Arrange
        var databasePath =
            Path.Combine(
                _testDirectory,
                "nested",
                "facility-inspection.db");

        // Act
        DatabaseInitializer.Initialize(
            databasePath);

        // Assert
        Assert.True(
            File.Exists(databasePath));
    }


    // ============================================
    // 既存DB
    // ============================================

    [Fact]
    public void Initialize_EnsureCreatedで作成した旧DB_履歴が登録され既存データが保持される()
    {
        // Arrange
        Guid operatorId;

        using (var dbContext =
                   new InspectionDbContext(
                       _databasePath))
        {
            dbContext.Database.EnsureCreated();

            var operatorEntity =
                CreateOperator();

            dbContext.Operators.Add(
                operatorEntity);

            dbContext.SaveChanges();

            operatorId =
                operatorEntity.Id;
        }

        SqliteConnection.ClearAllPools();

        // Act
        var exception =
            Record.Exception(
                () => DatabaseInitializer.Initialize(
                    _databasePath));

        // Assert
        Assert.Null(
            exception);

        using var verifyContext =
            new InspectionDbContext(
                _databasePath);

        Assert.Empty(
            verifyContext.Database.GetPendingMigrations());

        Assert.True(
            verifyContext.Operators.Any(
                x => x.Id == operatorId));
    }


    [Fact]
    public void Initialize_2回実行_2回目も例外にならずデータが保持される()
    {
        // Arrange
        DatabaseInitializer.Initialize(
            _databasePath);

        using (var dbContext =
                   new InspectionDbContext(
                       _databasePath))
        {
            dbContext.Operators.Add(
                CreateOperator());

            dbContext.SaveChanges();
        }

        // Act
        var exception =
            Record.Exception(
                () => DatabaseInitializer.Initialize(
                    _databasePath));

        // Assert
        Assert.Null(
            exception);

        using var verifyContext =
            new InspectionDbContext(
                _databasePath);

        Assert.Equal(
            1,
            verifyContext.Operators.Count());
    }


    [Fact]
    public void Initialize_設備点検アプリ以外のDB_InvalidOperationExceptionを投げる()
    {
        // Arrange
        using (var connection =
                   new SqliteConnection(
                       $"Data Source={_databasePath};Pooling=False"))
        {
            connection.Open();

            using var command =
                connection.CreateCommand();

            command.CommandText =
                "CREATE TABLE Foo (Id INTEGER PRIMARY KEY);";

            command.ExecuteNonQuery();
        }

        // Act & Assert
        Assert.Throws<InvalidOperationException>(
            () => DatabaseInitializer.Initialize(
                _databasePath));
    }


    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    public void Initialize_パスが空_ArgumentExceptionを投げる(
        string databasePath)
    {
        Assert.Throws<ArgumentException>(
            () => DatabaseInitializer.Initialize(
                databasePath));
    }


    // ============================================
    // Helper
    // ============================================

    private static Operator CreateOperator()
    {
        return new Operator
        {
            LoginId = "inspector",
            NormalizedLoginId = "INSPECTOR",
            DisplayName = "テスト点検担当者",
            PasswordHash = "dummy-hash",
            Role = OperatorRole.Inspector,
            IsActive = true
        };
    }
}
