using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class AdminDashboardViewModelTests
{
    private static readonly DateTime
        FixedNow =
            new(
                2026,
                8,
                18,
                10,
                30,
                0);


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithBlankDisplayName_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new AdminDashboardViewModel(
                        "   ",
                        () =>
                            Task.FromResult<
                                IReadOnlyList<
                                    InspectionListData>>(
                                []),
                        () =>
                            FixedNow));

        // Assert
        Assert.Equal(
            "displayName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullLoader_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AdminDashboardViewModel(
                        "管理者",
                        null!,
                        () =>
                            FixedNow));

        // Assert
        Assert.Equal(
            "loadInspectionsAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullNowProvider_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new AdminDashboardViewModel(
                        "管理者",
                        () =>
                            Task.FromResult<
                                IReadOnlyList<
                                    InspectionListData>>(
                                []),
                        null!));

        // Assert
        Assert.Equal(
            "nowProvider",
            exception.ParamName);
    }


    // ============================================
    // Header
    // ============================================

    [Fact]
    public void Constructor_SetsHeaderProperties()
    {
        // Arrange & Act
        var sut =
            CreateViewModel(
                []);

        // Assert
        Assert.Equal(
            "管理者",
            sut.DisplayName);

        Assert.Equal(
            "管理者さん、お疲れさまです。",
            sut.WelcomeMessage);

        Assert.Equal(
            "2026年8月18日",
            sut.CurrentDateText);
    }


    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_SetsInitialState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel(
                []);

        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.Equal(
            0,
            sut.TodayInspectionCount);

        Assert.Equal(
            0,
            sut.TodayCompletedCount);

        Assert.Equal(
            0,
            sut.TodayNotStartedCount);

        Assert.Equal(
            0,
            sut.ApprovalPendingCount);

        Assert.Equal(
            0,
            sut.TodayAbnormalCount);

        Assert.Equal(
            0,
            sut.CompletionRate);

        Assert.Equal(
            "0%",
            sut.CompletionRateText);

        Assert.Equal(
            "本日の点検予定はありません。",
            sut.TodaySummaryText);
    }


    // ============================================
    // Load
    // 集計
    // ============================================

    [Fact]
    public async Task LoadCommand_CalculatesDashboardCounts()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        var yesterday =
            today.AddDays(
                -1);

        var tomorrow =
            today.AddDays(
                1);

        IReadOnlyList<
            InspectionListData> data =
        [
            // 本日：未実施
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.NotStarted,
                abnormalCount:
                    0),

            // 本日：実施中
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.InProgress,
                abnormalCount:
                    1),

            // 本日：完了・承認待ち
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed,
                abnormalCount:
                    2),

            // 本日：承認済み
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved,
                abnormalCount:
                    3),

            // 昨日：完了・承認待ち
            CreateRow(
                scheduledDate:
                    yesterday,
                status:
                    InspectionStatus.Completed,
                abnormalCount:
                    10),

            // 明日：完了・承認待ち
            CreateRow(
                scheduledDate:
                    tomorrow,
                status:
                    InspectionStatus.Completed,
                abnormalCount:
                    20)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert

        // 本日の4件だけ
        Assert.Equal(
            4,
            sut.TodayInspectionCount);

        // Completed + Approved
        Assert.Equal(
            2,
            sut.TodayCompletedCount);

        // NotStartedのみ
        Assert.Equal(
            1,
            sut.TodayNotStartedCount);

        // 日付に関係なくCompleted
        Assert.Equal(
            3,
            sut.ApprovalPendingCount);

        // 本日のみ 0 + 1 + 2 + 3
        Assert.Equal(
            6,
            sut.TodayAbnormalCount);

        // 2 / 4 = 50%
        Assert.Equal(
            50,
            sut.CompletionRate);

        Assert.Equal(
            "50%",
            sut.CompletionRateText);

        Assert.Equal(
            "4件中 2件が完了しています。",
            sut.TodaySummaryText);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);
    }


    // ============================================
    // Completed / Approved
    // ============================================

    [Fact]
    public async Task LoadCommand_CountsCompletedAndApprovedAsCompletedToday()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.NotStarted),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.InProgress)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            4,
            sut.TodayInspectionCount);

        Assert.Equal(
            2,
            sut.TodayCompletedCount);
    }


    // ============================================
    // Approval Pending
    // ============================================

    [Fact]
    public async Task LoadCommand_ApprovalPendingCount_IncludesCompletedFromAllDates()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today.AddDays(-1),
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today.AddDays(-30),
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            3,
            sut.ApprovalPendingCount);
    }


    [Fact]
    public async Task LoadCommand_DoesNotCountApprovedAsApprovalPending()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            1,
            sut.ApprovalPendingCount);
    }


    // ============================================
    // Not Started
    // ============================================

    [Fact]
    public async Task LoadCommand_CountsOnlyTodayNotStartedItems()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.NotStarted),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.NotStarted),

            CreateRow(
                scheduledDate:
                    today.AddDays(-1),
                status:
                    InspectionStatus.NotStarted),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.InProgress)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            2,
            sut.TodayNotStartedCount);
    }


    // ============================================
    // Abnormal Count
    // ============================================

    [Fact]
    public async Task LoadCommand_SumsOnlyTodayAbnormalCounts()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                abnormalCount:
                    2),

            CreateRow(
                scheduledDate:
                    today,
                abnormalCount:
                    3),

            CreateRow(
                scheduledDate:
                    today,
                abnormalCount:
                    1),

            CreateRow(
                scheduledDate:
                    today.AddDays(-1),
                abnormalCount:
                    100)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            6,
            sut.TodayAbnormalCount);
    }


    // ============================================
    // Completion Rate
    // ============================================

    [Fact]
    public async Task LoadCommand_WithNoTodayItems_SetsCompletionRateToZero()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today.AddDays(-1),
                status:
                    InspectionStatus.Completed)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            sut.TodayInspectionCount);

        Assert.Equal(
            0,
            sut.TodayCompletedCount);

        Assert.Equal(
            0,
            sut.CompletionRate);

        Assert.Equal(
            "0%",
            sut.CompletionRateText);

        Assert.Equal(
            "本日の点検予定はありません。",
            sut.TodaySummaryText);
    }


    [Fact]
    public async Task LoadCommand_CalculatesAndRoundsCompletionRate()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.NotStarted)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        // 2 / 3 * 100 = 66.666... → 67
        Assert.Equal(
            67,
            sut.CompletionRate);

        Assert.Equal(
            "67%",
            sut.CompletionRateText);
    }


    [Fact]
    public async Task LoadCommand_WhenAllTodayItemsAreCompleted_SetsRateToOneHundred()
    {
        // Arrange
        var today =
            DateOnly.FromDateTime(
                FixedNow);

        IReadOnlyList<
            InspectionListData> data =
        [
            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Completed),

            CreateRow(
                scheduledDate:
                    today,
                status:
                    InspectionStatus.Approved)
        ];

        var sut =
            CreateViewModel(
                data);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            100,
            sut.CompletionRate);

        Assert.Equal(
            "100%",
            sut.CompletionRateText);
    }


    // ============================================
    // Error
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenLoaderThrows_SetsErrorMessage()
    {
        // Arrange
        var sut =
            new AdminDashboardViewModel(
                "管理者",
                () =>
                    throw new InvalidOperationException(
                        "テストエラー"),
                () =>
                    FixedNow);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "ダッシュボードを読み込めませんでした。",
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
            InspectionListData> successData =
        [
            CreateRow(
                scheduledDate:
                    DateOnly.FromDateTime(
                        FixedNow),
                status:
                    InspectionStatus.Completed)
        ];

        var sut =
            new AdminDashboardViewModel(
                "管理者",
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        throw new InvalidOperationException(
                            "1回目の読み込みエラー");
                    }

                    return Task.FromResult(
                        successData);
                },
                () =>
                    FixedNow);

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

        Assert.Equal(
            1,
            sut.TodayInspectionCount);

        Assert.Equal(
            1,
            sut.TodayCompletedCount);

        Assert.Equal(
            100,
            sut.CompletionRate);
    }


    // ============================================
    // Navigation
    // ============================================

    [Fact]
    public void OpenInspectionStatusCommand_InvokesInspectionStatusRequested()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        var callCount =
            0;

        sut.InspectionStatusRequested =
            () =>
                callCount++;

        // Act
        sut.OpenInspectionStatusCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public void OpenNotStartedCommand_InvokesNotStartedRequested()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        var callCount =
            0;

        sut.NotStartedRequested =
            () =>
                callCount++;

        // Act
        sut.OpenNotStartedCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public void OpenApprovalPendingCommand_InvokesApprovalPendingRequested()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        var callCount =
            0;

        sut.ApprovalPendingRequested =
            () =>
                callCount++;

        // Act
        sut.OpenApprovalPendingCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public void OpenAbnormalListCommand_InvokesAbnormalListRequested()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        var callCount =
            0;

        sut.AbnormalListRequested =
            () =>
                callCount++;

        // Act
        sut.OpenAbnormalListCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public void NavigationCommands_WithNoSubscribers_DoNotThrow()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        // Act
        var exception =
            Record.Exception(
                () =>
                {
                    sut.OpenInspectionStatusCommand
                        .Execute(null);

                    sut.OpenNotStartedCommand
                        .Execute(null);

                    sut.OpenApprovalPendingCommand
                        .Execute(null);

                    sut.OpenAbnormalListCommand
                        .Execute(null);
                });

        // Assert
        Assert.Null(
            exception);
    }


    // ============================================
    // Refresh
    // ============================================

    [Fact]
    public async Task Refresh_LoadsLatestDataAgain()
    {
        // Arrange
        var callCount =
            0;

        var today =
            DateOnly.FromDateTime(
                FixedNow);

        var firstData =
            (IReadOnlyList<InspectionListData>)
            [
                CreateRow(
                    scheduledDate:
                        today,
                    status:
                        InspectionStatus.NotStarted)
            ];

        var secondData =
            (IReadOnlyList<InspectionListData>)
            [
                CreateRow(
                    scheduledDate:
                        today,
                    status:
                        InspectionStatus.Completed),

                CreateRow(
                    scheduledDate:
                        today,
                    status:
                        InspectionStatus.Approved)
            ];

        var sut =
            new AdminDashboardViewModel(
                "管理者",
                () =>
                {
                    callCount++;

                    return Task.FromResult(
                        callCount == 1
                            ? firstData
                            : secondData);
                },
                () =>
                    FixedNow);

        await sut.LoadCommand
            .ExecuteAsync(null);

        Assert.Equal(
            1,
            sut.TodayInspectionCount);

        Assert.Equal(
            0,
            sut.TodayCompletedCount);

        // Act
        sut.Refresh();

        // Assert
        //
        // Task.FromResultなのでLoadAsyncは
        // Refresh内で実質的に完了する。
        Assert.Equal(
            2,
            callCount);

        Assert.Equal(
            2,
            sut.TodayInspectionCount);

        Assert.Equal(
            2,
            sut.TodayCompletedCount);

        Assert.Equal(
            100,
            sut.CompletionRate);
    }


    // ============================================
    // Test Helpers
    // ============================================

    private static AdminDashboardViewModel
        CreateViewModel(
            IReadOnlyList<
                InspectionListData> data)
    {
        return new AdminDashboardViewModel(
            "管理者",
            () =>
                Task.FromResult(
                    data),
            () =>
                FixedNow);
    }


    private static InspectionListData
        CreateRow(
            DateOnly? scheduledDate = null,
            InspectionStatus status =
                InspectionStatus.NotStarted,
            int resultCount = 0,
            int abnormalCount = 0,
            int photoCount = 0)
    {
        var scheduleId =
            Guid.NewGuid();

        Guid? inspectionId =
            status ==
            InspectionStatus.NotStarted
                ? null
                : Guid.NewGuid();

        return new InspectionListData(
            ScheduleId:
                scheduleId,

            InspectionId:
                inspectionId,

            ScheduledDate:
                scheduledDate ??
                DateOnly.FromDateTime(
                    FixedNow),

            FactorySiteName:
                "第1工場",

            LocationName:
                "製造エリア",

            EquipmentCode:
                "EQ-001",

            EquipmentName:
                "コンプレッサー",

            TemplateName:
                "日常点検",

            OperatorName:
                "点検担当者A",

            Status:
                status,

            ResultCount:
                resultCount,

            AbnormalCount:
                abnormalCount,

            PhotoCount:
                photoCount);
    }
}