using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using Microsoft.Data.Sqlite;

namespace FacilityInspection.Tests.Data;

/// <summary>
/// Repository結合テスト用の一時SQLiteデータベース。
/// 本番と同じくマイグレーションでスキーマを作成し、
/// 点検予定1件（選択項目・数値項目の2項目）を登録する。
/// </summary>
internal sealed class InspectionTestDatabase
    : IDisposable
{
    public InspectionTestDatabase()
    {
        DirectoryPath =
            Path.Combine(
                Path.GetTempPath(),
                "FacilityInspection.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            DirectoryPath);

        DatabasePath =
            Path.Combine(
                DirectoryPath,
                "facility-inspection.db");

        DatabaseInitializer.Initialize(
            DatabasePath);

        Factory =
            new InspectionDbContextFactory(
                DatabasePath);

        Seed();
    }


    public string DirectoryPath { get; }

    public string DatabasePath { get; }

    public InspectionDbContextFactory Factory { get; }

    public Guid InspectorId { get; private set; }

    public Guid ManagerId { get; private set; }

    public Guid ScheduleId { get; private set; }

    public Guid CheckItemId { get; private set; }

    public Guid NumericItemId { get; private set; }

    /// <summary>
    /// Repositoryが削除しようとした写真の相対パス。
    /// 実ファイル（LocalApplicationData）には触れない。
    /// </summary>
    public List<string> DeletedPhotoPaths { get; } = [];


    public InspectionRepository CreateRepository()
    {
        return new InspectionRepository(
            Factory,
            DeletedPhotoPaths.Add);
    }


    public IReadOnlyCollection<InspectionCompletionItemData> CreateItems(
        bool? checkValue,
        decimal? numericValue,
        IReadOnlyList<InspectionCompletionPhotoData>? checkItemPhotos = null)
    {
        return
        [
            new InspectionCompletionItemData(
                CheckItemId,
                checkValue,
                null,
                null,
                null,
                checkItemPhotos ?? []),

            new InspectionCompletionItemData(
                NumericItemId,
                null,
                numericValue,
                null,
                null,
                [])
        ];
    }


    public static InspectionCompletionPhotoData CreatePhoto(
        string relativePath)
    {
        return new InspectionCompletionPhotoData(
            relativePath,
            new DateTime(
                2026, 11, 20, 0, 0, 0,
                DateTimeKind.Utc));
    }


    public void Dispose()
    {
        // Windows でファイルロックが残らないよう接続プールを解放する
        SqliteConnection.ClearAllPools();

        try
        {
            if (Directory.Exists(DirectoryPath))
            {
                Directory.Delete(
                    DirectoryPath,
                    recursive: true);
            }
        }
        catch (IOException)
        {
            // 一時フォルダーの削除失敗はテスト結果に影響させない
        }
    }


    private void Seed()
    {
        using var dbContext =
            Factory.CreateDbContext();

        var inspector =
            new Operator
            {
                LoginId = "inspector",
                NormalizedLoginId = "INSPECTOR",
                DisplayName = "テスト点検担当者",
                PasswordHash = "dummy-hash",
                Role = OperatorRole.Inspector,
                IsActive = true
            };

        var manager =
            new Operator
            {
                LoginId = "manager",
                NormalizedLoginId = "MANAGER",
                DisplayName = "テスト保全管理者",
                PasswordHash = "dummy-hash",
                Role = OperatorRole.MaintenanceManager,
                IsActive = true
            };

        var site =
            new FactorySite(
                "F01",
                "テスト工場");

        var location =
            new Location(
                site.Id,
                "L01",
                "テスト設置場所");

        var equipment =
            new Equipment(
                location.Id,
                "EQ-001",
                "テストコンプレッサー",
                EquipmentType.AirCompressor);

        var template =
            new InspectionTemplate
            {
                Name = "テスト点検票",
                EquipmentType = EquipmentType.AirCompressor,
                Version = 1,
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            };

        var checkItem =
            new InspectionTemplateItem
            {
                InspectionTemplateId = template.Id,
                ItemName = "異音の有無",
                InputType = InspectionInputType.NormalAbnormal,
                DisplayOrder = 1,
                IsRequired = true,
                IsActive = true
            };

        var numericItem =
            new InspectionTemplateItem
            {
                InspectionTemplateId = template.Id,
                ItemName = "吐出圧力",
                InputType = InspectionInputType.Numeric,
                Unit = "MPa",
                MinimumValue = 0.5,
                MaximumValue = 0.9,
                DisplayOrder = 2,
                IsRequired = true,
                IsActive = true
            };

        template.Items.Add(checkItem);
        template.Items.Add(numericItem);

        var schedule =
            new InspectionSchedule(
                new DateOnly(2026, 11, 20),
                equipment.Id,
                template.Id,
                inspector.Id);

        dbContext.Operators.AddRange(
            inspector,
            manager);

        dbContext.FactorySites.Add(site);
        dbContext.Locations.Add(location);
        dbContext.Equipments.Add(equipment);
        dbContext.InspectionTemplates.Add(template);
        dbContext.InspectionSchedules.Add(schedule);

        dbContext.SaveChanges();

        InspectorId = inspector.Id;
        ManagerId = manager.Id;
        ScheduleId = schedule.Id;
        CheckItemId = checkItem.Id;
        NumericItemId = numericItem.Id;
    }
}
