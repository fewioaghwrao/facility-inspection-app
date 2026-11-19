using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

/// <summary>
/// 点検入力画面の一時保存・保存済み写真に関するテスト。
/// </summary>
public sealed class InspectionEntryViewModelDraftTests
{
    private static readonly Guid ScheduleId =
        Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static readonly Guid OperatorId =
        Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

    private static readonly Guid ChoiceItemId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid NumericItemId =
        Guid.Parse("10000000-0000-0000-0000-000000000002");


    // ============================================
    // 一時保存
    // ============================================

    [Fact]
    public async Task SaveDraftCommand_必須項目が未入力_保存処理が呼ばれる()
    {
        // Arrange
        IReadOnlyCollection<InspectionCompletionItemData>? savedItems = null;

        var sut =
            CreateViewModel(
                saveDraftAsync: items =>
                {
                    savedItems = items;
                    return Task.CompletedTask;
                });

        await sut.InitializeAsync();

        // Act
        await sut.SaveDraftCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(
            savedItems);

        Assert.Equal(
            2,
            savedItems!.Count);

        Assert.All(
            savedItems,
            x =>
            {
                Assert.Null(x.CheckValue);
                Assert.Null(x.NumericValue);
            });

        Assert.False(
            sut.HasValidationMessage);
    }


    [Fact]
    public async Task SaveDraftCommand_成功_一時保存メッセージが表示される()
    {
        // Arrange
        var sut =
            CreateViewModel();

        await sut.InitializeAsync();

        // Act
        await sut.SaveDraftCommand.ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasDraftStatusMessage);

        Assert.Contains(
            "一時保存しました",
            sut.DraftStatusMessage);
    }


    [Fact]
    public async Task SaveDraftCommand_成功_追加した写真が保存済みになる()
    {
        // Arrange
        var sut =
            CreateViewModel();

        await sut.InitializeAsync();

        var item =
            sut.Items.Single(
                x => x.TemplateItemId == ChoiceItemId);

        item.AddPhoto(
            "photo.jpg",
            "photos/s/i/photo.jpg",
            DateTime.UtcNow);

        Assert.False(
            item.Photos.Single().IsPersisted);

        // Act
        await sut.SaveDraftCommand.ExecuteAsync(null);

        // Assert
        Assert.True(
            item.Photos.Single().IsPersisted);
    }


    [Fact]
    public async Task SaveDraftCommand_数値の形式が不正_保存処理は呼ばれずエラーになる()
    {
        // Arrange
        var called = false;

        var sut =
            CreateViewModel(
                saveDraftAsync: _ =>
                {
                    called = true;
                    return Task.CompletedTask;
                });

        await sut.InitializeAsync();

        sut.Items
            .Single(x => x.TemplateItemId == NumericItemId)
            .NumericText = "abc";

        // Act
        await sut.SaveDraftCommand.ExecuteAsync(null);

        // Assert
        Assert.False(
            called);

        Assert.True(
            sut.HasValidationMessage);

        Assert.False(
            sut.HasDraftStatusMessage);
    }


    [Fact]
    public async Task SaveDraftCommand_保存処理が失敗_エラーメッセージが表示され写真は未保存のまま()
    {
        // Arrange
        var sut =
            CreateViewModel(
                saveDraftAsync: _ =>
                    throw new InvalidOperationException(
                        "DBエラー"));

        await sut.InitializeAsync();

        var item =
            sut.Items.Single(
                x => x.TemplateItemId == ChoiceItemId);

        item.AddPhoto(
            "photo.jpg",
            "photos/s/i/photo.jpg",
            DateTime.UtcNow);

        // Act
        await sut.SaveDraftCommand.ExecuteAsync(null);

        // Assert
        Assert.Contains(
            "一時保存できませんでした",
            sut.ValidationMessage);

        Assert.Contains(
            "DBエラー",
            sut.ValidationMessage);

        Assert.False(
            item.Photos.Single().IsPersisted);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // 保存済み写真
    // ============================================

    [Fact]
    public void ItemViewModel_保存済み写真がある_保存済みとして読み込まれる()
    {
        // Arrange
        var data =
            CreateChoiceItemData(
                photos:
                [
                    new InspectionEntryPhotoData(
                        Guid.NewGuid(),
                        "photos/s/i/saved.jpg",
                        DateTime.UtcNow)
                ]);

        // Act
        var sut =
            new InspectionEntryItemViewModel(
                data);

        // Assert
        var photo =
            Assert.Single(
                sut.Photos);

        Assert.True(
            photo.IsPersisted);

        Assert.Equal(
            "saved.jpg",
            photo.FileName);

        Assert.True(
            sut.HasPhotos);
    }


    [Fact]
    public void ItemViewModel_保存済み写真を削除_一覧から外れ完了データにも含まれない()
    {
        // Arrange
        var sut =
            new InspectionEntryItemViewModel(
                CreateChoiceItemData(
                    photos:
                    [
                        new InspectionEntryPhotoData(
                            Guid.NewGuid(),
                            "photos/s/i/saved.jpg",
                            DateTime.UtcNow)
                    ]));

        // Act
        sut.Photos.Single()
            .RemoveCommand
            .Execute(null);

        var created =
            sut.TryCreateDraftData(
                out var data);

        // Assert
        Assert.True(
            created);

        Assert.Empty(
            sut.Photos);

        Assert.Empty(
            data.Photos);
    }


    [Fact]
    public void ItemViewModel_TryCreateDraftData_必須未入力でも成功する()
    {
        // Arrange
        var sut =
            new InspectionEntryItemViewModel(
                CreateChoiceItemData(
                    checkValue: null));

        // Act
        var draftCreated =
            sut.TryCreateDraftData(
                out _);

        var completionCreated =
            sut.TryCreateCompletionData(
                out _);

        // Assert
        Assert.True(
            draftCreated);

        Assert.False(
            completionCreated);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionEntryViewModel CreateViewModel(
        Func<IReadOnlyCollection<InspectionCompletionItemData>, Task>?
            saveDraftAsync = null)
    {
        return new InspectionEntryViewModel(
            ScheduleId,
            OperatorId,
            () => Task.FromResult(
                CreateEntryData()),
            _ => Task.CompletedTask,
            _ => { },
            () => { },
            saveDraftAsync ??
            (_ => Task.CompletedTask));
    }


    private static InspectionEntryData CreateEntryData()
    {
        return new InspectionEntryData(
            ScheduleId: ScheduleId,
            InspectionId: Guid.NewGuid(),
            ScheduledDate: new DateOnly(2026, 11, 20),
            FactorySiteName: "第1工場",
            LocationName: "コンプレッサー室",
            EquipmentCode: "EQ-001",
            EquipmentName: "コンプレッサー1号",
            TemplateName: "日常点検",
            Status: InspectionStatus.InProgress,
            Items:
            [
                CreateChoiceItemData(
                    checkValue: null),

                new InspectionEntryItemData(
                    TemplateItemId: NumericItemId,
                    DisplayOrder: 2,
                    ItemName: "吐出圧力",
                    InputType: InspectionInputType.Numeric,
                    Unit: "MPa",
                    MinimumValue: 0.5,
                    MaximumValue: 0.9,
                    IsRequired: true,
                    Description: null,
                    CheckValue: null,
                    NumericValue: null,
                    TextValue: null,
                    Comment: null)
            ]);
    }


    private static InspectionEntryItemData CreateChoiceItemData(
        bool? checkValue = true,
        IReadOnlyList<InspectionEntryPhotoData>? photos = null)
    {
        return new InspectionEntryItemData(
            TemplateItemId: ChoiceItemId,
            DisplayOrder: 1,
            ItemName: "異音確認",
            InputType: InspectionInputType.NormalAbnormal,
            Unit: null,
            MinimumValue: null,
            MaximumValue: null,
            IsRequired: true,
            Description: null,
            CheckValue: checkValue,
            NumericValue: null,
            TextValue: null,
            Comment: null,
            Photos: photos);
    }
}
