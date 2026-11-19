using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using System;

namespace FacilityInspection.ViewModels;

public sealed class InspectionEntryPhotoViewModel : ViewModelBase
{
    private readonly Action<InspectionEntryPhotoViewModel>
        _removeRequested;

    public InspectionEntryPhotoViewModel(
        string fileName,
        string relativePath,
        DateTime capturedAtUtc,
        Action<InspectionEntryPhotoViewModel> removeRequested,
        bool isPersisted = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(
            fileName);

        ArgumentException.ThrowIfNullOrWhiteSpace(
            relativePath);

        ArgumentNullException.ThrowIfNull(
            removeRequested);

        FileName =
            fileName.Trim();

        RelativePath =
            relativePath.Trim();

        CapturedAtUtc =
            capturedAtUtc;

        _removeRequested =
            removeRequested;

        IsPersisted =
            isPersisted;

        RemoveCommand =
            new RelayCommand(
                Remove);
    }

    public string FileName { get; }

    public string RelativePath { get; }

    public DateTime CapturedAtUtc { get; }

    /// <summary>
    /// DBに保存済みの写真かどうか。
    /// 保存済みの写真は、画面で削除しても保存するまで実ファイルを消さない。
    /// </summary>
    public bool IsPersisted { get; private set; }

    internal void MarkPersisted()
    {
        IsPersisted = true;
    }

    public IRelayCommand RemoveCommand { get; }

    public InspectionCompletionPhotoData
        ToCompletionData()
    {
        return new InspectionCompletionPhotoData(
            RelativePath,
            CapturedAtUtc);
    }

    private void Remove()
    {
        _removeRequested(this);
    }
}
