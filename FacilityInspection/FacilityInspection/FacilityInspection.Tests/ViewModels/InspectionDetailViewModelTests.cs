using FacilityInspection.Data;
using FacilityInspection.Domain.Inspections;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class InspectionDetailViewModelTests
{
    private static readonly Guid
        ScheduleId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");

    private static readonly Guid
        InspectionId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


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
                    new InspectionDetailViewModel(
                        Guid.Empty,
                        () =>
                            Task.FromResult<
                                InspectionDetailData?>(
                                null)));

        // Assert
        Assert.Equal(
            "scheduleId",
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
                    new InspectionDetailViewModel(
                        ScheduleId,
                        null!));

        // Assert
        Assert.Equal(
            "loadDetailAsync",
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
            "点検実施詳細",
            sut.Title);

        Assert.Equal(
            "点検の実施内容、異常、写真を確認します。",
            sut.Description);

        Assert.False(
            sut.IsLoading);

        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.InspectionId);

        Assert.False(
            sut.HasInspection);

        Assert.Equal(
            default,
            sut.ScheduledDate);

        Assert.Equal(
            "-",
            sut.ScheduledDateText);

        Assert.Equal(
            string.Empty,
            sut.FactorySiteName);

        Assert.Equal(
            string.Empty,
            sut.LocationName);

        Assert.Equal(
            "-",
            sut.LocationDisplayName);

        Assert.Equal(
            string.Empty,
            sut.EquipmentCode);

        Assert.Equal(
            string.Empty,
            sut.EquipmentName);

        Assert.Equal(
            "-",
            sut.EquipmentDisplayName);

        Assert.Equal(
            string.Empty,
            sut.TemplateName);

        Assert.Equal(
            string.Empty,
            sut.OperatorName);

        Assert.Equal(
            InspectionStatus.NotStarted,
            sut.Status);

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
    public async Task LoadCommand_WhenSuccessful_LoadsBasicInformation()
    {
        // Arrange
        var detail =
            CreateDetail(
                scheduledDate:
                    new DateOnly(
                        2026,
                        8,
                        19),

                factorySiteName:
                    "第1工場",

                locationName:
                    "コンプレッサー室",

                equipmentCode:
                    "EQ-001",

                equipmentName:
                    "エアコンプレッサー1号",

                templateName:
                    "コンプレッサー日常点検",

                operatorName:
                    "点検担当者A",

                status:
                    InspectionStatus.Completed);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            InspectionId,
            sut.InspectionId);

        Assert.True(
            sut.HasInspection);

        Assert.Equal(
            new DateOnly(
                2026,
                8,
                19),
            sut.ScheduledDate);

        Assert.Equal(
            "2026/08/19",
            sut.ScheduledDateText);

        Assert.Equal(
            "第1工場",
            sut.FactorySiteName);

        Assert.Equal(
            "コンプレッサー室",
            sut.LocationName);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationDisplayName);

        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "エアコンプレッサー1号",
            sut.EquipmentName);

        Assert.Equal(
            "EQ-001  エアコンプレッサー1号",
            sut.EquipmentDisplayName);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.TemplateName);

        Assert.Equal(
            "点検担当者A",
            sut.OperatorName);

        Assert.Equal(
            InspectionStatus.Completed,
            sut.Status);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // Results
    // ============================================

    [Fact]
    public async Task LoadCommand_LoadsResultsAndCalculatesAbnormalCount()
    {
        // Arrange
        var detail =
            CreateDetail(
                results:
                    [
                        CreateResult(
                            displayOrder:
                                1,
                            itemName:
                                "圧力確認",
                            isAbnormal:
                                false),

                        CreateResult(
                            displayOrder:
                                2,
                            itemName:
                                "異音確認",
                            isAbnormal:
                                true),

                        CreateResult(
                            displayOrder:
                                3,
                            itemName:
                                "油量確認",
                            isAbnormal:
                                true)
                    ]);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            3,
            sut.Results.Count);

        Assert.True(
            sut.HasResults);

        Assert.Equal(
            "3項目",
            sut.ResultCountText);

        Assert.Equal(
            2,
            sut.AbnormalCount);

        Assert.Equal(
            "異常 2件",
            sut.AbnormalCountText);
    }


    [Fact]
    public async Task LoadCommand_WithNoAbnormalResults_ShowsNoAbnormalText()
    {
        // Arrange
        var detail =
            CreateDetail(
                results:
                    [
                        CreateResult(
                            isAbnormal:
                                false),

                        CreateResult(
                            displayOrder:
                                2,
                            isAbnormal:
                                false)
                    ]);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            sut.AbnormalCount);

        Assert.Equal(
            "異常なし",
            sut.AbnormalCountText);
    }


    // ============================================
    // Photos
    // ============================================

    [Fact]
    public async Task LoadCommand_LoadsPhotosAndSeparatesGeneralPhotos()
    {
        // Arrange
        var generalPhoto =
            CreatePhoto(
                isGeneralPhoto:
                    true,
                relativePath:
                    "photos/general.jpg");

        var resultPhoto =
            CreatePhoto(
                isGeneralPhoto:
                    false,
                relativePath:
                    "photos/result.jpg");


        var detail =
            CreateDetail(
                photos:
                    [
                        generalPhoto,
                        resultPhoto
                    ]);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
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

        /*
         * 全写真のうち一般写真として
         * 判定された同一ViewModelが
         * GeneralPhotosにも格納される。
         */
        Assert.Same(
            sut.Photos[0],
            sut.GeneralPhotos[0]);
    }


    [Fact]
    public async Task LoadCommand_WhenNoGeneralPhotos_SetsHasGeneralPhotosFalse()
    {
        // Arrange
        var detail =
            CreateDetail(
                photos:
                    [
                        CreatePhoto(
                            isGeneralPhoto:
                                false),

                        CreatePhoto(
                            isGeneralPhoto:
                                false)
                    ]);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            2,
            sut.Photos.Count);

        Assert.Empty(
            sut.GeneralPhotos);

        Assert.True(
            sut.HasPhotos);

        Assert.False(
            sut.HasGeneralPhotos);
    }


    // ============================================
    // Not Started
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenInspectionDoesNotExist_LoadsScheduleOnly()
    {
        // Arrange
        var detail =
            CreateDetail(
                hasInspection:
                    false,
                status:
                    InspectionStatus.NotStarted,
                results:
                    [],
                photos:
                    []);


        var sut =
            CreateViewModel(
                detail);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Null(
            sut.InspectionId);

        Assert.False(
            sut.HasInspection);

        Assert.Equal(
            InspectionStatus.NotStarted,
            sut.Status);

        Assert.Equal(
            "未実施",
            sut.StatusText);

        Assert.Empty(
            sut.Results);

        Assert.Empty(
            sut.Photos);

        Assert.False(
            sut.HasResults);

        Assert.False(
            sut.HasPhotos);

        Assert.Equal(
            "0項目",
            sut.ResultCountText);

        Assert.Equal(
            "0枚",
            sut.PhotoCountText);
    }


    // ============================================
    // Replace
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenReloaded_ReplacesPreviousCollections()
    {
        // Arrange
        var callCount =
            0;


        var first =
            CreateDetail(
                results:
                    [
                        CreateResult(
                            displayOrder:
                                1),

                        CreateResult(
                            displayOrder:
                                2),

                        CreateResult(
                            displayOrder:
                                3)
                    ],
                photos:
                    [
                        CreatePhoto(
                            isGeneralPhoto:
                                true),

                        CreatePhoto(
                            isGeneralPhoto:
                                false)
                    ]);


        var second =
            CreateDetail(
                results:
                    [
                        CreateResult(
                            displayOrder:
                                1)
                    ],
                photos:
                    [
                        CreatePhoto(
                            isGeneralPhoto:
                                false)
                    ]);


        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                {
                    callCount++;

                    return Task.FromResult<
                        InspectionDetailData?>(
                        callCount == 1
                            ? first
                            : second);
                });


        await sut.LoadCommand
            .ExecuteAsync(null);


        Assert.Equal(
            3,
            sut.Results.Count);

        Assert.Equal(
            2,
            sut.Photos.Count);

        Assert.Single(
            sut.GeneralPhotos);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Single(
            sut.Results);

        Assert.Single(
            sut.Photos);

        Assert.Empty(
            sut.GeneralPhotos);

        Assert.Equal(
            "1項目",
            sut.ResultCountText);

        Assert.Equal(
            "1枚",
            sut.PhotoCountText);
    }


    // ============================================
    // Null
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenDetailIsNull_SetsNotFoundError()
    {
        // Arrange
        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                    Task.FromResult<
                        InspectionDetailData?>(
                        null));


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
            sut.IsLoading);
    }


    // ============================================
    // Exception
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenLoaderThrows_SetsErrorMessage()
    {
        // Arrange
        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                    Task.FromException<
                        InspectionDetailData?>(
                        new InvalidOperationException(
                            "詳細取得テストエラー")));


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検実施詳細を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "詳細取得テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);
    }


    // ============================================
    // Error Recovery
    // ============================================

    [Fact]
    public async Task LoadCommand_AfterPreviousError_ClearsErrorMessage()
    {
        // Arrange
        var callCount =
            0;


        var detail =
            CreateDetail();


        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        return Task.FromException<
                            InspectionDetailData?>(
                            new InvalidOperationException(
                                "1回目エラー"));
                    }

                    return Task.FromResult<
                        InspectionDetailData?>(
                        detail);
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

        Assert.True(
            sut.HasInspection);
    }


    // ============================================
    // Already Loading
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenAlreadyLoading_DoesNotInvokeLoader()
    {
        // Arrange
        var callCount =
            0;


        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                {
                    callCount++;

                    return Task.FromResult<
                        InspectionDetailData?>(
                        CreateDetail());
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
    // Existing Data On Error
    // ============================================

    [Fact]
    public async Task LoadCommand_WhenReloadFails_KeepsPreviousDetail()
    {
        // Arrange
        var callCount =
            0;


        var first =
            CreateDetail(
                equipmentCode:
                    "EQ-001",
                equipmentName:
                    "既存設備",

                results:
                    [
                        CreateResult()
                    ]);


        var sut =
            new InspectionDetailViewModel(
                ScheduleId,
                () =>
                {
                    callCount++;

                    if (callCount == 1)
                    {
                        return Task.FromResult<
                            InspectionDetailData?>(
                            first);
                    }

                    return Task.FromException<
                        InspectionDetailData?>(
                        new InvalidOperationException(
                            "再読込エラー"));
                });


        await sut.LoadCommand
            .ExecuteAsync(null);


        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Single(
            sut.Results);


        // Act
        await sut.LoadCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.HasError);

        /*
         * 現行仕様ではロード開始時に
         * 既存データをClearしないため、
         * 再読込失敗時は以前の表示を残す。
         */
        Assert.Equal(
            "EQ-001",
            sut.EquipmentCode);

        Assert.Equal(
            "既存設備",
            sut.EquipmentName);

        Assert.Single(
            sut.Results);
    }


    // ============================================
    // Status Display
    // ============================================

    [Theory]
    [InlineData(
        InspectionStatus.NotStarted,
        "未実施",
        "#F1F5F9",
        "#475569")]
    [InlineData(
        InspectionStatus.InProgress,
        "実施中",
        "#DBEAFE",
        "#1D4ED8")]
    [InlineData(
        InspectionStatus.Completed,
        "完了・承認待ち",
        "#FFEDD5",
        "#C2410C")]
    [InlineData(
        InspectionStatus.Approved,
        "承認済み",
        "#DCFCE7",
        "#15803D")]
    [InlineData(
        InspectionStatus.Returned,
        "差し戻し",
        "#FEE2E2",
        "#B91C1C")]
    public void StatusDisplay_ReturnsExpectedValues(
        InspectionStatus status,
        string expectedText,
        string expectedBackground,
        string expectedForeground)
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.Status =
            status;


        // Assert
        Assert.Equal(
            expectedText,
            sut.StatusText);

        Assert.Equal(
            expectedBackground,
            sut.StatusBackground);

        Assert.Equal(
            expectedForeground,
            sut.StatusForeground);
    }


    // ============================================
    // Back
    // ============================================

    [Fact]
    public void BackCommand_RaisesBackRequested()
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

    private static InspectionDetailViewModel
        CreateViewModel(
            InspectionDetailData? detail = null)
    {
        detail ??=
            CreateDetail();


        return new InspectionDetailViewModel(
            ScheduleId,
            () =>
                Task.FromResult<
                    InspectionDetailData?>(
                    detail));
    }


    private static InspectionDetailData
        CreateDetail(
            Guid? scheduleId = null,
            Guid? inspectionId = null,
            bool hasInspection = true,
            DateOnly? scheduledDate = null,
            string factorySiteName =
                "第1工場",
            string locationName =
                "コンプレッサー室",
            string equipmentCode =
                "EQ-001",
            string equipmentName =
                "コンプレッサー1号",
            string templateName =
                "日常点検",
            string operatorName =
                "点検担当者A",
            InspectionStatus status =
                InspectionStatus.Completed,
            IReadOnlyList<
                InspectionResultDetailData>?
                results = null,
            IReadOnlyList<
                InspectionPhotoDetailData>?
                photos = null)
    {
        Guid? actualInspectionId =
            hasInspection
                ? inspectionId ??
                  InspectionId
                : null;


        return new InspectionDetailData(
            ScheduleId:
                scheduleId ??
                ScheduleId,

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

            Results:
                results ??
                [],

            Photos:
                photos ??
                []);
    }


    private static InspectionResultDetailData
        CreateResult(
            Guid? resultId = null,
            int displayOrder = 1,
            string itemName =
                "圧力確認",
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            bool? checkValue = true,
            decimal? numericValue = null,
            string? textValue = null,
            string? unit = null,
            bool isAbnormal = false,
            string? comment = null)
    {
        return new InspectionResultDetailData(
            ResultId:
                resultId ??
                Guid.NewGuid(),

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

            IsAbnormal:
                isAbnormal,

            Comment:
                comment);
    }


    private static InspectionPhotoDetailData
        CreatePhoto(
            Guid? photoId = null,
            bool isGeneralPhoto = false,
            Guid? inspectionResultId = null,
            string relativePath =
                "photos/test.jpg",
            string? caption =
                "点検写真",
            int displayOrder = 1,
            DateTime? capturedAtUtc = null)
    {
        Guid? actualInspectionResultId =
            isGeneralPhoto
                ? null
                : inspectionResultId ??
                  Guid.NewGuid();


        return new InspectionPhotoDetailData(
            PhotoId:
                photoId ??
                Guid.NewGuid(),

            InspectionResultId:
                actualInspectionResultId,

            RelativePath:
                relativePath,

            Caption:
                caption,

            DisplayOrder:
                displayOrder,

            CapturedAtUtc:
                capturedAtUtc ??
                new DateTime(
                    2026,
                    8,
                    19,
                    0,
                    0,
                    0,
                    DateTimeKind.Utc));
    }
}