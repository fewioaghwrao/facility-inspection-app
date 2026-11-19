using FacilityInspection.Data;
using Xunit;

namespace FacilityInspection.Tests.Data;

public sealed class InspectionPhotoCleanerTests
    : IDisposable
{
    private static readonly DateTimeOffset Now =
        new(2026, 11, 20, 12, 0, 0, TimeSpan.Zero);

    private static readonly TimeSpan MinimumOrphanAge =
        TimeSpan.FromHours(1);

    private static readonly TimeSpan TrashRetention =
        TimeSpan.FromDays(30);

    private readonly InspectionTestDatabase _database;

    private readonly string _rootDirectory;

    private readonly InspectionPhotoCleaner _cleaner;


    public InspectionPhotoCleanerTests()
    {
        _database =
            new InspectionTestDatabase();

        _rootDirectory =
            Path.Combine(
                _database.DirectoryPath,
                "appdata");

        Directory.CreateDirectory(
            _rootDirectory);

        _cleaner =
            new InspectionPhotoCleaner(
                _database.Factory,
                _rootDirectory,
                new FixedTimeProvider(Now));
    }


    public void Dispose()
    {
        _database.Dispose();
    }


    // ============================================
    // 未参照ファイル
    // ============================================

    [Fact]
    public async Task CleanupAsync_参照されていない古い写真_ゴミ箱へ移動される()
    {
        // Arrange
        var orphan =
            CreatePhotoFile(
                "photos/s1/i1/orphan.jpg",
                age: TimeSpan.FromHours(2));

        // Act
        var result =
            await CleanupAsync();

        // Assert
        Assert.Equal(
            1,
            result.MovedToTrashCount);

        Assert.False(
            File.Exists(orphan));

        Assert.True(
            File.Exists(
                Path.Combine(
                    _rootDirectory,
                    "photos-trash",
                    "20261120",
                    "s1",
                    "i1",
                    "orphan.jpg")));
    }


    [Fact]
    public async Task CleanupAsync_DBから参照されている写真_移動されない()
    {
        // Arrange
        const string relativePath =
            "photos/s1/i1/referenced.jpg";

        var referenced =
            CreatePhotoFile(
                relativePath,
                age: TimeSpan.FromDays(10));

        var repository =
            _database.CreateRepository();

        await repository.StartOrResumeAsync(
            _database.ScheduleId,
            _database.InspectorId);

        await repository.SaveDraftAsync(
            _database.ScheduleId,
            _database.InspectorId,
            _database.CreateItems(
                checkValue: true,
                numericValue: 0.7m,
                checkItemPhotos:
                [
                    InspectionTestDatabase.CreatePhoto(
                        relativePath)
                ]));

        // Act
        var result =
            await CleanupAsync();

        // Assert
        Assert.Equal(
            0,
            result.MovedToTrashCount);

        Assert.True(
            File.Exists(referenced));
    }


    [Fact]
    public async Task CleanupAsync_作成から間もない未参照の写真_移動されない()
    {
        // Arrange
        var recent =
            CreatePhotoFile(
                "photos/s1/i1/recent.jpg",
                age: TimeSpan.FromMinutes(10));

        // Act
        var result =
            await CleanupAsync();

        // Assert
        Assert.Equal(
            0,
            result.MovedToTrashCount);

        Assert.True(
            File.Exists(recent));
    }


    [Fact]
    public async Task CleanupAsync_写真を移動して空になったフォルダー_削除される()
    {
        // Arrange
        CreatePhotoFile(
            "photos/s1/i1/orphan.jpg",
            age: TimeSpan.FromHours(2));

        // Act
        await CleanupAsync();

        // Assert
        Assert.False(
            Directory.Exists(
                Path.Combine(
                    _rootDirectory,
                    "photos",
                    "s1")));

        Assert.True(
            Directory.Exists(
                Path.Combine(
                    _rootDirectory,
                    "photos")));
    }


    [Fact]
    public async Task CleanupAsync_写真フォルダーが無い_例外にならない()
    {
        // Act
        var exception =
            await Record.ExceptionAsync(
                () => CleanupAsync());

        // Assert
        Assert.Null(
            exception);
    }


    // ============================================
    // ゴミ箱
    // ============================================

    [Fact]
    public async Task CleanupAsync_保持期間を過ぎたゴミ箱フォルダー_削除される()
    {
        // Arrange
        var expired =
            CreateTrashFolder("20261001");

        var kept =
            CreateTrashFolder("20261115");

        var unknown =
            CreateTrashFolder("manual-backup");

        // Act
        var result =
            await CleanupAsync();

        // Assert
        Assert.Equal(
            1,
            result.DeletedTrashFolderCount);

        Assert.False(
            Directory.Exists(expired));

        Assert.True(
            Directory.Exists(kept));

        // アプリが作成した形式ではないフォルダーは触らない
        Assert.True(
            Directory.Exists(unknown));
    }


    // ============================================
    // Helper
    // ============================================

    private Task<InspectionPhotoCleanupResult> CleanupAsync()
    {
        return _cleaner.CleanupAsync(
            MinimumOrphanAge,
            TrashRetention);
    }


    private string CreatePhotoFile(
        string relativePath,
        TimeSpan age)
    {
        var path =
            Path.Combine(
                _rootDirectory,
                relativePath.Replace(
                    '/',
                    Path.DirectorySeparatorChar));

        Directory.CreateDirectory(
            Path.GetDirectoryName(path)!);

        File.WriteAllBytes(
            path,
            [0x01, 0x02, 0x03]);

        File.SetLastWriteTimeUtc(
            path,
            Now.UtcDateTime - age);

        return path;
    }


    private string CreateTrashFolder(
        string folderName)
    {
        var path =
            Path.Combine(
                _rootDirectory,
                "photos-trash",
                folderName);

        Directory.CreateDirectory(
            path);

        File.WriteAllBytes(
            Path.Combine(path, "photo.jpg"),
            [0x01]);

        return path;
    }


    private sealed class FixedTimeProvider(
        DateTimeOffset utcNow)
        : TimeProvider
    {
        public override DateTimeOffset GetUtcNow()
        {
            return utcNow;
        }
    }
}
