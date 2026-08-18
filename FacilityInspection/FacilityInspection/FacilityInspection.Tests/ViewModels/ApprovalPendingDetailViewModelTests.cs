using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class ApprovalPendingDetailViewModelTests
{
    private static readonly Guid
        ScheduleId =
            Guid.Parse(
                "11111111-1111-1111-1111-111111111111");

    private static readonly Guid
        InspectionId =
            Guid.Parse(
                "22222222-2222-2222-2222-222222222222");

    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "33333333-3333-3333-3333-333333333333");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyScheduleId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new ApprovalPendingDetailViewModel(
                        Guid.Empty,
                        OperatorId,
                        () =>
                            Task.FromResult<
                                InspectionDetailData?>(
                                null),
                        () =>
                            Task.CompletedTask,
                        _ =>
                            Task.CompletedTask));

        // Assert
        Assert.Equal(
            "scheduleId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithEmptyOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentException>(
                () =>
                    new ApprovalPendingDetailViewModel(
                        ScheduleId,
                        Guid.Empty,
                        () =>
                            Task.FromResult<
                                InspectionDetailData?>(
                                null),
                        () =>
                            Task.CompletedTask,
                        _ =>
                            Task.CompletedTask));

        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullLoadDetail_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new ApprovalPendingDetailViewModel(
                        ScheduleId,
                        OperatorId,
                        null!,
                        () =>
                            Task.CompletedTask,
                        _ =>
                            Task.CompletedTask));

        // Assert
        Assert.Equal(
            "loadDetailAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullApprove_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new ApprovalPendingDetailViewModel(
                        ScheduleId,
                        OperatorId,
                        () =>
                            Task.FromResult<
                                InspectionDetailData?>(
                                null),
                        null!,
                        _ =>
                            Task.CompletedTask));

        // Assert
        Assert.Equal(
            "approveAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullReturn_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new ApprovalPendingDetailViewModel(
                        ScheduleId,
                        OperatorId,
                        () =>
                            Task.FromResult<
                                InspectionDetailData?>(
                                null),
                        () =>
                            Task.CompletedTask,
                        null!));

        // Assert
        Assert.Equal(
            "returnAsync",
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
            CreateViewModel();

        // Assert
        Assert.Equal(
            ScheduleId,
            sut.ScheduleId);

        Assert.Equal(
            "点検承認",
            sut.Title);

        Assert.Equal(
            "完了した点検内容を確認し、承認または差し戻しを行います。",
            sut.Description);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Null(
            sut.OperationMessage);

        Assert.Null(
            sut.OperationErrorMessage);

        Assert.Null(
            sut.InspectionId);

        Assert.False(
            sut.HasInspection);

        Assert.Empty(
            sut.Results);

        Assert.Empty(
            sut.Photos);

        Assert.Empty(
            sut.GeneralPhotos);

        Assert.False(
            sut.HasResults);

        Assert.False(
            sut.HasPhotos);

        Assert.False(
            sut.HasGeneralPhotos);

        Assert.False(
            sut.CanReview);

        Assert.False(
            sut.IsReturnDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.ReturnReason);

        Assert.Equal(
            "-",
            sut.ScheduledDateText);

        Assert.Equal(
            "-",
            sut.LocationDisplayName);

        Assert.Equal(
            "-",
            sut.EquipmentDisplayName);

        Assert.Equal(
            "0項目",
            sut.ResultCountText);

        Assert.Equal(
            0,
            sut.AbnormalCount);

        Assert.Equal(
            "異常なし",
            sut.AbnormalCountText);

        Assert.Equal(
            "0枚",
            sut.PhotoCountText);
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadCommand_LoadsInspectionDetail()
    {
        // Arrange
        var detail =
            CreateDetail();

        var sut =
            CreateViewModel(
                detail: detail);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            InspectionId,
            sut.InspectionId);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                18),
            sut.ScheduledDate);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "製造エリア",
            sut.LocationName);

        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "コンプレッサー",
            sut.EquipmentName);

        Assert.Equal(
            "日常点検",
            sut.TemplateName);

        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);

        Assert.Equal(
            InspectionStatus.Completed,
            sut.Status);

        Assert.Equal(
            "2026/08/18",
            sut.ScheduledDateText);

        Assert.Equal(
            "第1工場 / 製造エリア",
            sut.LocationDisplayName);

        Assert.Equal(
            "EQ-001  コンプレッサー",
            sut.EquipmentDisplayName);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.True(
            sut.HasInspection);

        Assert.True(
            sut.CanReview);
    }


    [Fact]
    public async Task LoadCommand_LoadsResultsAndPhotos()
    {
        // Arrange
        var detail =
            CreateDetail(
                results:
                [
                    CreateResult(
                        itemName:
                            "異音確認",
                        isAbnormal:
                            true),

                    CreateResult(
                        itemName:
                            "油量確認",
                        isAbnormal:
                            false)
                ],
                photos:
                [
                    CreatePhoto(
                        inspectionResultId:
                            Guid.NewGuid(),
                        relativePath:
                            "photos/result.jpg"),

                    CreatePhoto(
                        inspectionResultId:
                            null,
                        relativePath:
                            "photos/general.jpg")
                ]);

        var sut =
            CreateViewModel(
                detail: detail);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            2,
            sut.Results.Count);

        Assert.True(
            sut.HasResults);

        Assert.Equal(
            "2項目",
            sut.ResultCountText);

        Assert.Equal(
            1,
            sut.AbnormalCount);

        Assert.Equal(
            "異常 1件",
            sut.AbnormalCountText);

        Assert.Equal(
            2,
            sut.Photos.Count);

        Assert.True(
            sut.HasPhotos);

        Assert.Equal(
            "2枚",
            sut.PhotoCountText);

        Assert.Single(
            sut.GeneralPhotos);

        Assert.True(
            sut.HasGeneralPhotos);
    }


    [Fact]
    public async Task LoadCommand_WhenDetailIsNull_SetsErrorMessage()
    {
        // Arrange
        var sut =
            CreateViewModel(
                detail: null);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.Equal(
            "点検実施データが見つかりません。",
            sut.ErrorMessage);

        Assert.False(
            sut.HasInspection);

        Assert.False(
            sut.CanReview);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task LoadCommand_WhenLoaderThrows_SetsErrorMessage()
    {
        // Arrange
        var sut =
            new ApprovalPendingDetailViewModel(
                ScheduleId,
                OperatorId,
                () =>
                    throw new InvalidOperationException(
                        "読込テストエラー"),
                () =>
                    Task.CompletedTask,
                _ =>
                    Task.CompletedTask);

        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "承認対象の点検詳細を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "読込テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // CanReview
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        false)]
    [InlineData(
        InspectionStatus.InProgress,
        false)]
    [InlineData(
        InspectionStatus.Completed,
        true)]
    [InlineData(
        InspectionStatus.Approved,
        false)]
    [InlineData(
        InspectionStatus.Returned,
        false)]
    public void CanReview_ReturnsExpectedValue(
        InspectionStatus status,
        bool expected)
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.InspectionId =
            InspectionId;

        sut.Status =
            status;

        // Act
        var actual =
            sut.CanReview;

        // Assert
        Assert.Equal(
            expected,
            actual);
    }


    [Fact]
    public void CanReview_WhenInspectionIdIsNull_ReturnsFalse()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.InspectionId =
            null;

        sut.Status =
            InspectionStatus.Completed;

        // Assert
        Assert.False(
            sut.CanReview);
    }


    [Fact]
    public void CanReview_WhenLoading_ReturnsFalse()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.InspectionId =
            InspectionId;

        sut.Status =
            InspectionStatus.Completed;

        sut.IsLoading =
            true;

        // Assert
        Assert.False(
            sut.CanReview);
    }


    // ============================================
    // Status Display
    // ============================================

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
        InspectionStatus.Approved,
        "承認済み")]
    [InlineData(
        InspectionStatus.Returned,
        "差し戻し")]
    public void StatusText_ReturnsExpectedValue(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        sut.Status =
            status;

        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "#F1F5F9")]
    [InlineData(
        InspectionStatus.InProgress,
        "#DBEAFE")]
    [InlineData(
        InspectionStatus.Completed,
        "#FFEDD5")]
    [InlineData(
        InspectionStatus.Approved,
        "#DCFCE7")]
    [InlineData(
        InspectionStatus.Returned,
        "#FEE2E2")]
    public void StatusBackground_ReturnsExpectedValue(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        sut.Status =
            status;

        // Assert
        Assert.Equal(
            expected,
            sut.StatusBackground);
    }


    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "#475569")]
    [InlineData(
        InspectionStatus.InProgress,
        "#1D4ED8")]
    [InlineData(
        InspectionStatus.Completed,
        "#C2410C")]
    [InlineData(
        InspectionStatus.Approved,
        "#15803D")]
    [InlineData(
        InspectionStatus.Returned,
        "#B91C1C")]
    public void StatusForeground_ReturnsExpectedValue(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        sut.Status =
            status;

        // Assert
        Assert.Equal(
            expected,
            sut.StatusForeground);
    }


    // ============================================
    // Approve
    // ============================================

    [Fact]
    public async Task ApproveCommand_WhenReviewable_ApprovesAndRequestsBack()
    {
        // Arrange
        var approveCallCount =
            0;

        var backCallCount =
            0;

        var sut =
            CreateViewModel(
                approveAsync:
                    () =>
                    {
                        approveCallCount++;

                        return Task.CompletedTask;
                    });

        SetReviewable(
            sut);

        sut.BackRequested =
            () =>
                backCallCount++;

        // Act
        await sut.ApproveCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            1,
            approveCallCount);

        Assert.Equal(
            "点検を承認しました。",
            sut.OperationMessage);

        Assert.Null(
            sut.OperationErrorMessage);

        Assert.Equal(
            1,
            backCallCount);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task ApproveCommand_WhenNotReviewable_DoesNothing()
    {
        // Arrange
        var approveCallCount =
            0;

        var sut =
            CreateViewModel(
                approveAsync:
                    () =>
                    {
                        approveCallCount++;

                        return Task.CompletedTask;
                    });

        sut.Status =
            InspectionStatus.NotStarted;

        // Act
        await sut.ApproveCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            approveCallCount);

        Assert.Null(
            sut.OperationMessage);
    }


    [Fact]
    public async Task ApproveCommand_WhenApproveFails_SetsErrorMessage()
    {
        // Arrange
        var backCallCount =
            0;

        var sut =
            CreateViewModel(
                approveAsync:
                    () =>
                        throw new InvalidOperationException(
                            "承認テストエラー"));

        SetReviewable(
            sut);

        sut.BackRequested =
            () =>
                backCallCount++;

        // Act
        await sut.ApproveCommand
            .ExecuteAsync(null);

        // Assert
        Assert.NotNull(
            sut.OperationErrorMessage);

        Assert.Contains(
            "点検を承認できませんでした。",
            sut.OperationErrorMessage);

        Assert.Contains(
            "承認テストエラー",
            sut.OperationErrorMessage);

        Assert.Null(
            sut.OperationMessage);

        Assert.Equal(
            0,
            backCallCount);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // Open Return Dialog
    // ============================================

    [Fact]
    public void OpenReturnDialogCommand_WhenReviewable_OpensDialog()
    {
        // Arrange
        var sut =
            CreateViewModel();

        SetReviewable(
            sut);

        sut.ReturnReason =
            "既存理由";

        sut.OperationErrorMessage =
            "既存エラー";

        // Act
        sut.OpenReturnDialogCommand
            .Execute(null);

        // Assert
        Assert.True(
            sut.IsReturnDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.ReturnReason);

        Assert.Null(
            sut.OperationErrorMessage);
    }


    [Fact]
    public void OpenReturnDialogCommand_WhenNotReviewable_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.Status =
            InspectionStatus.NotStarted;

        // Act
        sut.OpenReturnDialogCommand
            .Execute(null);

        // Assert
        Assert.False(
            sut.IsReturnDialogOpen);
    }


    // ============================================
    // Cancel Return
    // ============================================

    [Fact]
    public void CancelReturnCommand_ClosesDialogAndClearsState()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.IsReturnDialogOpen =
            true;

        sut.ReturnReason =
            "差し戻し理由";

        sut.OperationErrorMessage =
            "エラー";

        // Act
        sut.CancelReturnCommand
            .Execute(null);

        // Assert
        Assert.False(
            sut.IsReturnDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.ReturnReason);

        Assert.Null(
            sut.OperationErrorMessage);
    }


    // ============================================
    // Confirm Return
    // ============================================

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    public async Task ConfirmReturnCommand_WithBlankReason_SetsValidationError(
        string reason)
    {
        // Arrange
        var returnCallCount =
            0;

        var sut =
            CreateViewModel(
                returnAsync:
                    _ =>
                    {
                        returnCallCount++;

                        return Task.CompletedTask;
                    });

        SetReviewable(
            sut);

        sut.IsReturnDialogOpen =
            true;

        sut.ReturnReason =
            reason;

        // Act
        await sut.ConfirmReturnCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            returnCallCount);

        Assert.Equal(
            "差し戻し理由を入力してください。",
            sut.OperationErrorMessage);

        Assert.True(
            sut.IsReturnDialogOpen);
    }


    [Fact]
    public async Task ConfirmReturnCommand_WhenReviewable_ReturnsAndRequestsBack()
    {
        // Arrange
        string? receivedReason =
            null;

        var returnCallCount =
            0;

        var backCallCount =
            0;

        var sut =
            CreateViewModel(
                returnAsync:
                    reason =>
                    {
                        returnCallCount++;

                        receivedReason =
                            reason;

                        return Task.CompletedTask;
                    });

        SetReviewable(
            sut);

        sut.IsReturnDialogOpen =
            true;

        sut.ReturnReason =
            "異常箇所を再確認してください。";

        sut.BackRequested =
            () =>
                backCallCount++;

        // Act
        await sut.ConfirmReturnCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            1,
            returnCallCount);

        Assert.Equal(
            "異常箇所を再確認してください。",
            receivedReason);

        Assert.False(
            sut.IsReturnDialogOpen);

        Assert.Equal(
            "点検を差し戻しました。",
            sut.OperationMessage);

        Assert.Null(
            sut.OperationErrorMessage);

        Assert.Equal(
            1,
            backCallCount);

        Assert.False(
            sut.IsLoading);
    }


    [Fact]
    public async Task ConfirmReturnCommand_WhenNotReviewable_DoesNothing()
    {
        // Arrange
        var returnCallCount =
            0;

        var sut =
            CreateViewModel(
                returnAsync:
                    _ =>
                    {
                        returnCallCount++;

                        return Task.CompletedTask;
                    });

        sut.ReturnReason =
            "差し戻し理由";

        // Act
        await sut.ConfirmReturnCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            0,
            returnCallCount);

        Assert.Null(
            sut.OperationMessage);
    }


    [Fact]
    public async Task ConfirmReturnCommand_WhenReturnFails_SetsErrorMessage()
    {
        // Arrange
        var backCallCount =
            0;

        var sut =
            CreateViewModel(
                returnAsync:
                    _ =>
                        throw new InvalidOperationException(
                            "差し戻しテストエラー"));

        SetReviewable(
            sut);

        sut.IsReturnDialogOpen =
            true;

        sut.ReturnReason =
            "再確認してください。";

        sut.BackRequested =
            () =>
                backCallCount++;

        // Act
        await sut.ConfirmReturnCommand
            .ExecuteAsync(null);

        // Assert
        Assert.NotNull(
            sut.OperationErrorMessage);

        Assert.Contains(
            "点検を差し戻せませんでした。",
            sut.OperationErrorMessage);

        Assert.Contains(
            "差し戻しテストエラー",
            sut.OperationErrorMessage);

        Assert.Null(
            sut.OperationMessage);

        Assert.Equal(
            0,
            backCallCount);

        Assert.True(
            sut.IsReturnDialogOpen);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // Back
    // ============================================

    [Fact]
    public void BackCommand_InvokesBackRequested()
    {
        // Arrange
        var callCount =
            0;

        var sut =
            CreateViewModel();

        sut.BackRequested =
            () =>
                callCount++;

        // Act
        sut.BackCommand
            .Execute(null);

        // Assert
        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public void BackCommand_WithNoSubscriber_DoesNotThrow()
    {
        // Arrange
        var sut =
            CreateViewModel();

        // Act
        var exception =
            Record.Exception(
                () =>
                    sut.BackCommand
                        .Execute(null));

        // Assert
        Assert.Null(
            exception);
    }


    // ============================================
    // Helpers
    // ============================================

    private static ApprovalPendingDetailViewModel
        CreateViewModel(
            InspectionDetailData? detail = null,
            Func<Task>? approveAsync = null,
            Func<string, Task>? returnAsync = null)
    {
        return new ApprovalPendingDetailViewModel(
            ScheduleId,
            OperatorId,

            loadDetailAsync:
                () =>
                    Task.FromResult<
                        InspectionDetailData?>(
                        detail),

            approveAsync:
                approveAsync ??
                (() =>
                    Task.CompletedTask),

            returnAsync:
                returnAsync ??
                (_ =>
                    Task.CompletedTask));
    }


    private static void SetReviewable(
        ApprovalPendingDetailViewModel sut)
    {
        sut.InspectionId =
            InspectionId;

        sut.Status =
            InspectionStatus.Completed;

        sut.IsLoading =
            false;
    }


    private static InspectionDetailData
        CreateDetail(
            InspectionStatus status =
                InspectionStatus.Completed,
            IReadOnlyList<
                InspectionResultDetailData>? results = null,
            IReadOnlyList<
                InspectionPhotoDetailData>? photos = null)
    {
        return new InspectionDetailData(
            ScheduleId:
                ScheduleId,

            InspectionId:
                InspectionId,

            ScheduledDate:
                new DateOnly(
                    2026,
                    8,
                    18),

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

            Results:
                results ??
                [],

            Photos:
                photos ??
                []);
    }


    private static InspectionResultDetailData
        CreateResult(
            string itemName,
            bool isAbnormal)
    {
        return new InspectionResultDetailData(
            ResultId:
                Guid.NewGuid(),

            DisplayOrder:
                1,

            ItemName:
                itemName,

            InputType:
                InspectionInputType.NormalAbnormal,

            CheckValue:
                !isAbnormal,

            NumericValue:
                null,

            TextValue:
                null,

            Unit:
                null,

            IsAbnormal:
                isAbnormal,

            Comment:
                isAbnormal
                    ? "異常あり"
                    : null);
    }


    private static InspectionPhotoDetailData
        CreatePhoto(
            Guid? inspectionResultId,
            string relativePath)
    {
        return new InspectionPhotoDetailData(
            PhotoId:
                Guid.NewGuid(),

            InspectionResultId:
                inspectionResultId,

            RelativePath:
                relativePath,

            Caption:
                null,

            DisplayOrder:
                1,

            CapturedAtUtc:
                new DateTime(
                    2026,
                    8,
                    18,
                    1,
                    0,
                    0,
                    DateTimeKind.Utc));
    }
}