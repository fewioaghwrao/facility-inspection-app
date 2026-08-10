using Avalonia.Media.Imaging;
using FacilityInspection.Data;
using System;
using System.IO;

namespace FacilityInspection.ViewModels;

public sealed class InspectionPhotoDetailItemViewModel
{
    public InspectionPhotoDetailItemViewModel(
        InspectionPhotoDetailData source)
    {
        ArgumentNullException.ThrowIfNull(source);

        PhotoId = source.PhotoId;
        InspectionResultId = source.InspectionResultId;
        RelativePath = source.RelativePath;
        Caption = source.Caption ?? string.Empty;
        DisplayOrder = source.DisplayOrder;
        CapturedAtUtc = source.CapturedAtUtc;

        PhotoSource =
            LoadPhoto(
                RelativePath);
    }

    public Guid PhotoId { get; }

    public Guid? InspectionResultId { get; }

    public string RelativePath { get; }

    public string Caption { get; }

    public int DisplayOrder { get; }

    public DateTime CapturedAtUtc { get; }

    public Bitmap? PhotoSource { get; }

    public bool IsGeneralPhoto =>
        InspectionResultId is null;

    public bool HasCaption =>
        !string.IsNullOrWhiteSpace(
            Caption);

    public string CapturedAtText =>
        CapturedAtUtc
            .ToLocalTime()
            .ToString("yyyy/MM/dd HH:mm");

    private static Bitmap? LoadPhoto(
        string relativePath)
    {
        if (string.IsNullOrWhiteSpace(
                relativePath))
        {
            return null;
        }

        var normalizedPath =
            relativePath.Replace(
                '/',
                Path.DirectorySeparatorChar);

        var fullPath =
            Path.Combine(
                AppContext.BaseDirectory,
                normalizedPath);

        if (!File.Exists(fullPath))
        {
            return null;
        }

        return new Bitmap(
            fullPath);
    }
}