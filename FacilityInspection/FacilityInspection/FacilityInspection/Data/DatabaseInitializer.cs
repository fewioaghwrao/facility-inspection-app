using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.IO;
using System.Linq;

namespace FacilityInspection.Data;

/// <summary>
/// SQLiteデータベースのスキーマを最新のマイグレーションまで更新する。
/// </summary>
/// <remarks>
/// 旧バージョン（EnsureCreatedで作成したDB）には
/// マイグレーション履歴テーブルが存在しないため、
/// 初回マイグレーション（InitialCreate）適用済みとして履歴を登録してから
/// 残りのマイグレーションを適用する。
///
/// この扱いは「InitialCreate が EnsureCreated 時代のモデルと同一」
/// であることを前提とする。InitialCreate は再生成しないこと。
/// </remarks>
public static class DatabaseInitializer
{
    /// <summary>
    /// 旧DBかどうかの判定に使用するテーブル。
    /// </summary>
    private const string LegacyMarkerTableName =
        "Operators";


    public static void Initialize(
        string databasePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            databasePath);

        var databaseDirectory =
            Path.GetDirectoryName(
                Path.GetFullPath(
                    databasePath));

        if (!string.IsNullOrWhiteSpace(
                databaseDirectory))
        {
            Directory.CreateDirectory(
                databaseDirectory);
        }

        using (var dbContext =
                   new InspectionDbContext(
                       databasePath))
        {
            var migrations =
                dbContext.Database
                    .GetMigrations()
                    .ToList();

            if (migrations.Count == 0)
            {
                throw new InvalidOperationException(
                    "マイグレーションが見つかりません。" +
                    "dotnet ef migrations add InitialCreate を実行してください。");
            }

            BaselineLegacyDatabase(
                dbContext,
                initialMigrationId:
                    migrations[0]);

            dbContext.Database.Migrate();
        }

        /*
         * 復元処理などで直後にファイルを操作できるよう、
         * プールされた接続を解放しておく。
         */
        SqliteConnection.ClearAllPools();
    }


    // ============================================
    // 旧DB（EnsureCreated）対応
    // ============================================

    private static void BaselineLegacyDatabase(
        InspectionDbContext dbContext,
        string initialMigrationId)
    {
        var databaseCreator =
            dbContext.GetService<IRelationalDatabaseCreator>();

        if (!databaseCreator.Exists() ||
            !databaseCreator.HasTables())
        {
            // 新規DB：通常のマイグレーションで作成する
            return;
        }

        var historyRepository =
            dbContext.GetService<IHistoryRepository>();

        if (historyRepository.Exists())
        {
            // マイグレーション管理済みDB
            return;
        }

        if (!TableExists(
                dbContext,
                LegacyMarkerTableName))
        {
            throw new InvalidOperationException(
                "設備点検アプリのデータベースではないため、" +
                "スキーマを更新できません。");
        }

        dbContext.Database.ExecuteSqlRaw(
            historyRepository
                .GetCreateIfNotExistsScript());

        dbContext.Database.ExecuteSqlRaw(
            historyRepository
                .GetInsertScript(
                    new HistoryRow(
                        initialMigrationId,
                        ProductInfo.GetVersion())));
    }


    private static bool TableExists(
        InspectionDbContext dbContext,
        string tableName)
    {
        var connection =
            dbContext.Database
                .GetDbConnection();

        var shouldClose =
            connection.State !=
                System.Data.ConnectionState.Open;

        if (shouldClose)
        {
            connection.Open();
        }

        try
        {
            using var command =
                connection.CreateCommand();

            command.CommandText =
                """
                SELECT COUNT(*)
                FROM sqlite_master
                WHERE type = 'table'
                  AND name = $tableName;
                """;

            var parameter =
                command.CreateParameter();

            parameter.ParameterName =
                "$tableName";

            parameter.Value =
                tableName;

            command.Parameters.Add(
                parameter);

            return Convert.ToInt64(
                command.ExecuteScalar()) > 0;
        }
        finally
        {
            if (shouldClose)
            {
                connection.Close();
            }
        }
    }
}
