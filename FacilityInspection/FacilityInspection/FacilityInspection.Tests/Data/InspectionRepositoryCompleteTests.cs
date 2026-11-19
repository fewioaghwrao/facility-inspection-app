using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.Domain.Locations;
using FacilityInspection.Domain.Operators;
using FacilityInspection.Domain.Sites;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FacilityInspection.Tests.Data;

/// <summary>
/// InspectionRepository.CompleteAsync の結合テスト。
/// 一時フォルダーに作成した SQLite ファイルを使用する。
/// </summary>
public sealed class InspectionRepositoryCompleteTests
    : IDisposable
{
    private readonly string _testDirectory;

    private readonly InspectionDbContextFactory _dbContextFactory;

    private readonly InspectionRepository _repository;

    private Guid _inspectorId;

    private Guid _managerId;

    private Guid _scheduleId;

    private Guid _checkItemId;

    private Guid _numericItemId;


    public InspectionRepositoryCompleteTests()
    {
        _testDirectory =
            Path.Combine(
                Path.GetTempPath(),
                "FacilityInspection.Tests",
                Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(
            _testDirectory);

        var databasePath =
            Path.Combine(
                _testDirectory,
                "facility-inspection.db");

        _dbContextFactory =
            new InspectionDbContextFactory(
                databasePath);

        _repository =
            new InspectionRepository(
                _dbContextFactory);

        SeedMinimumData();
    }


    public void Dispose()
    {
        // Windows でファイルロックが残らないよう接続プールを解放する
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
    // 初回完了
    // ============================================

    [Fact]
    public async Task CompleteAsync_初回完了_点検結果が新規作成され完了状態になる()
    {
        // Arrange
        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        // Act
        var exception =
            await Record.ExceptionAsync(
                () => _repository.CompleteAsync(
                    _scheduleId,
                    _inspectorId,
                    CreateItems(
                        checkValue: true,
                        numericValue: 0.7m)));

        // Assert
        Assert.Null(
            exception);

        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .SingleAsync(
                    x => x.InspectionScheduleId == _scheduleId);

        Assert.Equal(
            InspectionStatus.Completed,
            inspection.Status);

        Assert.NotNull(
            inspection.CompletedAtUtc);

        Assert.Equal(
            2,
            inspection.Results.Count);

        var checkResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _checkItemId);

        Assert.True(
            checkResult.CheckValue);

        Assert.False(
            checkResult.IsAbnormal);

        var numericResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _numericItemId);

        Assert.Equal(
            0.7m,
            numericResult.NumericValue);

        Assert.False(
            numericResult.IsAbnormal);
    }


    [Fact]
    public async Task CompleteAsync_初回完了_異常値は異常として記録される()
    {
        // Arrange
        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        // Act
        await _repository.CompleteAsync(
            _scheduleId,
            _inspectorId,
            CreateItems(
                checkValue: false,
                numericValue: 1.5m));

        // Assert
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var results =
            await dbContext.InspectionResults
                .ToListAsync();

        Assert.Equal(
            2,
            results.Count);

        Assert.All(
            results,
            x => Assert.True(x.IsAbnormal));
    }


    [Fact]
    public async Task CompleteAsync_初回完了_写真が点検結果に紐づいて登録される()
    {
        // Arrange
        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        var capturedAtUtc =
            new DateTime(
                2026, 11, 20, 0, 0, 0,
                DateTimeKind.Utc);

        var items =
            CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    new InspectionCompletionPhotoData(
                        "photos/test-1.jpg",
                        capturedAtUtc)
                ]);

        // Act
        await _repository.CompleteAsync(
            _scheduleId,
            _inspectorId,
            items);

        // Assert
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var checkResult =
            await dbContext.InspectionResults
                .SingleAsync(
                    x => x.InspectionTemplateItemId == _checkItemId);

        var photo =
            await dbContext.InspectionPhotos
                .SingleAsync();

        Assert.Equal(
            checkResult.Id,
            photo.InspectionResultId);

        Assert.Equal(
            "photos/test-1.jpg",
            photo.RelativePath);
    }


    [Fact]
    public async Task CompleteAsync_初回完了_完了操作が監査ログに記録される()
    {
        // Arrange
        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        // Act
        await _repository.CompleteAsync(
            _scheduleId,
            _inspectorId,
            CreateItems(
                checkValue: true,
                numericValue: 0.7m));

        // Assert
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var auditLog =
            await dbContext.AuditLogs
                .SingleAsync(
                    x => x.ActionType == AuditActionType.InspectionComplete);

        Assert.Equal(
            _inspectorId,
            auditLog.OperatorId);

        Assert.Equal(
            InspectionStatus.Completed.ToString(),
            auditLog.AfterValue);
    }


    // ============================================
    // 差し戻し後の再完了
    // ============================================

    [Fact]
    public async Task CompleteAsync_差し戻し後の再完了_既存の点検結果が更新され重複しない()
    {
        // Arrange
        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        await _repository.CompleteAsync(
            _scheduleId,
            _inspectorId,
            CreateItems(
                checkValue: true,
                numericValue: 0.7m));

        await _repository.ReturnAsync(
            _scheduleId,
            "数値を再確認してください。",
            _managerId);

        await _repository.StartOrResumeAsync(
            _scheduleId,
            _inspectorId);

        // Act
        await _repository.CompleteAsync(
            _scheduleId,
            _inspectorId,
            CreateItems(
                checkValue: false,
                numericValue: 0.8m));

        // Assert
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .SingleAsync(
                    x => x.InspectionScheduleId == _scheduleId);

        Assert.Equal(
            InspectionStatus.Completed,
            inspection.Status);

        Assert.Equal(
            2,
            inspection.Results.Count);

        var checkResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _checkItemId);

        Assert.False(
            checkResult.CheckValue);

        Assert.True(
            checkResult.IsAbnormal);

        var numericResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _numericItemId);

        Assert.Equal(
            0.8m,
            numericResult.NumericValue);
    }


    // ============================================
    // Helper
    // ============================================

    private IReadOnlyCollection<InspectionCompletionItemData> CreateItems(
        bool checkValue,
        decimal numericValue,
        IReadOnlyList<InspectionCompletionPhotoData>? checkItemPhotos = null)
    {
        return
        [
            new InspectionCompletionItemData(
                _checkItemId,
                checkValue,
                null,
                null,
                null,
                checkItemPhotos ?? []),

            new InspectionCompletionItemData(
                _numericItemId,
                null,
                numericValue,
                null,
                null,
                [])
        ];
    }


    private void SeedMinimumData()
    {
        using var dbContext =
            _dbContextFactory.CreateDbContext();

        dbContext.Database.EnsureCreated();

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

        _inspectorId = inspector.Id;
        _managerId = manager.Id;
        _scheduleId = schedule.Id;
        _checkItemId = checkItem.Id;
        _numericItemId = numericItem.Id;
    }
}
