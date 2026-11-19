using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FacilityInspection.Tests.Data;

/// <summary>
/// InspectionRepository.CompleteAsync の結合テスト。
/// </summary>
public sealed class InspectionRepositoryCompleteTests
    : IDisposable
{
    private readonly InspectionTestDatabase _database;

    private readonly InspectionRepository _repository;


    public InspectionRepositoryCompleteTests()
    {
        _database =
            new InspectionTestDatabase();

        _repository =
            _database.CreateRepository();
    }


    public void Dispose()
    {
        _database.Dispose();
    }


    // ============================================
    // 初回完了
    // ============================================

    [Fact]
    public async Task CompleteAsync_初回完了_点検結果が新規作成され完了状態になる()
    {
        // Arrange
        await StartAsync();

        // Act
        var exception =
            await Record.ExceptionAsync(
                () => _repository.CompleteAsync(
                    _database.ScheduleId,
                    _database.InspectorId,
                    _database.CreateItems(
                        checkValue: true,
                        numericValue: 0.7m)));

        // Assert
        Assert.Null(
            exception);

        await using var dbContext =
            _database.Factory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .SingleAsync(
                    x => x.InspectionScheduleId == _database.ScheduleId);

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
                x => x.InspectionTemplateItemId == _database.CheckItemId);

        Assert.True(
            checkResult.CheckValue);

        Assert.False(
            checkResult.IsAbnormal);

        var numericResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _database.NumericItemId);

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
        await StartAsync();

        // Act
        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: false,
                numericValue: 1.5m));

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

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
        await StartAsync();

        var items =
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    InspectionTestDatabase.CreatePhoto(
                        "photos/test-1.jpg")
                ]);

        // Act
        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            items);

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var checkResult =
            await dbContext.InspectionResults
                .SingleAsync(
                    x => x.InspectionTemplateItemId == _database.CheckItemId);

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
        await StartAsync();

        // Act
        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m));

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var auditLog =
            await dbContext.AuditLogs
                .SingleAsync(
                    x => x.ActionType == AuditActionType.InspectionComplete);

        Assert.Equal(
            _database.InspectorId,
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
        await StartAsync();

        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m));

        await _repository.ReturnAsync(
            _database.ScheduleId,
            "数値を再確認してください。",
            _database.ManagerId);

        await StartAsync();

        // Act
        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: false,
                numericValue: 0.8m));

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .SingleAsync(
                    x => x.InspectionScheduleId == _database.ScheduleId);

        Assert.Equal(
            InspectionStatus.Completed,
            inspection.Status);

        Assert.Equal(
            2,
            inspection.Results.Count);

        var checkResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _database.CheckItemId);

        Assert.False(
            checkResult.CheckValue);

        Assert.True(
            checkResult.IsAbnormal);

        var numericResult =
            inspection.Results.Single(
                x => x.InspectionTemplateItemId == _database.NumericItemId);

        Assert.Equal(
            0.8m,
            numericResult.NumericValue);
    }


    [Fact]
    public async Task CompleteAsync_差し戻し後の再開_前回の写真が再開データに含まれ完了後も保持される()
    {
        // Arrange
        await StartAsync();

        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    InspectionTestDatabase.CreatePhoto(
                        "photos/first.jpg")
                ]));

        await _repository.ReturnAsync(
            _database.ScheduleId,
            "写真を追加してください。",
            _database.ManagerId);

        // Act
        var entry =
            await StartAsync();

        var resumedPhotos =
            entry.Items
                .Single(x => x.TemplateItemId == _database.CheckItemId)
                .Photos!;

        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    InspectionTestDatabase.CreatePhoto(
                        "photos/first.jpg"),
                    InspectionTestDatabase.CreatePhoto(
                        "photos/second.jpg")
                ]));

        // Assert
        Assert.Equal(
            "photos/first.jpg",
            Assert.Single(resumedPhotos).RelativePath);

        await using var dbContext =
            _database.Factory.CreateDbContext();

        var paths =
            await dbContext.InspectionPhotos
                .OrderBy(x => x.DisplayOrder)
                .Select(x => x.RelativePath)
                .ToListAsync();

        Assert.Equal(
            new[] { "photos/first.jpg", "photos/second.jpg" },
            paths);
    }


    // ============================================
    // Helper
    // ============================================

    private Task<InspectionEntryData> StartAsync()
    {
        return _repository.StartOrResumeAsync(
            _database.ScheduleId,
            _database.InspectorId);
    }
}
