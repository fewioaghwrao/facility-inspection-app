using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Data;

/// <summary>
/// 点検写真の実ファイルをアプリ専用データ領域へ保存する。
/// DBにはこのクラスが返す相対パスだけを保存する。
/// </summary>
public static class InspectionPhotoStorage
{
    private const string ApplicationFolderName =
        "FacilityInspection";

    private static readonly string RootDirectory =
        Path.Combine(
            Environment.GetFolderPath(
                Environment.SpecialFolder.LocalApplicationData),
            ApplicationFolderName);

    public static async Task<string> SaveAsync(
        Stream source,
        Guid scheduleId,
        Guid templateItemId,
        string originalFileName,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            source);

        if (scheduleId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検予定IDを指定してください。",
                nameof(scheduleId));
        }

        if (templateItemId == Guid.Empty)
        {
            throw new ArgumentException(
                "点検項目IDを指定してください。",
                nameof(templateItemId));
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(
            originalFileName);

        var extension =
            NormalizeExtension(
                Path.GetExtension(
                    originalFileName));

        var storedFileName =
            $"{Guid.NewGuid():N}{extension}";

        var relativePath =
            Path.Combine(
                    "photos",
                    scheduleId.ToString("N"),
                    templateItemId.ToString("N"),
                    storedFileName)
                .Replace(
                    Path.DirectorySeparatorChar,
                    '/');

        var absolutePath =
            ToAbsolutePath(
                relativePath);

        var directory =
            Path.GetDirectoryName(
                absolutePath)
            ?? throw new InvalidOperationException(
                "写真保存先フォルダーを解決できませんでした。");

        Directory.CreateDirectory(
            directory);

        await using var destination =
            new FileStream(
                absolutePath,
                FileMode.CreateNew,
                FileAccess.Write,
                FileShare.None,
                81920,
                useAsync: true);

        await source.CopyToAsync(
            destination,
            cancellationToken);

        return relativePath;
    }

    public static void DeleteIfExists(
        string? relativePath)
    {
        if (string.IsNullOrWhiteSpace(
                relativePath))
        {
            return;
        }

        var absolutePath =
            ToAbsolutePath(
                relativePath);

        if (File.Exists(
                absolutePath))
        {
            File.Delete(
                absolutePath);
        }
    }

    public static string GetAbsolutePath(
        string relativePath)
    {
        return ToAbsolutePath(
            relativePath);
    }

    private static string ToAbsolutePath(
        string relativePath)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            relativePath);

        var normalized =
            relativePath
                .Trim()
                .Replace('\\', '/');

        if (normalized.StartsWith('/') ||
            normalized.Contains(":/", StringComparison.Ordinal) ||
            normalized.StartsWith("../", StringComparison.Ordinal) ||
            normalized.Contains("/../", StringComparison.Ordinal) ||
            normalized.EndsWith("/..", StringComparison.Ordinal))
        {
            throw new ArgumentException(
                "写真パスにはアプリ専用データ領域からの相対パスを指定してください。",
                nameof(relativePath));
        }

        var platformRelativePath =
            normalized.Replace(
                '/',
                Path.DirectorySeparatorChar);

        return Path.Combine(
            RootDirectory,
            platformRelativePath);
    }

    private static string NormalizeExtension(
        string? extension)
    {
        if (string.IsNullOrWhiteSpace(
                extension))
        {
            return ".img";
        }

        var normalized =
            extension.Trim().ToLowerInvariant();

        if (normalized.Length > 10 ||
            normalized.IndexOfAny(
                Path.GetInvalidFileNameChars()) >= 0)
        {
            return ".img";
        }

        return normalized.StartsWith('.')
            ? normalized
            : $".{normalized}";
    }
}
