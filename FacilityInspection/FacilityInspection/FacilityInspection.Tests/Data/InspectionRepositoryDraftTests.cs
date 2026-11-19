using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.Domain.Inspections;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace FacilityInspection.Tests.Data;

/// <summary>
/// InspectionRepository.SaveDraftAsync（一時保存）の結合テスト。
/// </summary>
public sealed class InspectionRepositoryDraftTests
    : IDisposable
{
    private readonly InspectionTestDatabase _database;

    private readonly InspectionRepository _repository;


    public InspectionRepositoryDraftTests()
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
    // 一時保存
    // ============================================

    [Fact]
    public async Task SaveDraftAsync_必須項目が未入力_保存でき状態は実施中のまま()
    {
        // Arrange
        await StartAsync();

        // Act
        await SaveDraftAsync(
            checkValue: null,
            numericValue: null);

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .SingleAsync();

        Assert.Equal(
            InspectionStatus.InProgress,
            inspection.Status);

        Assert.Null(
            inspection.CompletedAtUtc);

        Assert.Equal(
            2,
            inspection.Results.Count);

        Assert.All(
            inspection.Results,
            x =>
            {
                Assert.Null(x.CheckValue);
                Assert.Null(x.NumericValue);
            });
    }


    [Fact]
    public async Task SaveDraftAsync_完了の監査ログは記録しない()
    {
        // Arrange
        await StartAsync();

        // Act
        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m);

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        Assert.False(
            await dbContext.AuditLogs.AnyAsync(
                x => x.ActionType == AuditActionType.InspectionComplete));
    }


    [Fact]
    public async Task SaveDraftAsync_保存後に再開_入力値と写真が復元される()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: false,
            numericValue: 0.7m,
            "photos/draft-1.jpg");

        // Act
        var entry =
            await StartAsync();

        // Assert
        var checkItem =
            entry.Items.Single(
                x => x.TemplateItemId == _database.CheckItemId);

        Assert.False(
            checkItem.CheckValue);

        var photo =
            Assert.Single(
                checkItem.Photos!);

        Assert.Equal(
            "photos/draft-1.jpg",
            photo.RelativePath);

        var numericItem =
            entry.Items.Single(
                x => x.TemplateItemId == _database.NumericItemId);

        Assert.Equal(
            0.7m,
            numericItem.NumericValue);

        Assert.Empty(
            numericItem.Photos!);

        Assert.Equal(
            InspectionStatus.InProgress,
            entry.Status);
    }


    [Fact]
    public async Task SaveDraftAsync_同じ内容で2回保存_結果と写真が重複しない()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m,
            "photos/draft-1.jpg");

        // Act
        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.8m,
            "photos/draft-1.jpg");

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        Assert.Equal(
            2,
            await dbContext.InspectionResults.CountAsync());

        Assert.Equal(
            1,
            await dbContext.InspectionPhotos.CountAsync());

        var numericResult =
            await dbContext.InspectionResults
                .SingleAsync(
                    x => x.InspectionTemplateItemId == _database.NumericItemId);

        Assert.Equal(
            0.8m,
            numericResult.NumericValue);
    }


    [Fact]
    public async Task SaveDraftAsync_保存済み写真を外して保存_写真が削除され実ファイル削除が呼ばれる()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m,
            "photos/draft-1.jpg",
            "photos/draft-2.jpg");

        // Act
        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m,
            "photos/draft-2.jpg");

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var remaining =
            await dbContext.InspectionPhotos
                .SingleAsync();

        Assert.Equal(
            "photos/draft-2.jpg",
            remaining.RelativePath);

        Assert.Equal(
            new[] { "photos/draft-1.jpg" },
            _database.DeletedPhotoPaths);
    }


    [Fact]
    public async Task SaveDraftAsync_photos配下以外の写真を外す_実ファイル削除は呼ばれない()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m,
            "sample/images/resultcomp1.jpg");

        // Act
        await SaveDraftAsync(
            checkValue: true,
            numericValue: 0.7m);

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        Assert.False(
            await dbContext.InspectionPhotos.AnyAsync());

        Assert.Empty(
            _database.DeletedPhotoPaths);
    }


    [Fact]
    public async Task SaveDraftAsync_入力形式が不正_例外となり何も保存されない()
    {
        // Arrange
        await StartAsync();

        // 数値項目に選択値を指定する
        IReadOnlyCollection<InspectionCompletionItemData> items =
        [
            new InspectionCompletionItemData(
                _database.CheckItemId,
                true,
                null,
                null,
                null,
                []),

            new InspectionCompletionItemData(
                _database.NumericItemId,
                true,
                null,
                null,
                null,
                [])
        ];

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.SaveDraftAsync(
                _database.ScheduleId,
                _database.InspectorId,
                items));

        await using var dbContext =
            _database.Factory.CreateDbContext();

        Assert.False(
            await dbContext.InspectionResults.AnyAsync());
    }


    [Fact]
    public async Task SaveDraftAsync_完了済みの点検_例外となる()
    {
        // Arrange
        await StartAsync();

        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m));

        // Act
        var exception =
            await Assert.ThrowsAsync<InvalidOperationException>(
                () => SaveDraftAsync(
                    checkValue: true,
                    numericValue: 0.7m));

        // Assert
        Assert.Contains(
            "一時保存",
            exception.Message);
    }


    [Fact]
    public async Task SaveDraftAsync_担当外の点検担当者_例外となる()
    {
        // Arrange
        await StartAsync();

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.SaveDraftAsync(
                _database.ScheduleId,
                _database.ManagerId,
                _database.CreateItems(
                    checkValue: true,
                    numericValue: 0.7m)));
    }


    // ============================================
    // 一時保存 → 完了
    // ============================================

    [Fact]
    public async Task CompleteAsync_一時保存後に完了_写真が維持され重複しない()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: true,
            numericValue: null,
            "photos/draft-1.jpg");

        // Act
        await _repository.CompleteAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    InspectionTestDatabase.CreatePhoto(
                        "photos/draft-1.jpg")
                ]));

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .Include(x => x.Results)
                .Include(x => x.Photos)
                .SingleAsync();

        Assert.Equal(
            InspectionStatus.Completed,
            inspection.Status);

        Assert.Equal(
            2,
            inspection.Results.Count);

        Assert.Single(
            inspection.Photos);

        Assert.Empty(
            _database.DeletedPhotoPaths);
    }


    [Fact]
    public async Task CompleteAsync_一時保存後も必須項目が未入力_例外となり実施中のまま()
    {
        // Arrange
        await StartAsync();

        await SaveDraftAsync(
            checkValue: null,
            numericValue: null);

        // Act
        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _repository.CompleteAsync(
                _database.ScheduleId,
                _database.InspectorId,
                _database.CreateItems(
                    checkValue: null,
                    numericValue: 0.7m)));

        // Assert
        await using var dbContext =
            _database.Factory.CreateDbContext();

        var inspection =
            await dbContext.Inspections
                .SingleAsync();

        Assert.Equal(
            InspectionStatus.InProgress,
            inspection.Status);
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


    private Task SaveDraftAsync(
        bool? checkValue,
        decimal? numericValue,
        params string[] checkItemPhotoPaths)
    {
        return _repository.SaveDraftAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue,
                numericValue,
                checkItemPhotoPaths
                    .Select(InspectionTestDatabase.CreatePhoto)
                    .ToList()));
    }
}
