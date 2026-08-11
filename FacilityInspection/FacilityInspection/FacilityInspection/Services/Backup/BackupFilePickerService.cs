using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform.Storage;
using System;
using System.Threading.Tasks;

namespace FacilityInspection.Services.Backup;

public sealed class BackupFilePickerService
{
    private static readonly
        FilePickerFileType DatabaseFileType =
            new(
                "SQLite Database")
            {
                Patterns =
                [
                    "*.db"
                ],

                MimeTypes =
                [
                    "application/vnd.sqlite3",
                    "application/octet-stream"
                ]
            };


    // ============================================
    // Backup destination
    // ============================================

    public async Task<IStorageFile?>
        PickBackupDestinationAsync(
            string suggestedFileName)
    {
        var topLevel =
            GetTopLevel();

        return await topLevel
            .StorageProvider
            .SaveFilePickerAsync(
                new FilePickerSaveOptions
                {
                    Title =
                        "バックアップDBの保存先",

                    SuggestedFileName =
                        suggestedFileName,

                    DefaultExtension =
                        "db",

                    FileTypeChoices =
                    [
                        DatabaseFileType
                    ],

                    ShowOverwritePrompt =
                        true
                });
    }


    // ============================================
    // Restore source
    // ============================================

    public async Task<IStorageFile?>
        PickRestoreSourceAsync()
    {
        var topLevel =
            GetTopLevel();

        var files =
            await topLevel
                .StorageProvider
                .OpenFilePickerAsync(
                    new FilePickerOpenOptions
                    {
                        Title =
                            "復元するバックアップDBを選択",

                        AllowMultiple =
                            false,

                        FileTypeFilter =
                        [
                            DatabaseFileType
                        ]
                    });

        return files.Count == 0
            ? null
            : files[0];
    }


    // ============================================
    // TopLevel
    // ============================================

    private static TopLevel GetTopLevel()
    {
        if (Application.Current?
                .ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime
            desktop &&
            desktop.MainWindow is not null)
        {
            return desktop.MainWindow;
        }

        if (Application.Current?
                .ApplicationLifetime
            is ISingleViewApplicationLifetime
            singleView &&
            singleView.MainView is not null)
        {
            var topLevel =
                TopLevel.GetTopLevel(
                    singleView.MainView);

            if (topLevel is not null)
            {
                return topLevel;
            }
        }

        throw new InvalidOperationException(
            "ファイル選択画面を表示できません。");
    }
}