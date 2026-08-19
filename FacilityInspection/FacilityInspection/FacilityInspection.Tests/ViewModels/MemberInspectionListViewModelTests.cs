using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class
    MemberInspectionListViewModelTests
{
    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new MemberInspectionListViewModel(
                        Guid.Empty,
                        _ =>
                            Task.FromResult(
                                0),
                        (_, _, _) =>
                            EmptyRows()));


        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);

        Assert.Contains(
            "点検担当者IDを指定してください。",
            exception.Message);
    }


    [Fact]
    public void Constructor_WithNullCountLoader_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberInspectionListViewModel(
                        OperatorId,
                        null!,
                        (_, _, _) =>
                            EmptyRows()));


        // Assert
        Assert.Equal(
            "getCountForOperatorAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullPageLoader_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberInspectionListViewModel(
                        OperatorId,
                        _ =>
                            Task.FromResult(
                                0),
                        null!));


        // Assert
        Assert.Equal(
            "getPageForOperatorAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_InitializesExpectedStateWithoutAutoLoad()
    {
        // Arrange
        var countCallCount =
            0;

        var pageCallCount =
            0;


        // Act
        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                    {
                        countCallCount++;

                        return Task.FromResult(
                            10);
                    },

                getPageForOperatorAsync:
                    (_, _, _) =>
                    {
                        pageCallCount++;

                        return EmptyRows();
                    });


        // Assert
        Assert.Equal(
            "点検一覧",
            sut.Title);

        Assert.Equal(
            "担当している点検を確認できます。",
            sut.Description);

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
            1,
            sut.PageNumber);

        Assert.Equal(
            0,
            sut.TotalCount);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.Equal(
            "全 0 件",
            sut.TotalCountText);

        Assert.False(
            sut.CanPreviousPage);

        Assert.False(
            sut.CanNextPage);

        Assert.Equal(
            0,
            countCallCount);

        Assert.Equal(
            0,
            pageCallCount);
    }


    // ============================================
    // Total Pages
    // ============================================

    [Theory]
    [InlineData(
        0,
        1)]
    [InlineData(
        1,
        1)]
    [InlineData(
        5,
        1)]
    [InlineData(
        6,
        2)]
    [InlineData(
        10,
        2)]
    [InlineData(
        11,
        3)]
    public void TotalPages_ReturnsExpectedValue(
        int totalCount,
        int expected)
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.TotalCount =
            totalCount;


        // Assert
        Assert.Equal(
            expected,
            sut.TotalPages);

        Assert.Equal(
            $"1 / {expected}",
            sut.PageText);

        Assert.Equal(
            $"全 {totalCount} 件",
            sut.TotalCountText);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadAsync_LoadsCountAndFirstPage()
    {
        // Arrange
        Guid capturedCountOperatorId =
            Guid.Empty;

        Guid capturedPageOperatorId =
            Guid.Empty;

        var capturedPageNumber =
            0;

        var capturedPageSize =
            0;


        IReadOnlyList<InspectionListData>
            rows =
                CreateRows(
                    count:
                        5);


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    operatorId =>
                    {
                        capturedCountOperatorId =
                            operatorId;

                        return Task.FromResult(
                            6);
                    },

                getPageForOperatorAsync:
                    (
                        operatorId,
                        pageNumber,
                        pageSize) =>
                    {
                        capturedPageOperatorId =
                            operatorId;

                        capturedPageNumber =
                            pageNumber;

                        capturedPageSize =
                            pageSize;

                        return Task.FromResult(
                            rows);
                    });


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            OperatorId,
            capturedCountOperatorId);

        Assert.Equal(
            OperatorId,
            capturedPageOperatorId);

        Assert.Equal(
            1,
            capturedPageNumber);

        Assert.Equal(
            5,
            capturedPageSize);


        Assert.Equal(
            6,
            sut.TotalCount);

        Assert.Equal(
            2,
            sut.TotalPages);

        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            "全 6 件",
            sut.TotalCountText);


        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.False(
            sut.IsEmpty);

        Assert.False(
            sut.CanPreviousPage);

        Assert.True(
            sut.CanNextPage);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);
    }


    [Fact]
    public async Task LoadAsync_WhenCountIsZero_CreatesEmptyFirstPage()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            0,
            sut.TotalCount);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            "1 / 1",
            sut.PageText);

        Assert.Equal(
            "全 0 件",
            sut.TotalCountText);

        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);

        Assert.False(
            sut.CanPreviousPage);

        Assert.False(
            sut.CanNextPage);
    }


    [Fact]
    public async Task LoadAsync_WhenCurrentPageExceedsTotalPages_ClampsPageNumber()
    {
        // Arrange
        var capturedPageNumber =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            4),

                getPageForOperatorAsync:
                    (
                        _,
                        pageNumber,
                        _) =>
                    {
                        capturedPageNumber =
                            pageNumber;

                        return Task.FromResult<
                            IReadOnlyList<
                                InspectionListData>>(
                            CreateRows(
                                4));
                    });


        sut.PageNumber =
            3;


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            1,
            capturedPageNumber);

        Assert.Equal(
            "1 / 1",
            sut.PageText);
    }


    // ============================================
    // Next Page
    // ============================================

    [Fact]
    public async Task NextPageCommand_LoadsNextPage()
    {
        // Arrange
        var pageCallCount =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            7),

                getPageForOperatorAsync:
                    (
                        _,
                        pageNumber,
                        pageSize) =>
                    {
                        pageCallCount++;


                        if (pageNumber == 1)
                        {
                            return Task.FromResult<
                                IReadOnlyList<
                                    InspectionListData>>(
                                CreateRows(
                                    count:
                                        pageSize,
                                    startIndex:
                                        1));
                        }


                        return Task.FromResult<
                            IReadOnlyList<
                                InspectionListData>>(
                            CreateRows(
                                count:
                                    2,
                                startIndex:
                                    6));
                    });


        await sut.LoadAsync();


        // Act
        await sut.NextPageCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            pageCallCount);

        Assert.Equal(
            2,
            sut.PageNumber);

        Assert.Equal(
            "2 / 2",
            sut.PageText);

        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "EQ-006 設備6",
            sut.Items[0]
                .EquipmentText);

        Assert.True(
            sut.CanPreviousPage);

        Assert.False(
            sut.CanNextPage);
    }


    // ============================================
    // Previous Page
    // ============================================

    [Fact]
    public async Task PreviousPageCommand_LoadsPreviousPage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            7),

                getPageForOperatorAsync:
                    (
                        _,
                        pageNumber,
                        pageSize) =>
                    {
                        var startIndex =
                            pageNumber == 1
                                ? 1
                                : 6;

                        var count =
                            pageNumber == 1
                                ? pageSize
                                : 2;


                        return Task.FromResult<
                            IReadOnlyList<
                                InspectionListData>>(
                            CreateRows(
                                count,
                                startIndex));
                    });


        await sut.LoadAsync();

        await sut.NextPageCommand
            .ExecuteAsync(null);


        Assert.Equal(
            2,
            sut.PageNumber);


        // Act
        await sut.PreviousPageCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            "1 / 2",
            sut.PageText);

        Assert.Equal(
            5,
            sut.Items.Count);

        Assert.Equal(
            "EQ-001 設備1",
            sut.Items[0]
                .EquipmentText);

        Assert.False(
            sut.CanPreviousPage);

        Assert.True(
            sut.CanNextPage);
    }


    // ============================================
    // Page Boundaries
    // ============================================

    [Fact]
    public async Task PreviousPageCommand_OnFirstPage_DoesNothing()
    {
        // Arrange
        var pageCallCount =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            7),

                getPageForOperatorAsync:
                    (_, _, _) =>
                    {
                        pageCallCount++;

                        return EmptyRows();
                    });


        await sut.LoadAsync();


        Assert.Equal(
            1,
            pageCallCount);


        // Act
        await sut.PreviousPageCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            1,
            pageCallCount);
    }


    [Fact]
    public async Task NextPageCommand_OnLastPage_DoesNothing()
    {
        // Arrange
        var pageCallCount =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            5),

                getPageForOperatorAsync:
                    (_, _, _) =>
                    {
                        pageCallCount++;

                        return EmptyRows();
                    });


        await sut.LoadAsync();


        Assert.False(
            sut.CanNextPage);

        Assert.Equal(
            1,
            pageCallCount);


        // Act
        await sut.NextPageCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            1,
            pageCallCount);
    }


    // ============================================
    // Refresh
    // ============================================

    [Fact]
    public async Task RefreshCommand_ResetsPageToOneAndReloadsCount()
    {
        // Arrange
        var countCallCount =
            0;

        var pageCallCount =
            0;

        var latestPageNumber =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                    {
                        countCallCount++;

                        return Task.FromResult(
                            countCallCount == 1
                                ? 11
                                : 4);
                    },

                getPageForOperatorAsync:
                    (
                        _,
                        pageNumber,
                        _) =>
                    {
                        pageCallCount++;

                        latestPageNumber =
                            pageNumber;

                        return EmptyRows();
                    });


        await sut.LoadAsync();


        sut.PageNumber =
            3;


        // Act
        await sut.RefreshCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            countCallCount);

        Assert.Equal(
            2,
            pageCallCount);

        Assert.Equal(
            1,
            latestPageNumber);

        Assert.Equal(
            1,
            sut.PageNumber);

        Assert.Equal(
            4,
            sut.TotalCount);

        Assert.Equal(
            1,
            sut.TotalPages);

        Assert.Equal(
            "1 / 1",
            sut.PageText);
    }


    // ============================================
    // Loading
    // ============================================

    [Fact]
    public async Task LoadAsync_WhileLoading_DisablesPageNavigation()
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
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            6),

                getPageForOperatorAsync:
                    (_, _, _) =>
                        completionSource.Task);


        // Act
        var loadTask =
            sut.LoadAsync();


        // Assert - loading
        Assert.True(
            sut.IsLoading);

        Assert.Equal(
            6,
            sut.TotalCount);

        Assert.Equal(
            2,
            sut.TotalPages);

        Assert.False(
            sut.CanPreviousPage);

        Assert.False(
            sut.CanNextPage);

        Assert.False(
            sut.IsEmpty);


        // Complete
        completionSource.SetResult(
            CreateRows(
                5));


        await loadTask;


        // Assert - completed
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.CanNextPage);
    }


    // ============================================
    // Count Error
    // ============================================

    [Fact]
    public async Task LoadAsync_WhenCountLoaderFails_SetsErrorAndDoesNotLoadPage()
    {
        // Arrange
        var pageCallCount =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromException<int>(
                            new InvalidOperationException(
                                "件数取得テストエラー")),

                getPageForOperatorAsync:
                    (_, _, _) =>
                    {
                        pageCallCount++;

                        return EmptyRows();
                    });


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            0,
            pageCallCount);

        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検一覧を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "件数取得テストエラー",
            sut.ErrorMessage);
    }


    // ============================================
    // Page Error
    // ============================================

    [Fact]
    public async Task LoadAsync_WhenPageLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            3),

                getPageForOperatorAsync:
                    (_, _, _) =>
                        Task.FromException<
                            IReadOnlyList<
                                InspectionListData>>(
                            new InvalidOperationException(
                                "ページ取得テストエラー")));


        // Act
        await sut.LoadAsync();


        // Assert
        Assert.Equal(
            3,
            sut.TotalCount);

        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検一覧を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "ページ取得テストエラー",
            sut.ErrorMessage);
    }


    // ============================================
    // Error Recovery
    // ============================================

    [Fact]
    public async Task RefreshCommand_AfterFailureThenSuccess_ClearsError()
    {
        // Arrange
        var pageCallCount =
            0;


        var sut =
            CreateViewModel(
                getCountForOperatorAsync:
                    _ =>
                        Task.FromResult(
                            1),

                getPageForOperatorAsync:
                    (_, _, _) =>
                    {
                        pageCallCount++;


                        if (pageCallCount == 1)
                        {
                            return Task.FromException<
                                IReadOnlyList<
                                    InspectionListData>>(
                                new InvalidOperationException(
                                    "一時的なエラー"));
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
        await sut.RefreshCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            pageCallCount);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Single(
            sut.Items);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // List Item
    // ============================================

    [Fact]
    public void ListItem_Constructor_WithNullData_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MemberInspectionListItemViewModel(
                        null!));


        // Assert
        Assert.Equal(
            "data",
            exception.ParamName);
    }


    [Fact]
    public void ListItem_Constructor_MapsDisplayProperties()
    {
        // Arrange
        var data =
            CreateData(
                index:
                    1,

                date:
                    new DateOnly(
                        2026,
                        8,
                        5),

                factorySiteName:
                    "第1工場",

                locationName:
                    "コンプレッサー室",

                equipmentCode:
                    "COMP-01",

                equipmentName:
                    "コンプレッサー1号機",

                templateName:
                    "コンプレッサー日常点検",

                status:
                    InspectionStatus.Completed,

                abnormalCount:
                    2);


        // Act
        var sut =
            new MemberInspectionListItemViewModel(
                data);


        // Assert
        Assert.Equal(
            "2026/08/05",
            sut.ScheduledDateText);

        Assert.Equal(
            "COMP-01 コンプレッサー1号機",
            sut.EquipmentText);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationText);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.TemplateName);

        Assert.Equal(
            "完了・承認待ち",
            sut.StatusText);

        Assert.Equal(
            2,
            sut.AbnormalCount);

        Assert.Equal(
            "異常 2 件",
            sut.AbnormalText);
    }


    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "未実施")]
    [InlineData(
        InspectionStatus.InProgress,
        "実施中")]
    [InlineData(
        InspectionStatus.Completed,
        "完了・承認待ち")]
    [InlineData(
        InspectionStatus.Returned,
        "差し戻し")]
    [InlineData(
        InspectionStatus.Approved,
        "承認済み")]
    public void ListItem_StatusText_ReturnsExpectedText(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var data =
            CreateData(
                index:
                    1,
                status:
                    status);


        // Act
        var sut =
            new MemberInspectionListItemViewModel(
                data);


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    [Fact]
    public void ListItem_StatusText_WithUnknownStatus_ReturnsDash()
    {
        // Arrange
        var data =
            CreateData(
                index:
                    1,

                status:
                    (InspectionStatus)999);


        // Act
        var sut =
            new MemberInspectionListItemViewModel(
                data);


        // Assert
        Assert.Equal(
            "-",
            sut.StatusText);
    }


    [Theory]
    [InlineData(
        0,
        "異常なし")]
    [InlineData(
        1,
        "異常 1 件")]
    [InlineData(
        3,
        "異常 3 件")]
    public void ListItem_AbnormalText_ReturnsExpectedText(
        int abnormalCount,
        string expected)
    {
        // Arrange
        var data =
            CreateData(
                index:
                    1,

                abnormalCount:
                    abnormalCount);


        // Act
        var sut =
            new MemberInspectionListItemViewModel(
                data);


        // Assert
        Assert.Equal(
            expected,
            sut.AbnormalText);
    }


    // ============================================
    // Helpers
    // ============================================

    private static MemberInspectionListViewModel
        CreateViewModel(
            Func<
                Guid,
                Task<int>>?
                getCountForOperatorAsync = null,

            Func<
                Guid,
                int,
                int,
                Task<
                    IReadOnlyList<
                        InspectionListData>>>?
                getPageForOperatorAsync = null)
    {
        return new MemberInspectionListViewModel(
            OperatorId,

            getCountForOperatorAsync ??
            (_ =>
                Task.FromResult(
                    0)),

            getPageForOperatorAsync ??
            ((_, _, _) =>
                EmptyRows()));
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
            DateOnly? date = null,
            string factorySiteName = "第1工場",
            string locationName = "設備エリア",
            string? equipmentCode = null,
            string? equipmentName = null,
            string templateName = "日常点検",
            InspectionStatus status =
                InspectionStatus.NotStarted,
            int abnormalCount = 0)
    {
        return new InspectionListData(
            ScheduleId:
                Guid.Parse(
                    $"00000000-0000-0000-0000-{index:D12}"),

            InspectionId:
                null,

            ScheduledDate:
                date ??
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
                "点検担当者A",

            Status:
                status,

            ResultCount:
                0,

            AbnormalCount:
                abnormalCount,

            PhotoCount:
                0);
    }
}