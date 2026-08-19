using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class NotStartedListViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullLoader_ThrowsArgumentNullException()
    {
        // Arrange
        Func<
            Task<
                IReadOnlyList<
                    InspectionListData>>>?
            loader =
                null;


        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new NotStartedListViewModel(
                        loader!));


        // Assert
        Assert.Equal(
            "loadNotStartedAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_DoesNotAutoLoadAndHasExpectedInitialState()
    {
        // Arrange
        var loadCallCount =
            0;


        // Act
        var sut =
            new NotStartedListViewModel(
                () =>
                {
                    loadCallCount++;

                    return EmptyRows();
                });


        // Assert
        Assert.Equal(
            0,
            loadCallCount);

        Assert.Equal(
            "未実施一覧",
            sut.Title);

        Assert.Equal(
            "点検予定のうち、まだ点検が開始されていない項目を一覧表示します。",
            sut.Description);

        Assert.Empty(
            sut.Items);

        Assert.Equal(
            string.Empty,
            sut.SearchText);

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
    public async Task LoadAsync_LoadsItemsAndCount()
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
                CreateRows(
                    3);


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            3,
            sut.Items.Count);

        Assert.Equal(
            "3件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.False(
            sut.IsEmpty);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);


        Assert.Equal(
            "EQ-001",
            sut.Items[0]
                .EquipmentCode);

        Assert.Equal(
            "EQ-002",
            sut.Items[1]
                .EquipmentCode);

        Assert.Equal(
            "EQ-003",
            sut.Items[2]
                .EquipmentCode);
    }


    [Fact]
    public async Task LoadAsync_ReplacesPreviouslyLoadedData()
    {
        // Arrange
        var loadCount =
            0;


        var sut =
            CreateViewModel(
                () =>
                {
                    loadCount++;


                    if (loadCount == 1)
                    {
                        return Task.FromResult<
                            IReadOnlyList<
                                InspectionListData>>(
                        [
                            CreateData(
                                1,
                                equipmentCode:
                                    "OLD-001")
                        ]);
                    }


                    return Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                    [
                        CreateData(
                            2,
                            equipmentCode:
                                "NEW-001"),

                        CreateData(
                            3,
                            equipmentCode:
                                "NEW-002")
                    ]);
                });


        await sut.LoadAsync();


        Assert.Single(
            sut.Items);

        Assert.Equal(
            "OLD-001",
            sut.Items[0]
                .EquipmentCode);


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "2件",
            sut.CountText);

        Assert.Equal(
            "NEW-001",
            sut.Items[0]
                .EquipmentCode);

        Assert.Equal(
            "NEW-002",
            sut.Items[1]
                .EquipmentCode);
    }


    [Fact]
    public async Task LoadAsync_WhileAlreadyLoading_IgnoresSecondRequest()
    {
        // Arrange
        var loadCallCount =
            0;


        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<
                    InspectionListData>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                () =>
                {
                    loadCallCount++;

                    return completionSource.Task;
                });


        // Act
        var firstLoad =
            sut.LoadAsync();


        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsEmpty);


        var secondLoad =
            sut.LoadAsync();


        await secondLoad;


        // Assert
        Assert.Equal(
            1,
            loadCallCount);

        Assert.True(
            sut.IsLoading);


        // Complete first load
        completionSource.SetResult(
            CreateRows(
                1));


        await firstLoad;


        Assert.False(
            sut.IsLoading);

        Assert.Equal(
            1,
            loadCallCount);

        Assert.Single(
            sut.Items);
    }


    // ============================================
    // Error
    // ============================================

    [Fact]
    public async Task LoadAsync_WhenLoaderFails_ClearsDataAndSetsError()
    {
        // Arrange
        var shouldFail =
            false;


        var sut =
            CreateViewModel(
                () =>
                {
                    if (shouldFail)
                    {
                        return Task.FromException<
                            IReadOnlyList<
                                InspectionListData>>(
                            new InvalidOperationException(
                                "未実施読込テストエラー"));
                    }


                    return Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            6));
                });


        await sut.LoadAsync();

        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.NotEmpty(
            sut.Items);


        shouldFail =
            true;


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Empty(
            sut.Items);

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

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "未実施一覧を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "未実施読込テストエラー",
            sut.ErrorMessage);

        Assert.True(
            sut.IsEmpty);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task LoadAsync_AfterFailureThenSuccess_ClearsError()
    {
        // Arrange
        var shouldFail =
            true;


        var sut =
            CreateViewModel(
                () =>
                {
                    if (shouldFail)
                    {
                        return Task.FromException<
                            IReadOnlyList<
                                InspectionListData>>(
                            new InvalidOperationException(
                                "一時エラー"));
                    }


                    return Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                    [
                        CreateData(
                            1)
                    ]);
                });


        await sut.LoadAsync();


        Assert.True(
            sut.HasError);

        Assert.Empty(
            sut.Items);


        // Act
        shouldFail =
            false;

        await sut.LoadAsync();


        // Assert
        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Single(
            sut.Items);

        Assert.Equal(
            "1件",
            sut.CountText);

        Assert.False(
            sut.IsEmpty);
    }


    // ============================================
    // Search
    // ============================================

    [Theory]
    [InlineData(
        "西工場")]
    [InlineData(
        "ポンプ室")]
    [InlineData(
        "PUMP-X1")]
    [InlineData(
        "循環ポンプ")]
    [InlineData(
        "月次点検")]
    [InlineData(
        "山田太郎")]
    public async Task SearchText_FiltersBySupportedFields(
        string keyword)
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
            [
                CreateData(
                    index:
                        1,
                    factorySiteName:
                        "西工場",
                    locationName:
                        "ポンプ室",
                    equipmentCode:
                        "PUMP-X1",
                    equipmentName:
                        "循環ポンプ",
                    templateName:
                        "月次点検",
                    operatorName:
                        "山田太郎"),

                CreateData(
                    index:
                        2,
                    factorySiteName:
                        "東工場",
                    locationName:
                        "コンプレッサー室",
                    equipmentCode:
                        "COMP-Y1",
                    equipmentName:
                        "コンプレッサー",
                    templateName:
                        "日常点検",
                    operatorName:
                        "佐藤花子")
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadAsync();


        // Act
        sut.SearchText =
            keyword;


        // Assert
        var item =
            Assert.Single(
                sut.Items);

        Assert.Equal(
            "PUMP-X1",
            item.EquipmentCode);

        Assert.Equal(
            "1件",
            sut.CountText);

        Assert.Equal(
            1,
            sut.TotalPages);
    }


    [Fact]
    public async Task SearchText_IsCaseInsensitive()
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
            [
                CreateData(
                    1,
                    equipmentCode:
                        "PUMP-A01"),

                CreateData(
                    2,
                    equipmentCode:
                        "COMP-B01")
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadAsync();


        // Act
        sut.SearchText =
            "pump-a01";


        // Assert
        var item =
            Assert.Single(
                sut.Items);

        Assert.Equal(
            "PUMP-A01",
            item.EquipmentCode);
    }


    [Fact]
    public async Task SearchText_TrimsLeadingAndTrailingWhitespace()
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
            [
                CreateData(
                    1,
                    equipmentCode:
                        "PUMP-001"),

                CreateData(
                    2,
                    equipmentCode:
                        "COMP-001")
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadAsync();


        // Act
        sut.SearchText =
            "   PUMP-001   ";


        // Assert
        var item =
            Assert.Single(
                sut.Items);

        Assert.Equal(
            "PUMP-001",
            item.EquipmentCode);

        Assert.Equal(
            "1件",
            sut.CountText);
    }


    [Fact]
    public async Task SearchText_WhenNoMatch_ShowsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            3)));


        await sut.LoadAsync();


        // Act
        sut.SearchText =
            "存在しない設備";


        // Assert
        Assert.Empty(
            sut.Items);

        Assert.Equal(
            "0件",
            sut.CountText);

        Assert.True(
            sut.IsEmpty);

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
    public async Task SearchText_WhenCleared_RestoresAllItems()
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
            [
                CreateData(
                    1,
                    equipmentCode:
                        "PUMP-001"),

                CreateData(
                    2,
                    equipmentCode:
                        "COMP-001"),

                CreateData(
                    3,
                    equipmentCode:
                        "FAN-001")
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadAsync();


        sut.SearchText =
            "PUMP";


        Assert.Single(
            sut.Items);


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

        Assert.False(
            sut.IsEmpty);
    }


    [Fact]
    public async Task SearchText_Change_ResetsCurrentPageToOne()
    {
        // Arrange
        IReadOnlyList<InspectionListData>
            rows =
                Enumerable
                    .Range(
                        1,
                        8)
                    .Select(
                        index =>
                            CreateData(
                                index,
                                equipmentName:
                                    $"対象設備{index}"))
                    .ToArray();


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadAsync();


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


        // Act
        sut.SearchText =
            "対象設備";


        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            5,
            sut.Items.Count);
    }


    // ============================================
    // Paging
    // ============================================

    [Fact]
    public async Task LoadAsync_WithSixItems_CreatesTwoPagesAndShowsFirstFive()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            6)));


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            "6件",
            sut.CountText);

        Assert.Equal(
            2,
            sut.TotalPages);

        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.False(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);


        Assert.Equal(
            "EQ-001",
            sut.Items[0]
                .EquipmentCode);

        Assert.Equal(
            "EQ-005",
            sut.Items[4]
                .EquipmentCode);
    }


    [Fact]
    public async Task NextPageCommand_MovesToNextPage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            7)));


        await sut.LoadAsync();


        // Act
        sut.NextPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Equal(
            "2 / 2",
            sut.PageText);

        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "EQ-006",
            sut.Items[0]
                .EquipmentCode);

        Assert.Equal(
            "EQ-007",
            sut.Items[1]
                .EquipmentCode);

        Assert.True(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);
    }


    [Fact]
    public async Task PreviousPageCommand_MovesToPreviousPage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            7)));


        await sut.LoadAsync();


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


        // Act
        sut.PreviousPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            "EQ-001",
            sut.Items[0]
                .EquipmentCode);

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
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            7)));


        await sut.LoadAsync();


        var firstItem =
            sut.Items[0];


        // Act
        sut.PreviousPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Same(
            firstItem,
            sut.Items[0]);
    }


    [Fact]
    public async Task NextPageCommand_OnLastPage_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        CreateRows(
                            7)));


        await sut.LoadAsync();


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


        var firstItemOnSecondPage =
            sut.Items[0];


        // Act
        sut.NextPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Equal(
            "2 / 2",
            sut.PageText);

        Assert.Same(
            firstItemOnSecondPage,
            sut.Items[0]);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequestedWithScheduleId()
    {
        // Arrange
        var scheduleId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                    [
                        CreateData(
                            index:
                                1,
                            scheduleId:
                                scheduleId)
                    ]));


        Guid?
            capturedScheduleId =
                null;


        sut.DetailRequested =
            id =>
                capturedScheduleId =
                    id;


        await sut.LoadAsync();


        var item =
            Assert.Single(
                sut.Items);


        // Act
        item.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            scheduleId,
            capturedScheduleId);
    }


    [Fact]
    public async Task ItemOpenDetailCommand_WithEmptyScheduleId_DoesNotRaiseDetailRequested()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                    [
                        CreateData(
                            index:
                                1,
                            scheduleId:
                                Guid.Empty)
                    ]));


        var detailCallCount =
            0;


        sut.DetailRequested =
            _ =>
                detailCallCount++;


        await sut.LoadAsync();


        var item =
            Assert.Single(
                sut.Items);


        // Act
        item.OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            0,
            detailCallCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static NotStartedListViewModel
        CreateViewModel(
            Func<
                Task<
                    IReadOnlyList<
                        InspectionListData>>>?
                loadNotStartedAsync = null)
    {
        return new NotStartedListViewModel(
            loadNotStartedAsync ??
            EmptyRows);
    }


    private static Task<
        IReadOnlyList<InspectionListData>>
        EmptyRows()
    {
        return Task.FromResult<
            IReadOnlyList<
                InspectionListData>>(
            []);
    }


    private static IReadOnlyList<
        InspectionListData>
        CreateRows(
            int count,
            int startIndex = 1)
    {
        return Enumerable
            .Range(
                startIndex,
                count)
            .Select(
                index =>
                    CreateData(
                        index))
            .ToArray();
    }


    private static InspectionListData
        CreateData(
            int index,
            Guid? scheduleId = null,
            DateOnly? scheduledDate = null,
            string factorySiteName = "第1工場",
            string locationName = "設備エリア",
            string? equipmentCode = null,
            string? equipmentName = null,
            string templateName = "日常点検",
            string operatorName = "点検担当者A")
    {
        return new InspectionListData(
            ScheduleId:
                scheduleId ??
                Guid.Parse(
                    $"00000000-0000-0000-0000-{index:D12}"),

            InspectionId:
                null,

            ScheduledDate:
                scheduledDate ??
                new DateOnly(
                    2026,
                    8,
                    20),

            FactorySiteName:
                factorySiteName,

            LocationName:
                locationName,

            EquipmentCode:
                equipmentCode ??
                $"EQ-{index:000}",

            EquipmentName:
                equipmentName ??
                $"設備{index}",

            TemplateName:
                templateName,

            OperatorName:
                operatorName,

            Status:
                InspectionStatus.NotStarted,

            ResultCount:
                0,

            AbnormalCount:
                0,

            PhotoCount:
                0);
    }
}