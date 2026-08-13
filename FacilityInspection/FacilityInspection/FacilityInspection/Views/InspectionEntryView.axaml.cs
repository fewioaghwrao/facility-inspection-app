using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Platform.Storage;
using FacilityInspection.Data;
using FacilityInspection.ViewModels;
using System;

namespace FacilityInspection.Views;

public partial class InspectionEntryView : UserControl
{
    public InspectionEntryView()
    {
        InitializeComponent();
    }

    private async void SelectPhotos_Click(
        object? sender,
        RoutedEventArgs e)
    {
        if (sender is not Button
            {
                DataContext: InspectionEntryItemViewModel item
            })
        {
            return;
        }

        if (DataContext is not InspectionEntryViewModel viewModel ||
            viewModel.IsSaving)
        {
            return;
        }

        var topLevel =
            TopLevel.GetTopLevel(this);

        if (topLevel is null)
        {
            item.SetPhotoError(
                "写真選択画面を開けませんでした。");

            return;
        }

        try
        {
            item.ClearPhotoError();

            var files =
                await topLevel.StorageProvider
                    .OpenFilePickerAsync(
                        new FilePickerOpenOptions
                        {
                            Title = "点検写真を選択",
                            AllowMultiple = true,
                            FileTypeFilter =
                                new[]
                                {
                                    FilePickerFileTypes.ImageAll
                                }
                        });

            foreach (var file in files)
            {
                await using var source =
                    await file.OpenReadAsync();

                var relativePath =
                    await InspectionPhotoStorage
                        .SaveAsync(
                            source,
                            viewModel.ScheduleId,
                            item.TemplateItemId,
                            file.Name);

                item.AddPhoto(
                    file.Name,
                    relativePath,
                    DateTime.UtcNow);
            }
        }
        catch (Exception exception)
        {
            item.SetPhotoError(
                "写真を追加できませんでした。" +
                Environment.NewLine +
                exception.Message);
        }
    }
}
