using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionStatusViewModelTests
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
                    new InspectionStatusViewModel(
                        (Func<
                            Task<
                                IReadOnlyList<
                                    InspectionListData>>>)
                        null!));


        // Assert
        Assert.Equal(
            "loadInspectionsAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_InitializesFiltersAndInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel();


        // Assert
        Assert.Equal(
            "点検実施状況",
            sut.Title);

        Assert.Equal(
            "点検の実施状況、異常件数、登録写真を確認します。",
            sut.Description);


        Assert.Equal(
            6,
            sut.StatusFilters.Count);


        Assert.Equal(
            "すべて",
            sut.StatusFilters[0]
                .DisplayName);

        Assert.Null(
            sut.StatusFilters[0]
                .Status);


        Assert.Equal(
            "未実施",
            sut.StatusFilters[1]
                .DisplayName);

        Assert.Equal(
            InspectionStatus.NotStarted,
            sut.StatusFilters[1]
                .Status);


        Assert.Equal(
            "実施中",
            sut.StatusFilters[2]
                .DisplayName);

        Assert.Equal(
            InspectionStatus.InProgress,
            sut.StatusFilters[2]
                .Status);


        Assert.Equal(
            "完了・承認待ち",
            sut.StatusFilters[3]
                .DisplayName);

        Assert.Equal(
            InspectionStatus.Completed,
            sut.StatusFilters[3]
                .Status);


        Assert.Equal(
            "承認済み",
            sut.StatusFilters[4]
                .DisplayName);

        Assert.Equal(
            InspectionStatus.Approved,
            sut.StatusFilters[4]
                .Status);


        Assert.Equal(
            "差し戻し",
            sut.StatusFilters[5]
                .DisplayName);

        Assert.Equal(
            InspectionStatus.Returned,
            sut.StatusFilters[5]
                .Status);


        Assert.Same(
            sut.StatusFilters[0],
            sut.SelectedStatusFilter);


        Assert.Empty(
            sut.Items);

        Assert.Equal(
            string.Empty,
            sut.SearchText);

        Assert.False(
            sut.IsLoading);

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
    public async Task LoadCommand_WhenSuccessful_LoadsFirstPage()
    {
        // Arrange
        var rows =
            CreateRows(
                6);


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsEmpty);


        /*
         * PageSize = 5
         */
        Assert.Equal(
            5,
            sut.Items.Count);


        /*
         * CountTextは現在ページではなく
         * フィルター後の総件数。
         */
        Assert.Equal(
            "6件",
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


        Assert.Equal(
            rows[0].ScheduleId,
            sut.Items[0].ScheduleId);

        Assert.Equal(
            rows[4].ScheduleId,
            sut.Items[4].ScheduleId);
    }


    [Fact]
    public async Task LoadCommand_WhileLoading_SetsLoadingState()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<
                    InspectionListData>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                () =>
                    completionSource.Task);


        // Act
        var loadTask =
            sut.LoadCommand
                .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsEmpty);


        completionSource.SetResult(
            CreateRows(
                1));


        await loadTask;


        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.IsEmpty);

        Assert.Single(
            sut.Items);
    }


    [Fact]
    public async Task LoadCommand_WhenLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromException<
                        IReadOnlyList<
                            InspectionListData>>(
                        new InvalidOperationException(
                            "読込テストエラー")));


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検実施状況を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "読込テストエラー",
            sut.ErrorMessage);

        Assert.True(
            sut.IsEmpty);
    }


    [Fact]
    public async Task LoadCommand_AfterFailure_CanRecover()
    {
        // Arrange
        var callCount =
            0;


        var sut =
            CreateViewModel(
                () =>
                {
                    callCount++;


                    if (callCount == 1)
                    {
                        return Task.FromException<
                            IReadOnlyList<
                                InspectionListData>>(
                            new InvalidOperationException(
                                "一時エラー"));
                    }


                    return Task.FromResult(
                        CreateRows(
                            2));
                });


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.HasError);


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            callCount);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "2件",
            sut.CountText);

        Assert.False(
            sut.IsEmpty);
    }


    // ============================================
    // Status Filter
    // ============================================

    [Fact]
    public async Task SelectedStatusFilter_FiltersByStatus()
    {
        // Arrange
        var approved1 =
            CreateData(
                index:
                    1,
                status:
                    InspectionStatus.Approved);

        var inProgress =
            CreateData(
                index:
                    2,
                status:
                    InspectionStatus.InProgress);

        var approved2 =
            CreateData(
                index:
                    3,
                status:
                    InspectionStatus.Approved);


        IReadOnlyList<
            InspectionListData> rows =
            [
                approved1,
                inProgress,
                approved2
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        var approvedFilter =
            sut.StatusFilters
                .Single(
                    x =>
                        x.Status ==
                        InspectionStatus.Approved);


        // Act
        sut.SelectedStatusFilter =
            approvedFilter;


        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "2件",
            sut.CountText);


        Assert.All(
            sut.Items,
            item =>
                Assert.Equal(
                    InspectionStatus.Approved,
                    item.Status));


        Assert.Contains(
            sut.Items,
            x =>
                x.ScheduleId ==
                approved1.ScheduleId);

        Assert.Contains(
            sut.Items,
            x =>
                x.ScheduleId ==
                approved2.ScheduleId);
    }


    // ============================================
    // Search
    // ============================================

    [Theory]
    [InlineData(
        "northplant")]
    [InlineData(
        "pumproom")]
    [InlineData(
        "eq-alpha")]
    [InlineData(
        "maincompressor")]
    [InlineData(
        "monthlycheck")]
    [InlineData(
        "tanaka")]
    public async Task SearchText_FiltersAcrossAllSearchFieldsCaseInsensitively(
        string keyword)
    {
        // Arrange
        var target =
            CreateData(
                index:
                    1,
                factorySiteName:
                    "NorthPlant",
                locationName:
                    "PumpRoom",
                equipmentCode:
                    "EQ-ALPHA",
                equipmentName:
                    "MainCompressor",
                templateName:
                    "MonthlyCheck",
                operatorName:
                    "Tanaka");


        var other =
            CreateData(
                index:
                    2,
                factorySiteName:
                    "SouthPlant",
                locationName:
                    "Warehouse",
                equipmentCode:
                    "PUMP-002",
                equipmentName:
                    "CoolingPump",
                templateName:
                    "DailyInspection",
                operatorName:
                    "Suzuki");


        IReadOnlyList<
            InspectionListData> rows =
            [
                target,
                other
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Act
        sut.SearchText =
            keyword;


        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.ScheduleId,
            sut.Items[0].ScheduleId);

        Assert.Equal(
            "1件",
            sut.CountText);
    }


    [Fact]
    public async Task SearchText_TrimsKeyword()
    {
        // Arrange
        var target =
            CreateData(
                index:
                    1,
                equipmentCode:
                    "EQ-001");


        var other =
            CreateData(
                index:
                    2,
                equipmentCode:
                    "PUMP-002");


        IReadOnlyList<
            InspectionListData> rows =
            [
                target,
                other
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Act
        sut.SearchText =
            "   EQ-001   ";


        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            target.ScheduleId,
            sut.Items[0].ScheduleId);
    }


    [Fact]
    public async Task SearchText_WhenNothingMatches_SetsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        CreateRows(
                            3)));


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Act
        sut.SearchText =
            "__NO_MATCH__";


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


    // ============================================
    // Search + Status
    // ============================================

    [Fact]
    public async Task SearchAndStatusFilter_AreCombinedWithAndCondition()
    {
        // Arrange
        var expected =
            CreateData(
                index:
                    1,
                equipmentCode:
                    "EQ-001",
                status:
                    InspectionStatus.Approved);


        var sameKeywordWrongStatus =
            CreateData(
                index:
                    2,
                equipmentCode:
                    "EQ-001",
                status:
                    InspectionStatus.InProgress);


        var sameStatusWrongKeyword =
            CreateData(
                index:
                    3,
                equipmentCode:
                    "PUMP-003",
                status:
                    InspectionStatus.Approved);


        IReadOnlyList<
            InspectionListData> rows =
            [
                expected,
                sameKeywordWrongStatus,
                sameStatusWrongKeyword
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Act
        sut.SearchText =
            "EQ-001";


        sut.SelectedStatusFilter =
            sut.StatusFilters
                .Single(
                    x =>
                        x.Status ==
                        InspectionStatus.Approved);


        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            expected.ScheduleId,
            sut.Items[0].ScheduleId);

        Assert.Equal(
            "1件",
            sut.CountText);
    }


    // ============================================
    // Paging
    // ============================================

    [Fact]
    public async Task NextPageCommand_MovesThroughPages()
    {
        // Arrange
        var rows =
            CreateRows(
                11);


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            3,
            sut.TotalPages);

        Assert.Equal(
            5,
            sut.Items.Count);


        // Act - page 2
        sut.NextPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            sut.CurrentPage);

        Assert.Equal(
            "2 / 3",
            sut.PageText);

        Assert.True(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            rows[5].ScheduleId,
            sut.Items[0].ScheduleId);


        // Act - page 3
        sut.NextPageCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            3,
            sut.CurrentPage);

        Assert.Equal(
            "3 / 3",
            sut.PageText);

        Assert.True(
            sut.HasPreviousPage);

        Assert.False(
            sut.HasNextPage);

        Assert.Single(
            sut.Items);

        Assert.Equal(
            rows[10].ScheduleId,
            sut.Items[0].ScheduleId);


        /*
         * ページが変わっても総件数は11件。
         */
        Assert.Equal(
            "11件",
            sut.CountText);
    }


    [Fact]
    public async Task PreviousPageCommand_MovesToPreviousPage()
    {
        // Arrange
        var rows =
            CreateRows(
                6);


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


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

        Assert.False(
            sut.HasPreviousPage);

        Assert.True(
            sut.HasNextPage);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            rows[0].ScheduleId,
            sut.Items[0].ScheduleId);
    }


    [Fact]
    public async Task PreviousPageCommand_OnFirstPage_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        CreateRows(
                            6)));


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
            "1 / 2",
            sut.PageText);
    }


    [Fact]
    public async Task NextPageCommand_OnLastPage_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        CreateRows(
                            6)));


        await sut.LoadCommand
            .ExecuteAsync(null);


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


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

        Assert.False(
            sut.HasNextPage);
    }


    // ============================================
    // Filter resets page
    // ============================================

    [Fact]
    public async Task SearchTextChanged_WhenOnSecondPage_ReturnsToFirstPage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        CreateRows(
                            6)));


        await sut.LoadCommand
            .ExecuteAsync(null);


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


        // Act
        sut.SearchText =
            "EQ-";


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


    [Fact]
    public async Task StatusFilterChanged_WhenOnSecondPage_ReturnsToFirstPage()
    {
        // Arrange
        var rows =
            Enumerable
                .Range(
                    1,
                    6)
                .Select(
                    i =>
                        CreateData(
                            index:
                                i,
                            status:
                                InspectionStatus.Approved))
                .ToList();


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult<
                        IReadOnlyList<
                            InspectionListData>>(
                        rows));


        await sut.LoadCommand
            .ExecuteAsync(null);


        sut.NextPageCommand
            .Execute(null);


        Assert.Equal(
            2,
            sut.CurrentPage);


        // Act
        sut.SelectedStatusFilter =
            sut.StatusFilters
                .Single(
                    x =>
                        x.Status ==
                        InspectionStatus.Approved);


        // Assert
        Assert.Equal(
            1,
            sut.CurrentPage);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            "6件",
            sut.CountText);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequestedWithScheduleId()
    {
        // Arrange
        var row =
            CreateData(
                index:
                    1);


        IReadOnlyList<
            InspectionListData> rows =
            [
                row
            ];


        var sut =
            CreateViewModel(
                () =>
                    Task.FromResult(
                        rows));


        Guid?
            requestedScheduleId =
                null;


        sut.DetailRequested =
            scheduleId =>
                requestedScheduleId =
                    scheduleId;


        await sut.LoadCommand
            .ExecuteAsync(null);


        // Act
        sut.Items[0]
            .OpenDetailCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            row.ScheduleId,
            requestedScheduleId);
    }


    // ============================================
    // Helper
    // ============================================

    private static InspectionStatusViewModel
        CreateViewModel(
            Func<
                Task<
                    IReadOnlyList<
                        InspectionListData>>>?
                loadInspectionsAsync = null)
    {
        return new InspectionStatusViewModel(
            loadInspectionsAsync ??
            (() =>
                Task.FromResult<
                    IReadOnlyList<
                        InspectionListData>>(
                    Array.Empty<
                        InspectionListData>())));
    }


    private static IReadOnlyList<
        InspectionListData>
        CreateRows(
            int count)
    {
        return Enumerable
            .Range(
                1,
                count)
            .Select(
                i =>
                    CreateData(
                        index:
                            i,
                        equipmentCode:
                            $"EQ-{i:000}"))
            .ToList();
    }


    private static InspectionListData
        CreateData(
            int index,
            string factorySiteName =
                "第1工場",
            string locationName =
                "設備エリア",
            string? equipmentCode = null,
            string equipmentName =
                "設備",
            string templateName =
                "日常点検",
            string operatorName =
                "点検担当者A",
            InspectionStatus status =
                InspectionStatus.InProgress,
            int resultCount = 3,
            int abnormalCount = 0,
            int photoCount = 0)
    {
        return new InspectionListData(
            ScheduleId:
                CreateGuid(
                    index),

            InspectionId:
                CreateGuid(
                    1000 +
                    index),

            ScheduledDate:
                new DateOnly(
                    2026,
                    8,
                    Math.Min(
                        index,
                        28)),

            FactorySiteName:
                factorySiteName,

            LocationName:
                locationName,

            EquipmentCode:
                equipmentCode ??
                $"EQ-{index:000}",

            EquipmentName:
                equipmentName,

            TemplateName:
                templateName,

            OperatorName:
                operatorName,

            Status:
                status,

            ResultCount:
                resultCount,

            AbnormalCount:
                abnormalCount,

            PhotoCount:
                photoCount);
    }


    private static Guid
        CreateGuid(
            int value)
    {
        return Guid.Parse(
            $"00000000-0000-0000-0000-" +
            $"{value:000000000000}");
    }
}


// ============================================
// Filter Option
// ============================================

public sealed class
    InspectionStatusFilterOptionViewModelTests
{
    [Fact]
    public void Constructor_WithBlankDisplayName_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new InspectionStatusFilterOptionViewModel(
                        "   ",
                        InspectionStatus.Approved));


        // Assert
        Assert.Equal(
            "displayName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_SetsProperties()
    {
        // Act
        var sut =
            new InspectionStatusFilterOptionViewModel(
                "承認済み",
                InspectionStatus.Approved);


        // Assert
        Assert.Equal(
            "承認済み",
            sut.DisplayName);

        Assert.Equal(
            InspectionStatus.Approved,
            sut.Status);
    }


    [Fact]
    public void Constructor_AllFilter_AllowsNullStatus()
    {
        // Act
        var sut =
            new InspectionStatusFilterOptionViewModel(
                "すべて",
                null);


        // Assert
        Assert.Equal(
            "すべて",
            sut.DisplayName);

        Assert.Null(
            sut.Status);
    }
}