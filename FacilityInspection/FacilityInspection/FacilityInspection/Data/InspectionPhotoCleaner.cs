using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

public sealed record InspectionPhotoCleanupResult(
    int MovedToTrashCount,
    int DeletedTrashFolderCount);

/// <summary>
/// DBから参照されていない写真ファイル（異常終了などで残ったもの）を整理する。
/// </summary>
/// <remarks>
/// 参照されていない写真はすぐには削除せず、photos-trash/yyyyMMdd/ へ移動する。
/// バックアップ復元前のDB（restore-safety）を戻した場合など、
/// 後から参照が復活する可能性があるため。
/// 保持期間を過ぎたゴミ箱フォルダーだけを削除する。
/// </remarks>
public sealed class InspectionPhotoCleaner
{
    public const string PhotosFolderName =
        "photos";

    public const string TrashFolderName =
        "photos-trash";

    private const string TrashFolderDateFormat =
        "yyyyMMdd";

    private readonly InspectionDbContextFactory
        _dbContextFactory;

    private readonly string
        _rootDirectory;

    private readonly TimeProvider
        _timeProvider;


    public InspectionPhotoCleaner(
        InspectionDbContextFactory dbContextFactory,
        string rootDirectory,
        TimeProvider? timeProvider = null)
    {
        ArgumentNullException.ThrowIfNull(
            dbContextFactory);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            rootDirectory);

        _dbContextFactory =
            dbContextFactory;

        _rootDirectory =
            rootDirectory;

        _timeProvider =
            timeProvider ??
            TimeProvider.System;
    }


    /// <param name="minimumOrphanAge">
    /// 作成からこの時間が経っていない未参照ファイルは対象外にする
    /// （入力中でまだ保存されていない写真を守るため）。
    /// </param>
    /// <param name="trashRetention">
    /// ゴミ箱フォルダーを保持する期間。
    /// </param>
    public async Task<InspectionPhotoCleanupResult> CleanupAsync(
        TimeSpan minimumOrphanAge,
        TimeSpan trashRetention,
        CancellationToken cancellationToken = default)
    {
        var nowUtc =
            _timeProvider
                .GetUtcNow()
                .UtcDateTime;

        var referencedPaths =
            await GetReferencedPathsAsync(
                cancellationToken);

        var movedCount =
            MoveOrphanFilesToTrash(
                referencedPaths,
                nowUtc,
                minimumOrphanAge);

        var deletedTrashFolderCount =
            DeleteExpiredTrashFolders(
                nowUtc,
                trashRetention);

        return new InspectionPhotoCleanupResult(
            movedCount,
            deletedTrashFolderCount);
    }


    // ============================================
    // 参照中の写真
    // ============================================

    private async Task<HashSet<string>>
        GetReferencedPathsAsync(
            CancellationToken cancellationToken)
    {
        await using var dbContext =
            _dbContextFactory.CreateDbContext();

        var paths =
            await dbContext.InspectionPhotos
                .AsNoTracking()
                .Select(x =>
                    x.RelativePath)
                .ToListAsync(
                    cancellationToken);

        return paths
            .Select(NormalizeRelativePath)
            .ToHashSet(
                StringComparer.OrdinalIgnoreCase);
    }


    // ============================================
    // 未参照ファイル → ゴミ箱
    // ============================================

    private int MoveOrphanFilesToTrash(
        HashSet<string> referencedPaths,
        DateTime nowUtc,
        TimeSpan minimumOrphanAge)
    {
        var photosDirectory =
            Path.Combine(
                _rootDirectory,
                PhotosFolderName);

        if (!Directory.Exists(
                photosDirectory))
        {
            return 0;
        }

        var trashDirectory =
            Path.Combine(
                _rootDirectory,
                TrashFolderName,
                nowUtc.ToString(
                    TrashFolderDateFormat,
                    CultureInfo.InvariantCulture));

        var movedCount =
            0;

        foreach (var filePath in
                 Directory.EnumerateFiles(
                     photosDirectory,
                     "*",
                     SearchOption.AllDirectories))
        {
            var relativePath =
                NormalizeRelativePath(
                    Path.GetRelativePath(
                        _rootDirectory,
                        filePath));

            if (referencedPaths.Contains(
                    relativePath))
            {
                continue;
            }

            try
            {
                var age =
                    nowUtc -
                    File.GetLastWriteTimeUtc(
                        filePath);

                if (age < minimumOrphanAge)
                {
                    continue;
                }

                var destinationPath =
                    Path.Combine(
                        trashDirectory,
                        Path.GetRelativePath(
                            photosDirectory,
                            filePath));

                if (File.Exists(
                        destinationPath))
                {
                    destinationPath =
                        Path.Combine(
                            Path.GetDirectoryName(destinationPath)!,
                            $"{Path.GetFileNameWithoutExtension(destinationPath)}" +
                            $"_{Guid.NewGuid():N}" +
                            $"{Path.GetExtension(destinationPath)}");
                }

                Directory.CreateDirectory(
                    Path.GetDirectoryName(destinationPath)!);

                File.Move(
                    filePath,
                    destinationPath);

                movedCount++;
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException)
            {
                // 使用中などで移動できないファイルは次回に回す
            }
        }

        DeleteEmptyDirectories(
            photosDirectory);

        return movedCount;
    }


    // ============================================
    // 期限切れゴミ箱の削除
    // ============================================

    private int DeleteExpiredTrashFolders(
        DateTime nowUtc,
        TimeSpan trashRetention)
    {
        var trashRoot =
            Path.Combine(
                _rootDirectory,
                TrashFolderName);

        if (!Directory.Exists(
                trashRoot))
        {
            return 0;
        }

        var deletedCount =
            0;

        foreach (var directory in
                 Directory.GetDirectories(
                     trashRoot))
        {
            if (!DateTime.TryParseExact(
                    Path.GetFileName(directory),
                    TrashFolderDateFormat,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.AssumeUniversal |
                    DateTimeStyles.AdjustToUniversal,
                    out var folderDateUtc))
            {
                // アプリが作成したフォルダーではないため触らない
                continue;
            }

            if (nowUtc.Date - folderDateUtc.Date <=
                trashRetention)
            {
                continue;
            }

            try
            {
                Directory.Delete(
                    directory,
                    recursive: true);

                deletedCount++;
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException)
            {
                // 次回に回す
            }
        }

        return deletedCount;
    }


    // ============================================
    // Helper
    // ============================================

    private static void DeleteEmptyDirectories(
        string rootDirectory)
    {
        foreach (var directory in
                 Directory.GetDirectories(
                     rootDirectory))
        {
            DeleteEmptyDirectories(
                directory);

            try
            {
                if (!Directory.EnumerateFileSystemEntries(
                        directory).Any())
                {
                    Directory.Delete(
                        directory);
                }
            }
            catch (Exception exception) when (
                exception is IOException or
                UnauthorizedAccessException)
            {
                // 空フォルダーが残っても動作に影響しない
            }
        }
    }


    private static string NormalizeRelativePath(
        string relativePath)
    {
        return relativePath
            .Trim()
            .Replace('\\', '/');
    }
}
