using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

/// <summary>
/// 点検入力画面の未保存変更の確認（画面離脱ガード）に関するテスト。
/// </summary>
public sealed class InspectionEntryViewModelLeaveTests
{
    private static readonly Guid ScheduleId =
        Guid.Parse("11111111-2222-3333-4444-555555555555");

    private static readonly Guid OperatorId =
        Guid.Parse("AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");

    private static readonly Guid ChoiceItemId =
        Guid.Parse("10000000-0000-0000-0000-000000000001");

    private static readonly Guid NumericItemId =
        Guid.Parse("10000000-0000-0000-0000-000000000002");

    private int _backCount;

    private int _cleanupCount;

    private int _saveDraftCount;

    private Func<IReadOnlyCollection<InspectionCompletionItemData>, Task>
        _saveDraftAsync =
            _ => Task.CompletedTask;


    // ============================================
    // 未保存変更の判定
    // ============================================

    [Fact]
    public async Task HasUnsavedChanges_読み込み直後_falseになる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        Assert.False(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task HasUnsavedChanges_値を変更_trueになる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        Assert.True(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task HasUnsavedChanges_値を元に戻す_falseになる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        var choice =
            GetChoiceItem(sut);

        choice.CheckValue = false;
        choice.CheckValue = true;

        Assert.False(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task HasUnsavedChanges_写真を追加_trueになる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetChoiceItem(sut).AddPhoto(
            "photo.jpg",
            "photos/s/i/photo.jpg",
            DateTime.UtcNow);

        Assert.True(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task HasUnsavedChanges_一時保存後_falseになる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        await sut.SaveDraftCommand.ExecuteAsync(null);

        Assert.False(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task HasUnsavedChanges_一時保存に失敗_trueのまま()
    {
        _saveDraftAsync =
            _ => throw new InvalidOperationException("DBエラー");

        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        await sut.SaveDraftCommand.ExecuteAsync(null);

        Assert.True(
            sut.HasUnsavedChanges);
    }


    // ============================================
    // 戻る
    // ============================================

    [Fact]
    public async Task BackCommand_変更なし_確認せずに戻る()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        sut.BackCommand.Execute(null);

        Assert.False(
            sut.IsLeaveConfirmVisible);

        Assert.Equal(
            1,
            _backCount);
    }


    [Fact]
    public async Task BackCommand_変更あり_確認ダイアログを表示し戻らない()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        sut.BackCommand.Execute(null);

        Assert.True(
            sut.IsLeaveConfirmVisible);

        Assert.Equal(
            0,
            _backCount);

        Assert.Equal(
            0,
            _cleanupCount);
    }


    [Fact]
    public async Task CancelLeaveCommand_ダイアログを閉じ画面に留まる()
    {
        var sut =
            await CreateDirtyViewModelWithConfirmAsync();

        sut.CancelLeaveCommand.Execute(null);

        Assert.False(
            sut.IsLeaveConfirmVisible);

        Assert.Equal(
            0,
            _backCount);

        // 入力内容は残っている
        Assert.True(
            sut.HasUnsavedChanges);
    }


    [Fact]
    public async Task DiscardAndLeaveCommand_未保存写真を片付けて戻る()
    {
        var sut =
            await CreateDirtyViewModelWithConfirmAsync();

        sut.DiscardAndLeaveCommand.Execute(null);

        Assert.False(
            sut.IsLeaveConfirmVisible);

        Assert.Equal(
            sut.Items.Count,
            _cleanupCount);

        Assert.Equal(
            1,
            _backCount);

        Assert.Equal(
            0,
            _saveDraftCount);
    }


    [Fact]
    public async Task SaveDraftAndLeaveCommand_一時保存してから戻る()
    {
        IReadOnlyCollection<InspectionCompletionItemData>? savedItems = null;

        _saveDraftAsync =
            items =>
            {
                savedItems = items;
                return Task.CompletedTask;
            };

        var sut =
            await CreateDirtyViewModelWithConfirmAsync();

        await sut.SaveDraftAndLeaveCommand.ExecuteAsync(null);

        Assert.NotNull(
            savedItems);

        Assert.Equal(
            0.7m,
            savedItems!
                .Single(x => x.TemplateItemId == NumericItemId)
                .NumericValue);

        Assert.False(
            sut.IsLeaveConfirmVisible);

        Assert.Equal(
            1,
            _backCount);
    }


    [Fact]
    public async Task SaveDraftAndLeaveCommand_保存に失敗_画面に留まりエラーを表示する()
    {
        _saveDraftAsync =
            _ => throw new InvalidOperationException("DBエラー");

        var sut =
            await CreateDirtyViewModelWithConfirmAsync();

        await sut.SaveDraftAndLeaveCommand.ExecuteAsync(null);

        Assert.True(
            sut.IsLeaveConfirmVisible);

        Assert.True(
            sut.HasLeaveError);

        Assert.Contains(
            "DBエラー",
            sut.LeaveErrorMessage);

        Assert.Equal(
            0,
            _backCount);

        Assert.Equal(
            0,
            _cleanupCount);
    }


    [Fact]
    public async Task SaveDraftAndLeaveCommand_入力形式が不正_保存せず画面に留まる()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "abc";

        sut.BackCommand.Execute(null);

        await sut.SaveDraftAndLeaveCommand.ExecuteAsync(null);

        Assert.Equal(
            0,
            _saveDraftCount);

        Assert.True(
            sut.IsLeaveConfirmVisible);

        Assert.True(
            sut.HasLeaveError);

        Assert.Equal(
            0,
            _backCount);
    }


    // ============================================
    // シェル・ウィンドウからの離脱要求
    // ============================================

    [Fact]
    public async Task RequestLeave_変更あり_確認後に指定した処理を実行する()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        var proceeded = 0;

        sut.RequestLeave(
            () => proceeded++);

        Assert.Equal(
            0,
            proceeded);

        sut.DiscardAndLeaveCommand.Execute(null);

        Assert.Equal(
            1,
            proceeded);

        // 戻る処理（カレンダーへ戻る）は呼ばれない
        Assert.Equal(
            0,
            _backCount);
    }


    [Fact]
    public async Task RequestLeave_確認ダイアログ表示中に再度要求_無視される()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        var first = 0;
        var second = 0;

        sut.RequestLeave(() => first++);
        sut.RequestLeave(() => second++);

        sut.DiscardAndLeaveCommand.Execute(null);

        Assert.Equal(
            1,
            first);

        Assert.Equal(
            0,
            second);
    }


    // ============================================
    // Helper
    // ============================================

    private async Task<InspectionEntryViewModel>
        CreateDirtyViewModelWithConfirmAsync()
    {
        var sut =
            await CreateInitializedViewModelAsync();

        GetNumericItem(sut).NumericText = "0.7";

        sut.BackCommand.Execute(null);

        Assert.True(
            sut.IsLeaveConfirmVisible);

        return sut;
    }


    private async Task<InspectionEntryViewModel>
        CreateInitializedViewModelAsync()
    {
        var sut =
            new InspectionEntryViewModel(
                ScheduleId,
                OperatorId,
                () => Task.FromResult(
                    CreateEntryData()),
                _ => Task.CompletedTask,
                _ => _cleanupCount++,
                () => _backCount++,
                items =>
                {
                    _saveDraftCount++;
                    return _saveDraftAsync(items);
                });

        await sut.InitializeAsync();

        return sut;
    }


    private static InspectionEntryItemViewModel GetChoiceItem(
        InspectionEntryViewModel sut)
    {
        return sut.Items.Single(
            x => x.TemplateItemId == ChoiceItemId);
    }


    private static InspectionEntryItemViewModel GetNumericItem(
        InspectionEntryViewModel sut)
    {
        return sut.Items.Single(
            x => x.TemplateItemId == NumericItemId);
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
                new InspectionEntryItemData(
                    TemplateItemId: ChoiceItemId,
                    DisplayOrder: 1,
                    ItemName: "異音確認",
                    InputType: InspectionInputType.NormalAbnormal,
                    Unit: null,
                    MinimumValue: null,
                    MaximumValue: null,
                    IsRequired: true,
                    Description: null,
                    CheckValue: true,
                    NumericValue: null,
                    TextValue: null,
                    Comment: null),

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
}
