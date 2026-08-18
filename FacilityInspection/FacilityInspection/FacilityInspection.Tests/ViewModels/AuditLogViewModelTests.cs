using FacilityInspection.Data;
using FacilityInspection.Domain.AuditLogs;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AuditLogViewModelTests
{
    private static readonly DateTime
        BaseUtc =
            new(
                2026,
                8,
                19,
                0,
                0,
                0,
                DateTimeKind.Utc);


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullLoader_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AuditLogViewModel(
                        (Func<
                            Task<
                                IReadOnlyList<
                                    AuditLogListData>>>)null!));

        // Assert
        Assert.Equal(
            "loadAuditLogsAsync",
            exception.ParamName);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_InitializesFiltersAndInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel(
                []);

        // Assert
        Assert.Equal(
            "操作履歴",
            sut.Title);

        Assert.Equal(
            "システム内で実行された主要な操作を時系列で確認できます。",
            sut.Description);

        Assert.Equal(
            Enum.GetValues<AuditActionType>()
                .Length + 1,
            sut.ActionFilterOptions.Count);

        Assert.Equal(
            "すべて",
            sut.ActionFilterOptions[0].Label);

        Assert.Null(
            sut.ActionFilterOptions[0].Value);

        Assert.Same(
            sut.ActionFilterOptions[0],
            sut.SelectedActionFilter);


        Assert.Equal(
            Enum.GetValues<AuditEntityType>()
                .Length + 1,
            sut.EntityFilterOptions.Count);

        Assert.Equal(
            "すべて",
            sut.EntityFilterOptions[0].Label);

        Assert.Null(
            sut.EntityFilterOptions[0].Value);

        Assert.Same(
            sut.EntityFilterOptions[0],
            sut.SelectedEntityFilter);


        Assert.Equal(
            string.Empty,
            sut.SearchText);

        Assert.Empty(
            sut.Items);

        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.True(
            sut.IsEmpty);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.False(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadCommand_LoadsAndSortsItemsByOccurredAtDescending()
    {
        // Arrange
        var oldest =
            CreateRow(
                occurredAtUtc:
                    BaseUtc.AddHours(1),
                operatorName:
                    "古いログ");

        var newest =
            CreateRow(
                occurredAtUtc:
                    BaseUtc.AddHours(3),
                operatorName:
                    "新しいログ");

        var middle =
            CreateRow(
                occurredAtUtc:
                    BaseUtc.AddHours(2),
                operatorName:
                    "中間ログ");

        var sut =
            CreateViewModel(
                [
                    oldest,
                    newest,
                    middle
                ]);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            3,
            sut.Items.Count);

        Assert.Equal(
            "新しいログ",
            sut.Items[0].OperatorName);

        Assert.Equal(
            "中間ログ",
            sut.Items[1].OperatorName);

        Assert.Equal(
            "古いログ",
            sut.Items[2].OperatorName);

        Assert.Equal(
            "3件",
            sut.CountText);

        Assert.False(
            sut.IsEmpty);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);
    }


    [Fact]
    public async Task LoadCommand_WithNoItems_SetsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.False(
            sut.HasNextPage);
    }


    [Fact]
    public async Task LoadCommand_WhenAlreadyLoading_DoesNotInvokeLoader()
    {
        // Arrange
        var callCount =
            0;

        var sut =
            new AuditLogViewModel(
                () =>
                {
                    callCount++;

                    return Task.FromResult<
                        IReadOnlyList<
                            AuditLogListData>>(
                        []);
                });

        sut.IsLoading =
            true;

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            callCount);
    }


    // ============================================
    // Error
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenLoaderThrows_ClearsItemsAndSetsError()
    {
        // Arrange
        var callCount =
            0;

        IReadOnlyList<
            AuditLogListData> successData =
        [
            CreateRow(
                operatorName:
                    "既存ログ")
        ];

        var sut =
            new AuditLogViewModel(
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        return Task.FromResult(
                            successData);
                    }

                    return Task.FromException<
                        IReadOnlyList<
                            AuditLogListData>>(
                        new InvalidOperationException(
                            "テスト読込エラー"));
                });

        await sut.LoadCommand
            .ExecuteAsync(null);

        Assert.Single(
            sut.Items);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "操作履歴を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "テスト読込エラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task LoadCommand_AfterPreviousError_ClearsError()
    {
        // Arrange
        var callCount =
            0;

        var sut =
            new AuditLogViewModel(
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        return Task.FromException<
                            IReadOnlyList<
                                AuditLogListData>>(
                            new InvalidOperationException(
                                "1回目エラー"));
                    }

                    IReadOnlyList<
                        AuditLogListData> data =
                    [
                        CreateRow()
                    ];

                    return Task.FromResult(
                        data);
                });

        await sut.LoadCommand
            .ExecuteAsync(null);

        Assert.True(
            sut.HasError);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Single(
            sut.Items);

        Assert.False(
            sut.IsEmpty);
    }


    // ============================================
    // Action Filter
    // ============================================

    [Fact]
    public async Task ActionFilter_FiltersByActionType()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        actionType:
                            AuditActionType.Approve,
                        operatorName:
                            "承認1"),

                    CreateRow(
                        actionType:
                            AuditActionType.Create,
                        operatorName:
                            "登録"),

                    CreateRow(
                        actionType:
                            AuditActionType.Approve,
                        operatorName:
                            "承認2")
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SelectedActionFilter =
            sut.ActionFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditActionType.Approve);

        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.All(
            sut.Items,
            item =>
                Assert.Equal(
                    AuditActionType.Approve,
                    item.ActionType));

        Assert.Equal(
            "2件",
            sut.CountText);
    }


    // ============================================
    // Entity Filter
    // ============================================

    [Fact]
    public async Task EntityFilter_FiltersByEntityType()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        entityType:
                            AuditEntityType.Inspection),

                    CreateRow(
                        entityType:
                            AuditEntityType.Equipment),

                    CreateRow(
                        entityType:
                            AuditEntityType.Inspection)
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SelectedEntityFilter =
            sut.EntityFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditEntityType.Equipment);

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            AuditEntityType.Equipment,
            sut.Items[0].EntityType);

        Assert.Equal(
            "1件",
            sut.CountText);
    }


    // ============================================
    // Search - Operator
    // ============================================

    [Fact]
    public async Task SearchText_FiltersByOperatorNameIgnoringCaseAndWhitespace()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        operatorName:
                            "ManagerAlpha"),

                    CreateRow(
                        operatorName:
                            "OperatorBeta")
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "  manageralpha  ";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            "ManagerAlpha",
            sut.Items[0].OperatorName);
    }


    // ============================================
    // Search - Action
    // ============================================

    [Fact]
    public async Task SearchText_FiltersByActionTypeText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        actionType:
                            AuditActionType.Approve),

                    CreateRow(
                        actionType:
                            AuditActionType.Create)
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "承認";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            AuditActionType.Approve,
            sut.Items[0].ActionType);
    }


    // ============================================
    // Search - Entity
    // ============================================

    [Fact]
    public async Task SearchText_FiltersByEntityTypeText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        entityType:
                            AuditEntityType
                                .InspectionTemplate),

                    CreateRow(
                        entityType:
                            AuditEntityType.Equipment)
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "点検票テンプレート";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            AuditEntityType
                .InspectionTemplate,
            sut.Items[0].EntityType);
    }


    // ============================================
    // Search - EntityId
    // ============================================

    [Fact]
    public async Task SearchText_FiltersByEntityId()
    {
        // Arrange
        var targetEntityId =
            Guid.Parse(
                "12345678-1111-2222-3333-444444444444");

        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        entityId:
                            targetEntityId),

                    CreateRow(
                        entityId:
                            Guid.Parse(
                                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"))
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "12345678";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            targetEntityId,
            sut.Items[0].EntityId);
    }


    // ============================================
    // Search - Reason
    // ============================================

    [Fact]
    public async Task SearchText_FiltersByReason()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        reason:
                            "圧力異常のため差し戻し"),

                    CreateRow(
                        reason:
                            "定期点検を承認")
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "圧力異常";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            "圧力異常のため差し戻し",
            sut.Items[0].Reason);
    }


    // ============================================
    // Combined Filter
    // ============================================

    [Fact]
    public async Task FiltersAndSearch_AreCombined()
    {
        // Arrange
        var target =
            CreateRow(
                operatorName:
                    "保全管理者A",
                actionType:
                    AuditActionType.Approve,
                entityType:
                    AuditEntityType.Inspection);

        var wrongAction =
            CreateRow(
                operatorName:
                    "保全管理者A",
                actionType:
                    AuditActionType.Create,
                entityType:
                    AuditEntityType.Inspection);

        var wrongEntity =
            CreateRow(
                operatorName:
                    "保全管理者A",
                actionType:
                    AuditActionType.Approve,
                entityType:
                    AuditEntityType.Equipment);

        var wrongOperator =
            CreateRow(
                operatorName:
                    "別管理者",
                actionType:
                    AuditActionType.Approve,
                entityType:
                    AuditEntityType.Inspection);

        var sut =
            CreateViewModel(
                [
                    target,
                    wrongAction,
                    wrongEntity,
                    wrongOperator
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SelectedActionFilter =
            sut.ActionFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditActionType.Approve);

        sut.SelectedEntityFilter =
            sut.EntityFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditEntityType.Inspection);

        sut.SearchText =
            "保全管理者A";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.AuditLogId,
            sut.Items[0].AuditLogId);
    }


    [Fact]
    public async Task SearchText_WhenNoMatch_SetsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        operatorName:
                            "管理者A")
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "存在しない検索文字列";

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);

        Assert.Equal(
            "0件",
            sut.CountText);
    }


    // ============================================
    // Paging
    // ============================================

    [Fact]
    public async Task LoadCommand_WithMoreThanTenItems_CreatesMultiplePages()
    {
        // Arrange
        var data =
            CreateRows(
                12);

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            10,
            sut.Items.Count);

        Assert.Equal(
            "12件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            2,
            sut.TotalPages);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.False(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);
    }


    [Fact]
    public async Task NextAndPreviousPageCommands_ChangePage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateRows(
                    12));

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.NextPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "2 / 2",
            sut.PageText);

        Assert.True(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);


        // Act
        sut.PreviousPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            10,
            sut.Items.Count);

        Assert.False(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);
    }


    [Fact]
    public async Task FilterChange_ResetsCurrentPageToOne()
    {
        // Arrange
        var data =
            new List<AuditLogListData>();

        for (var index = 0;
             index < 12;
             index++)
        {
            data.Add(
                CreateRow(
                    occurredAtUtc:
                        BaseUtc.AddMinutes(
                            index),
                    actionType:
                        AuditActionType.Create));
        }

        data.Add(
            CreateRow(
                occurredAtUtc:
                    BaseUtc.AddMinutes(20),
                actionType:
                    AuditActionType.Approve));

        var sut =
            CreateViewModel(
                data);

        await sut.LoadCommand
            .ExecuteAsync(null);

        sut.NextPageCommand
            .Execute(null);

        Assert.Equal(
            2,
            sut.CurrentPage);

        // Act
        sut.SelectedActionFilter =
            sut.ActionFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditActionType.Create);

        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            2,
            sut.TotalPages);

        Assert.Equal(
            10,
            sut.Items.Count);
    }


    // ============================================
    // Reset Filter
    // ============================================

    [Fact]
    public async Task ResetFilterCommand_ClearsAllFilters()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        operatorName:
                            "管理者A",
                        actionType:
                            AuditActionType.Approve,
                        entityType:
                            AuditEntityType.Inspection),

                    CreateRow(
                        operatorName:
                            "管理者B",
                        actionType:
                            AuditActionType.Create,
                        entityType:
                            AuditEntityType.Equipment),

                    CreateRow(
                        operatorName:
                            "管理者C",
                        actionType:
                            AuditActionType.Delete,
                        entityType:
                            AuditEntityType.Operator)
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        sut.SelectedActionFilter =
            sut.ActionFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditActionType.Approve);

        sut.SelectedEntityFilter =
            sut.EntityFilterOptions
                .Single(x =>
                    x.Value ==
                    AuditEntityType.Inspection);

        sut.SearchText =
            "管理者A";

        Assert.Single(
            sut.Items);

        // Act
        sut.ResetFilterCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            string.Empty,
            sut.SearchText);

        Assert.Same(
            sut.ActionFilterOptions[0],
            sut.SelectedActionFilter);

        Assert.Same(
            sut.EntityFilterOptions[0],
            sut.SelectedEntityFilter);

        Assert.Equal(
            3,
            sut.Items.Count);

        Assert.Equal(
            "3件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.CurrentPage);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequested()
    {
        // Arrange
        var auditLogId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        auditLogId:
                            auditLogId)
                ]);

        Guid? requestedId =
            null;

        sut.DetailRequested =
            id =>
                requestedId =
                    id;

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.Items[0]
            .OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            auditLogId,
            requestedId);
    }


    // ============================================
    // Helpers
    // ============================================

    private static AuditLogViewModel
        CreateViewModel(
            IReadOnlyList<
                AuditLogListData> data)
    {
        return new AuditLogViewModel(
            () =>
                Task.FromResult(
                    data));
    }


    private static IReadOnlyList<
        AuditLogListData>
        CreateRows(
            int count)
    {
        var rows =
            new List<AuditLogListData>();

        for (var index = 0;
             index < count;
             index++)
        {
            rows.Add(
                CreateRow(
                    occurredAtUtc:
                        BaseUtc.AddMinutes(
                            index),

                    operatorName:
                        $"管理者{index + 1}"));
        }

        return rows;
    }


    private static AuditLogListData
        CreateRow(
            Guid? auditLogId = null,
            DateTime? occurredAtUtc = null,
            Guid? operatorId = null,
            string operatorName =
                "保全管理者A",
            AuditActionType actionType =
                AuditActionType.Approve,
            AuditEntityType entityType =
                AuditEntityType.Inspection,
            Guid? entityId = null,
            string? reason =
                "点検内容を確認")
    {
        return new AuditLogListData(
            AuditLogId:
                auditLogId ??
                Guid.NewGuid(),

            OccurredAtUtc:
                occurredAtUtc ??
                BaseUtc,

            OperatorId:
                operatorId ??
                Guid.NewGuid(),

            OperatorName:
                operatorName,

            ActionType:
                actionType,

            EntityType:
                entityType,

            EntityId:
                entityId ??
                Guid.NewGuid(),

            Reason:
                reason);
    }
}