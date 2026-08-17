using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AbnormalListViewModelTests
{
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
                    new AbnormalListViewModel(
                        (Func<
                            Task<
                                IReadOnlyList<
                                    AbnormalResultListData>>>)null!));

        // Assert
        Assert.Equal(
            "loadAbnormalResultsAsync",
            exception.ParamName);
    }


    // ============================================
    // 初期状態
    // ============================================

    [Fact]
    public void Constructor_SetsInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel([]);

        // Assert
        Assert.Equal(
            "異常一覧",
            sut.Title);

        Assert.Equal(
            "点検結果で異常と判定された項目を一覧表示します。",
            sut.Description);

        Assert.Empty(
            sut.Items);

        Assert.Equal(
            string.Empty,
            sut.SearchText);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "0件",
            sut.CountText);

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

        Assert.True(
            sut.IsEmpty);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadCommand_LoadsItems()
    {
        // Arrange
        var data =
            new[]
            {
                CreateSource(
                    equipmentCode: "EQ-001"),

                CreateSource(
                    equipmentCode: "EQ-002"),

                CreateSource(
                    equipmentCode: "EQ-003")
            };

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            3,
            sut.Items.Count);

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
            CreateViewModel([]);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.True(
            sut.IsEmpty);
    }


    [Fact]
    public async Task LoadCommand_WhenLoaderThrows_SetsErrorMessage()
    {
        // Arrange
        var sut =
            new AbnormalListViewModel(
                () =>
                    throw new InvalidOperationException(
                        "テストエラー"));

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "異常一覧を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task LoadCommand_AfterPreviousError_ClearsErrorMessage()
    {
        // Arrange
        var callCount =
            0;

        IReadOnlyList<
            AbnormalResultListData>
            successData =
            [
                CreateSource()
            ];

        var sut =
            new AbnormalListViewModel(
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        throw new InvalidOperationException(
                            "1回目エラー");
                    }

                    return Task.FromResult(
                        successData);
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
    }


    // ============================================
    // Paging
    // ============================================

    [Fact]
    public async Task LoadCommand_WithFiveItems_ShowsSinglePage()
    {
        // Arrange
        var data =
            CreateItems(
                5);

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            5,
            sut.Items.Count);

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


    [Fact]
    public async Task LoadCommand_WithSixItems_ShowsFirstFiveItems()
    {
        // Arrange
        var data =
            CreateItems(
                6);

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            "6件",
            sut.CountText);

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
    public async Task NextPageCommand_MovesToNextPage()
    {
        // Arrange
        var data =
            CreateItems(
                6);

        var sut =
            CreateViewModel(
                data);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.NextPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Single(
            sut.Items);

        Assert.Equal(
            "2 / 2",
            sut.PageText);

        Assert.True(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);
    }


    [Fact]
    public async Task PreviousPageCommand_MovesToPreviousPage()
    {
        // Arrange
        var data =
            CreateItems(
                6);

        var sut =
            CreateViewModel(
                data);

        await sut.LoadCommand
            .ExecuteAsync(null);

        sut.NextPageCommand
            .Execute(null);

        // Act
        sut.PreviousPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.False(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);
    }


    [Fact]
    public async Task PreviousPageCommand_OnFirstPage_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateItems(
                    6));

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.PreviousPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            5,
            sut.Items.Count);
    }


    [Fact]
    public async Task NextPageCommand_OnLastPage_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateItems(
                    6));

        await sut.LoadCommand
            .ExecuteAsync(null);

        sut.NextPageCommand
            .Execute(null);

        // Act
        sut.NextPageCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Single(
            sut.Items);
    }


    [Fact]
    public async Task LoadCommand_WithElevenItems_CalculatesThreePages()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateItems(
                    11));

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            3,
            sut.TotalPages);

        Assert.Equal(
            "1 / 3",
            sut.PageText);

        Assert.Equal(
            5,
            sut.Items.Count);
    }


    // ============================================
    // Search
    // ============================================

    [Theory]
    [InlineData("第2工場")]
    [InlineData("第2エリア")]
    [InlineData("EQ-999")]
    [InlineData("テストポンプ")]
    [InlineData("圧力確認")]
    [InlineData("田中")]
    [InlineData("要交換")]
    public async Task SearchText_FiltersBySupportedFields(
        string keyword)
    {
        // Arrange
        var target =
            CreateSource(
                factorySiteName:
                    "第2工場",
                locationName:
                    "第2エリア",
                equipmentCode:
                    "EQ-999",
                equipmentName:
                    "テストポンプ",
                itemName:
                    "圧力確認",
                operatorName:
                    "田中",
                comment:
                    "要交換");

        var other =
            CreateSource(
                factorySiteName:
                    "第1工場",
                locationName:
                    "製造エリア",
                equipmentCode:
                    "EQ-001",
                equipmentName:
                    "コンプレッサー",
                itemName:
                    "異音確認",
                operatorName:
                    "点検担当者A",
                comment:
                    "異常あり");

        var sut =
            CreateViewModel(
                [
                    target,
                    other
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            keyword;

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.ResultId,
            sut.Items[0].ResultId);

        Assert.Equal(
            "1件",
            sut.CountText);
    }


    [Fact]
    public async Task SearchText_IsCaseInsensitive()
    {
        // Arrange
        var target =
            CreateSource(
                equipmentCode:
                    "PUMP-ABC");

        var other =
            CreateSource(
                equipmentCode:
                    "EQ-001");

        var sut =
            CreateViewModel(
                [
                    target,
                    other
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "pump-abc";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.ResultId,
            sut.Items[0].ResultId);
    }


    [Fact]
    public async Task SearchText_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        var target =
            CreateSource(
                equipmentName:
                    "コンプレッサー");

        var other =
            CreateSource(
                equipmentName:
                    "循環ポンプ");

        var sut =
            CreateViewModel(
                [
                    target,
                    other
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "  コンプレッサー  ";

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.ResultId,
            sut.Items[0].ResultId);
    }


    [Fact]
    public async Task SearchText_WithNoMatch_ShowsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateSource(
                        equipmentName:
                            "コンプレッサー"),

                    CreateSource(
                        equipmentName:
                            "ポンプ")
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "存在しない設備";

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.True(
            sut.IsEmpty);

        Assert.False(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);
    }


    [Fact]
    public async Task SearchText_WithEmptyString_ShowsAllItems()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateItems(
                    3));

        await sut.LoadCommand
            .ExecuteAsync(null);

        sut.SearchText =
            "EQ-001";

        // Act
        sut.SearchText =
            string.Empty;

        // Assert
        Assert.Equal(
            3,
            sut.Items.Count);

        Assert.Equal(
            "3件",
            sut.CountText);
    }


    [Fact]
    public async Task SearchText_WithWhitespaceOnly_ShowsAllItems()
    {
        // Arrange
        var sut =
            CreateViewModel(
                CreateItems(
                    3));

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "   ";

        // Assert
        Assert.Equal(
            3,
            sut.Items.Count);

        Assert.Equal(
            "3件",
            sut.CountText);
    }


    [Fact]
    public async Task SearchText_DoesNotFailWhenCommentIsNull()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateSource(
                        equipmentCode:
                            "EQ-001",
                        comment:
                            null)
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.SearchText =
            "存在しない文字";

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);
    }


    [Fact]
    public async Task SearchText_ResetsCurrentPageToOne()
    {
        // Arrange
        var data =
            CreateItems(
                6);

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
        sut.SearchText =
            "EQ";

        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);
    }


    [Fact]
    public async Task SearchText_RecalculatesPageCount()
    {
        // Arrange
        var data =
            Enumerable.Range(
                    1,
                    6)
                .Select(index =>
                    CreateSource(
                        equipmentCode:
                            $"EQ-{index:000}",
                        equipmentName:
                            index <= 2
                                ? "検索対象設備"
                                : "通常設備"))
                .ToList();

        var sut =
            CreateViewModel(
                data);

        await sut.LoadCommand
            .ExecuteAsync(null);

        Assert.Equal(
            2,
            sut.TotalPages);

        // Act
        sut.SearchText =
            "検索対象";

        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "2件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);
    }


    // ============================================
    // DetailRequested
    // ============================================

    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequested()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        var source =
            CreateSource(
                scheduleId:
                    scheduleId);

        var sut =
            CreateViewModel(
                [
                    source
                ]);

        Guid? requestedScheduleId =
            null;

        sut.DetailRequested =
            id =>
                requestedScheduleId = id;

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.Items[0]
            .OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            scheduleId,
            requestedScheduleId);
    }


    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequestedOnce()
    {
        // Arrange
        var source =
            CreateSource();

        var sut =
            CreateViewModel(
                [
                    source
                ]);

        var callCount =
            0;

        sut.DetailRequested =
            _ =>
                callCount++;

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        sut.Items[0]
            .OpenDetailCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public async Task ItemOpenDetailCommand_WithNoSubscriber_DoesNotThrow()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateSource()
                ]);

        await sut.LoadCommand
            .ExecuteAsync(null);

        // Act
        var exception =
            Record.Exception(
                () =>
                    sut.Items[0]
                        .OpenDetailCommand
                        .Execute(null));

        // Assert
        Assert.Null(
            exception);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static AbnormalListViewModel
        CreateViewModel(
            IReadOnlyList<
                AbnormalResultListData> data)
    {
        return new AbnormalListViewModel(
            () =>
                Task.FromResult(
                    data));
    }


    private static IReadOnlyList<
        AbnormalResultListData>
        CreateItems(
            int count)
    {
        return Enumerable.Range(
                1,
                count)
            .Select(index =>
                CreateSource(
                    equipmentCode:
                        $"EQ-{index:000}",
                    equipmentName:
                        $"設備{index}",
                    itemName:
                        $"点検項目{index}",
                    comment:
                        $"コメント{index}"))
            .ToList();
    }


    private static AbnormalResultListData
        CreateSource(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            Guid? resultId = null,
            string factorySiteName =
                "第1工場",
            string locationName =
                "製造エリア",
            string equipmentCode =
                "EQ-001",
            string equipmentName =
                "コンプレッサー",
            string templateName =
                "日常点検",
            string operatorName =
                "点検担当者A",
            InspectionStatus inspectionStatus =
                InspectionStatus.Completed,
            int displayOrder = 1,
            string itemName =
                "異音確認",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            bool? checkValue = false,
            decimal? numericValue = null,
            string? textValue = null,
            string? unit = null,
            string? comment =
                "異音あり",
            int photoCount = 1)
    {
        return new AbnormalResultListData(
            ScheduleId:
                scheduleId ??
                Guid.NewGuid(),

            InspectionId:
                inspectionId ??
                Guid.NewGuid(),

            ResultId:
                resultId ??
                Guid.NewGuid(),

            ScheduledDate:
                new DateOnly(
                    2026,
                    8,
                    18),

            FactorySiteName:
                factorySiteName,

            LocationName:
                locationName,

            EquipmentCode:
                equipmentCode,

            EquipmentName:
                equipmentName,

            TemplateName:
                templateName,

            OperatorName:
                operatorName,

            InspectionStatus:
                inspectionStatus,

            DisplayOrder:
                displayOrder,

            ItemName:
                itemName,

            InputType:
                inputType,

            CheckValue:
                checkValue,

            NumericValue:
                numericValue,

            TextValue:
                textValue,

            Unit:
                unit,

            Comment:
                comment,

            PhotoCount:
                photoCount);
    }
}
