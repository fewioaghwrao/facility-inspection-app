using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class ApprovalPendingListViewModelTests
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
                    new ApprovalPendingListViewModel(
                        (Func<
                            Task<
                                IReadOnlyList<
                                    InspectionListData>>>)null!));

        // Assert
        Assert.Equal(
            "loadApprovalPendingAsync",
            exception.ParamName);
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
        Assert.Empty(
            sut.Items);

        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);

        Assert.True(
            sut.IsEmpty);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task ReloadCommand_LoadsApprovalPendingItems()
    {
        // Arrange
        var first =
            CreateRow(
                equipmentCode:
                    "EQ-001",
                equipmentName:
                    "コンプレッサー");

        var second =
            CreateRow(
                equipmentCode:
                    "EQ-002",
                equipmentName:
                    "循環ポンプ");

        var sut =
            CreateViewModel(
                [
                    first,
                    second
                ]);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            first.ScheduleId,
            sut.Items[0].ScheduleId);

        Assert.Equal(
            first.InspectionId,
            sut.Items[0].InspectionId);

        Assert.Equal(
            "EQ-001",
            sut.Items[0].EquipmentCode);

        Assert.Equal(
            "コンプレッサー",
            sut.Items[0].EquipmentName);

        Assert.Equal(
            second.ScheduleId,
            sut.Items[1].ScheduleId);

        Assert.False(
            sut.IsEmpty);

        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);
    }


    // ============================================
    // InspectionId == null
    // ============================================

    [Fact]
    public async Task ReloadCommand_SkipsRowsWithoutInspectionId()
    {
        // Arrange
        var valid =
            CreateRow(
                equipmentCode:
                    "EQ-001");

        var invalid =
            CreateRow(
                hasInspection:
                    false,
                equipmentCode:
                    "EQ-999");

        var sut =
            CreateViewModel(
                [
                    valid,
                invalid
                ]);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            valid.ScheduleId,
            sut.Items[0].ScheduleId);

        Assert.Equal(
            "EQ-001",
            sut.Items[0].EquipmentCode);
    }


    // ============================================
    // Empty
    // ============================================

    [Fact]
    public async Task ReloadCommand_WithNoItems_SetsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                []);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);

        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);
    }


    [Fact]
    public async Task ReloadCommand_WhenAllRowsHaveNullInspectionId_SetsEmptyState()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow(
                    hasInspection:
                        false),

                CreateRow(
                    hasInspection:
                        false)
                ]);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Empty(
            sut.Items);

        Assert.True(
            sut.IsEmpty);
    }


    // ============================================
    // Reload
    // ============================================

    [Fact]
    public async Task ReloadCommand_ReplacesPreviousItems()
    {
        // Arrange
        var callCount =
            0;

        IReadOnlyList<
            InspectionListData> firstData =
        [
            CreateRow(
                equipmentCode:
                    "OLD-001"),

            CreateRow(
                equipmentCode:
                    "OLD-002")
        ];

        IReadOnlyList<
            InspectionListData> secondData =
        [
            CreateRow(
                equipmentCode:
                    "NEW-001")
        ];

        var sut =
            new ApprovalPendingListViewModel(
                () =>
                {
                    callCount++;

                    return Task.FromResult(
                        callCount == 1
                            ? firstData
                            : secondData);
                });

        await sut.ReloadCommand
            .ExecuteAsync(null);

        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            "OLD-001",
            sut.Items[0].EquipmentCode);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            2,
            callCount);

        Assert.Single(
            sut.Items);

        Assert.Equal(
            "NEW-001",
            sut.Items[0].EquipmentCode);
    }


    // ============================================
    // Loading
    // ============================================

    [Fact]
    public async Task ReloadCommand_WhileLoading_SetsIsLoadingTrue()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<
                    InspectionListData>>();

        var sut =
            new ApprovalPendingListViewModel(
                () =>
                    completionSource.Task);

        // Act
        var loadTask =
            sut.ReloadCommand
                .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsEmpty);

        // Complete
        completionSource.SetResult(
            []);

        await loadTask;

        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsEmpty);
    }


    // ============================================
    // Error
    // ============================================

    [Fact]
    public async Task ReloadCommand_WhenLoaderThrows_SetsErrorMessage()
    {
        // Arrange
        var sut =
            new ApprovalPendingListViewModel(
                () =>
                    Task.FromException<
                        IReadOnlyList<
                            InspectionListData>>(
                        new InvalidOperationException(
                            "テストエラー")));

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "承認待ち一覧を取得できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsEmpty);
    }


    [Fact]
    public async Task ReloadCommand_AfterPreviousError_ClearsErrorMessage()
    {
        // Arrange
        var callCount =
            0;

        IReadOnlyList<
            InspectionListData> successData =
        [
            CreateRow()
        ];

        var sut =
            new ApprovalPendingListViewModel(
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        return Task.FromException<
                            IReadOnlyList<
                                InspectionListData>>(
                            new InvalidOperationException(
                                "1回目エラー"));
                    }

                    return Task.FromResult(
                        successData);
                });

        await sut.ReloadCommand
            .ExecuteAsync(null);

        Assert.NotNull(
            sut.ErrorMessage);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Null(
            sut.ErrorMessage);

        Assert.Single(
            sut.Items);

        Assert.False(
            sut.IsEmpty);
    }


    [Fact]
    public async Task ReloadCommand_WhenReloadFails_KeepsPreviousItems()
    {
        // Arrange
        var callCount =
            0;

        IReadOnlyList<
            InspectionListData> successData =
        [
            CreateRow(
                equipmentCode:
                    "EQ-001")
        ];

        var sut =
            new ApprovalPendingListViewModel(
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
                            InspectionListData>>(
                        new InvalidOperationException(
                            "再読込エラー"));
                });

        await sut.ReloadCommand
            .ExecuteAsync(null);

        Assert.Single(
            sut.Items);

        // Act
        await sut.ReloadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Single(
            sut.Items);

        Assert.Equal(
            "EQ-001",
            sut.Items[0].EquipmentCode);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "再読込エラー",
            sut.ErrorMessage);
    }


    // ============================================
    // Detail
    // ============================================

    [Fact]
    public async Task ItemOpenDetailCommand_RaisesDetailRequested()
    {
        // Arrange
        var scheduleId =
            Guid.NewGuid();

        var sut =
            CreateViewModel(
                [
                    CreateRow(
                        scheduleId:
                            scheduleId)
                ]);

        Guid? requestedScheduleId =
            null;

        sut.DetailRequested =
            id =>
                requestedScheduleId =
                    id;

        await sut.ReloadCommand
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
    public async Task ItemOpenDetailCommand_WithNoSubscriber_DoesNotThrow()
    {
        // Arrange
        var sut =
            CreateViewModel(
                [
                    CreateRow()
                ]);

        await sut.ReloadCommand
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
    // Helpers
    // ============================================

    private static ApprovalPendingListViewModel
        CreateViewModel(
            IReadOnlyList<
                InspectionListData> data)
    {
        return new ApprovalPendingListViewModel(
            () =>
                Task.FromResult(
                    data));
    }

    private static InspectionListData
        CreateRow(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            bool hasInspection = true,
            DateOnly? scheduledDate = null,
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
            InspectionStatus status =
                InspectionStatus.Completed,
            int resultCount = 5,
            int abnormalCount = 1,
            int photoCount = 2)
    {
        Guid? actualInspectionId =
            hasInspection
                ? inspectionId ??
                  Guid.NewGuid()
                : null;

        return new InspectionListData(
            ScheduleId:
                scheduleId ??
                Guid.NewGuid(),

            InspectionId:
                actualInspectionId,

            ScheduledDate:
                scheduledDate ??
                new DateOnly(
                    2026,
                    8,
                    19),

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