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

public sealed class InspectionEntryViewModelTests
{
    private static readonly Guid
        ScheduleId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE");


    private static readonly Guid
        ChoiceItemId =
            Guid.Parse(
                "10000000-0000-0000-0000-000000000001");


    private static readonly Guid
        NumericItemId =
            Guid.Parse(
                "10000000-0000-0000-0000-000000000002");


    private static readonly Guid
        TextItemId =
            Guid.Parse(
                "10000000-0000-0000-0000-000000000003");


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
                    CreateViewModel(
                        scheduleId:
                            Guid.Empty));


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
                    CreateViewModel(
                        operatorId:
                            Guid.Empty));


        // Assert
        Assert.Equal(
            "operatorId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullStartOrResume_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new InspectionEntryViewModel(
                        ScheduleId,
                        OperatorId,
                        null!,
                        _ =>
                            Task.CompletedTask,
                        _ =>
                        {
                        },
                        () =>
                        {
                        }));


        // Assert
        Assert.Equal(
            "startOrResumeAsync",
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
            "点検実施",
            sut.Title);

        Assert.Equal(
            "点検項目を確認し、現場の状態を入力します。",
            sut.Description);


        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.IsNotSaving);

        Assert.True(
            sut.IsContentVisible);


        Assert.Null(
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);


        Assert.Null(
            sut.ValidationMessage);

        Assert.False(
            sut.HasValidationMessage);


        Assert.Null(
            sut.CompletionErrorMessage);

        Assert.False(
            sut.HasCompletionError);


        Assert.False(
            sut.IsCompletionConfirmVisible);

        Assert.False(
            sut.IsCompletionSuccessVisible);


        Assert.Empty(
            sut.Items);


        Assert.Equal(
            string.Empty,
            sut.ScheduledDateText);

        Assert.Equal(
            string.Empty,
            sut.LocationText);

        Assert.Equal(
            string.Empty,
            sut.EquipmentText);

        Assert.Equal(
            string.Empty,
            sut.TemplateName);

        Assert.Equal(
            string.Empty,
            sut.StatusText);


        Assert.NotNull(
            sut.BackCommand);

        Assert.NotNull(
            sut.ReviewCompletionCommand);

        Assert.NotNull(
            sut.CancelCompletionCommand);

        Assert.NotNull(
            sut.ConfirmCompletionCommand);

        Assert.NotNull(
            sut.FinishCompletionCommand);
    }


    // ============================================
    // Initialize Success
    // ============================================

    [Fact]
    public async Task InitializeAsync_WhenSuccessful_LoadsDisplayAndItems()
    {
        // Arrange
        var entryData =
            CreateEntryData(
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
                    "コンプレッサー1号",

                templateName:
                    "コンプレッサー日常点検",

                status:
                    InspectionStatus.InProgress,

                items:
                    [
                        CreateChoiceItemData(
                            templateItemId:
                                ChoiceItemId,
                            displayOrder:
                                1,
                            itemName:
                                "異音確認",
                            checkValue:
                                true),

                        CreateNumericItemData(
                            templateItemId:
                                NumericItemId,
                            displayOrder:
                                2,
                            itemName:
                                "吐出圧力",
                            numericValue:
                                0.75m)
                    ]);


        var sut =
            CreateViewModel(
                startOrResumeAsync:
                    () =>
                        Task.FromResult(
                            entryData));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsContentVisible);

        Assert.False(
            sut.HasError);


        Assert.Equal(
            "2026年8月19日",
            sut.ScheduledDateText);

        Assert.Equal(
            "第1工場 / コンプレッサー室",
            sut.LocationText);

        Assert.Equal(
            "EQ-001 コンプレッサー1号",
            sut.EquipmentText);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.TemplateName);

        Assert.Equal(
            "実施中",
            sut.StatusText);


        Assert.Equal(
            2,
            sut.Items.Count);

        Assert.Equal(
            ChoiceItemId,
            sut.Items[0].TemplateItemId);

        Assert.Equal(
            NumericItemId,
            sut.Items[1].TemplateItemId);
    }


    // ============================================
    // Status
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
        InspectionStatus.Returned,
        "差し戻し")]
    [InlineData(
        InspectionStatus.Approved,
        "承認済み")]
    public async Task InitializeAsync_Status_ReturnsExpectedText(
        InspectionStatus status,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                startOrResumeAsync:
                    () =>
                        Task.FromResult(
                            CreateEntryData(
                                status:
                                    status)));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    // ============================================
    // Loading
    // ============================================

    [Fact]
    public async Task InitializeAsync_WhileLoading_SetsLoadingState()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource<
                InspectionEntryData>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                startOrResumeAsync:
                    () =>
                        completionSource.Task);


        // Act
        var initializeTask =
            sut.InitializeAsync();


        // Assert
        Assert.True(
            sut.IsLoading);

        Assert.False(
            sut.IsContentVisible);


        completionSource.SetResult(
            CreateEntryData());


        await initializeTask;


        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.IsContentVisible);
    }


    // ============================================
    // Initialize Failure
    // ============================================

    [Fact]
    public async Task InitializeAsync_WhenStartFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                startOrResumeAsync:
                    () =>
                        Task.FromException<
                            InspectionEntryData>(
                            new InvalidOperationException(
                                "開始テストエラー")));


        // Act
        await sut.InitializeAsync();


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.False(
            sut.IsContentVisible);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検を開始できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "開始テストエラー",
            sut.ErrorMessage);
    }


    // ============================================
    // Review Completion
    // ============================================

    [Fact]
    public void ReviewCompletionCommand_WhenAllItemsValid_OpensConfirmation()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.Items.Add(
            CreateChoiceItemViewModel(
                checkValue:
                    true));


        sut.Items.Add(
            CreateNumericItemViewModel(
                numericValue:
                    12.5m));


        // Act
        sut.ReviewCompletionCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.HasValidationMessage);

        Assert.False(
            sut.HasCompletionError);

        Assert.True(
            sut.IsCompletionConfirmVisible);
    }


    [Fact]
    public void ReviewCompletionCommand_WithOneInvalidItem_ShowsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.Items.Add(
            CreateChoiceItemViewModel(
                isRequired:
                    true,
                checkValue:
                    null));


        // Act
        sut.ReviewCompletionCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsCompletionConfirmVisible);

        Assert.True(
            sut.HasValidationMessage);

        Assert.Equal(
            "入力内容に 1 件のエラーがあります。" +
            "赤字の項目を修正してください。",
            sut.ValidationMessage);


        Assert.True(
            sut.Items[0]
                .HasValidationError);
    }


    [Fact]
    public void ReviewCompletionCommand_WithMultipleInvalidItems_ShowsCorrectErrorCount()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.Items.Add(
            CreateChoiceItemViewModel(
                templateItemId:
                    ChoiceItemId,
                isRequired:
                    true,
                checkValue:
                    null));


        sut.Items.Add(
            CreateNumericItemViewModel(
                templateItemId:
                    NumericItemId,
                isRequired:
                    true,
                numericValue:
                    null));


        sut.Items.Add(
            CreateTextItemViewModel(
                templateItemId:
                    TextItemId,
                isRequired:
                    true,
                textValue:
                    null));


        // Act
        sut.ReviewCompletionCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsCompletionConfirmVisible);

        Assert.Equal(
            "入力内容に 3 件のエラーがあります。" +
            "赤字の項目を修正してください。",
            sut.ValidationMessage);
    }


    // ============================================
    // Confirm Without Review
    // ============================================

    [Fact]
    public async Task ConfirmCompletionCommand_WithoutReview_ShowsError()
    {
        // Arrange
        var completeCallCount =
            0;


        var sut =
            CreateViewModel(
                completeAsync:
                    _ =>
                    {
                        completeCallCount++;

                        return Task.CompletedTask;
                    });


        // Act
        await sut.ConfirmCompletionCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            completeCallCount);

        Assert.True(
            sut.HasCompletionError);

        Assert.Equal(
            "完了対象の入力内容を確認できませんでした。" +
            "いったんキャンセルして、もう一度入力内容を確認してください。",
            sut.CompletionErrorMessage);
    }


    // ============================================
    // Confirm Success
    // ============================================

    [Fact]
    public async Task ConfirmCompletionCommand_WhenSuccessful_PassesCompletionDataAndShowsSuccess()
    {
        // Arrange
        IReadOnlyCollection<
            InspectionCompletionItemData>?
            receivedItems =
                null;


        var sut =
            CreateViewModel(
                completeAsync:
                    items =>
                    {
                        receivedItems =
                            items.ToList();

                        return Task.CompletedTask;
                    });


        var choiceItem =
            CreateChoiceItemViewModel(
                templateItemId:
                    ChoiceItemId,
                checkValue:
                    false);


        choiceItem.Comment =
            "  異音あり  ";


        var numericItem =
            CreateNumericItemViewModel(
                templateItemId:
                    NumericItemId,
                numericValue:
                    12.5m);


        var textItem =
            CreateTextItemViewModel(
                templateItemId:
                    TextItemId,
                textValue:
                    "  清掃済み  ");


        var capturedAtUtc =
            new DateTime(
                2026,
                8,
                19,
                1,
                0,
                0,
                DateTimeKind.Utc);


        choiceItem.AddPhoto(
            "abnormal.jpg",
            "photos/abnormal.jpg",
            capturedAtUtc);


        sut.Items.Add(
            choiceItem);

        sut.Items.Add(
            numericItem);

        sut.Items.Add(
            textItem);


        sut.ReviewCompletionCommand
            .Execute(null);


        Assert.True(
            sut.IsCompletionConfirmVisible);


        // Act
        await sut.ConfirmCompletionCommand
            .ExecuteAsync(null);


        // Assert
        Assert.NotNull(
            receivedItems);

        Assert.Equal(
            3,
            receivedItems.Count);


        var choiceResult =
            receivedItems.Single(
                x =>
                    x.TemplateItemId ==
                    ChoiceItemId);


        Assert.Equal(
            false,
            choiceResult.CheckValue);

        Assert.Null(
            choiceResult.NumericValue);

        Assert.Null(
            choiceResult.TextValue);

        Assert.Equal(
            "異音あり",
            choiceResult.Comment);

        Assert.Single(
            choiceResult.Photos);

        Assert.Equal(
            "photos/abnormal.jpg",
            choiceResult.Photos[0]
                .RelativePath);

        Assert.Equal(
            capturedAtUtc,
            choiceResult.Photos[0]
                .CapturedAtUtc);


        var numericResult =
            receivedItems.Single(
                x =>
                    x.TemplateItemId ==
                    NumericItemId);


        Assert.Equal(
            12.5m,
            numericResult.NumericValue);


        var textResult =
            receivedItems.Single(
                x =>
                    x.TemplateItemId ==
                    TextItemId);


        Assert.Equal(
            "清掃済み",
            textResult.TextValue);


        Assert.Equal(
            "完了・承認待ち",
            sut.StatusText);

        Assert.False(
            sut.IsCompletionConfirmVisible);

        Assert.True(
            sut.IsCompletionSuccessVisible);

        Assert.False(
            sut.HasCompletionError);

        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.IsNotSaving);
    }


    // ============================================
    // Confirm Failure
    // ============================================

    [Fact]
    public async Task ConfirmCompletionCommand_WhenCompleteFails_ShowsErrorAndKeepsConfirmation()
    {
        // Arrange
        var sut =
            CreateViewModel(
                completeAsync:
                    _ =>
                        Task.FromException(
                            new InvalidOperationException(
                                "完了テストエラー")));


        sut.Items.Add(
            CreateChoiceItemViewModel(
                checkValue:
                    true));


        sut.ReviewCompletionCommand
            .Execute(null);


        // Act
        await sut.ConfirmCompletionCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.IsCompletionConfirmVisible);

        Assert.False(
            sut.IsCompletionSuccessVisible);

        Assert.True(
            sut.HasCompletionError);

        Assert.NotNull(
            sut.CompletionErrorMessage);

        Assert.Contains(
            "点検を完了できませんでした。",
            sut.CompletionErrorMessage);

        Assert.Contains(
            "完了テストエラー",
            sut.CompletionErrorMessage);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // Cancel
    // ============================================

    [Fact]
    public async Task CancelCompletionCommand_ClearsConfirmationAndPendingItems()
    {
        // Arrange
        var completeCallCount =
            0;


        var sut =
            CreateViewModel(
                completeAsync:
                    _ =>
                    {
                        completeCallCount++;

                        return Task.CompletedTask;
                    });


        sut.Items.Add(
            CreateChoiceItemViewModel(
                checkValue:
                    true));


        sut.ReviewCompletionCommand
            .Execute(null);


        Assert.True(
            sut.IsCompletionConfirmVisible);


        // Act
        sut.CancelCompletionCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsCompletionConfirmVisible);

        Assert.False(
            sut.HasCompletionError);


        /*
         * Pendingも破棄されているため、
         * そのままConfirmしても保存されない。
         */
        await sut.ConfirmCompletionCommand
            .ExecuteAsync(null);


        Assert.Equal(
            0,
            completeCallCount);

        Assert.True(
            sut.HasCompletionError);
    }


    // ============================================
    // Saving State
    // ============================================

    [Fact]
    public async Task ConfirmCompletionCommand_WhileSaving_DisablesBackAndCancel()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource<bool>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var backCallCount =
            0;

        var cleanupCallCount =
            0;


        var sut =
            CreateViewModel(
                completeAsync:
                    async _ =>
                    {
                        await completionSource.Task;
                    },

                cleanupUnsavedPhotos:
                    _ =>
                        cleanupCallCount++,

                backRequested:
                    () =>
                        backCallCount++);


        sut.Items.Add(
            CreateChoiceItemViewModel(
                checkValue:
                    true));


        sut.ReviewCompletionCommand
            .Execute(null);


        // Act
        var confirmTask =
            sut.ConfirmCompletionCommand
                .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.IsSaving);

        Assert.False(
            sut.IsNotSaving);


        sut.CancelCompletionCommand
            .Execute(null);

        sut.BackCommand
            .Execute(null);


        Assert.True(
            sut.IsCompletionConfirmVisible);

        Assert.Equal(
            0,
            cleanupCallCount);

        Assert.Equal(
            0,
            backCallCount);


        completionSource.SetResult(
            true);


        await confirmTask;


        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.IsNotSaving);
    }


    // ============================================
    // Finish
    // ============================================

    [Fact]
    public async Task FinishCompletionCommand_AfterSuccess_ClosesSuccessAndRequestsBack()
    {
        // Arrange
        var backCallCount =
            0;


        var sut =
            CreateViewModel(
                backRequested:
                    () =>
                        backCallCount++);


        sut.Items.Add(
            CreateChoiceItemViewModel(
                checkValue:
                    true));


        sut.ReviewCompletionCommand
            .Execute(null);


        await sut.ConfirmCompletionCommand
            .ExecuteAsync(null);


        Assert.True(
            sut.IsCompletionSuccessVisible);


        // Act
        sut.FinishCompletionCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsCompletionSuccessVisible);

        Assert.Equal(
            1,
            backCallCount);
    }


    // ============================================
    // Back
    // ============================================

    [Fact]
    public void BackCommand_CleansUpAllItemsAndRequestsBack()
    {
        // Arrange
        var cleanedItems =
            new List<
                InspectionEntryItemViewModel>();


        var backCallCount =
            0;


        var sut =
            CreateViewModel(
                cleanupUnsavedPhotos:
                    item =>
                        cleanedItems.Add(
                            item),

                backRequested:
                    () =>
                        backCallCount++);


        var first =
            CreateChoiceItemViewModel(
                templateItemId:
                    ChoiceItemId,
                checkValue:
                    true);


        var second =
            CreateNumericItemViewModel(
                templateItemId:
                    NumericItemId,
                numericValue:
                    10m);


        sut.Items.Add(
            first);

        sut.Items.Add(
            second);


        // Act
        sut.BackCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            cleanedItems.Count);

        Assert.Contains(
            first,
            cleanedItems);

        Assert.Contains(
            second,
            cleanedItems);

        Assert.Equal(
            1,
            backCallCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static InspectionEntryViewModel
        CreateViewModel(
            Guid? scheduleId = null,
            Guid? operatorId = null,
            Func<Task<InspectionEntryData>>?
                startOrResumeAsync = null,
            Func<
                IReadOnlyCollection<
                    InspectionCompletionItemData>,
                Task>?
                completeAsync = null,
            Action<InspectionEntryItemViewModel>?
                cleanupUnsavedPhotos = null,
            Action?
                backRequested = null)
    {
        return new InspectionEntryViewModel(
            scheduleId ??
                ScheduleId,

            operatorId ??
                OperatorId,

            startOrResumeAsync ??
                (() =>
                    Task.FromResult(
                        CreateEntryData())),

            completeAsync ??
                (_ =>
                    Task.CompletedTask),

            cleanupUnsavedPhotos ??
                (_ =>
                {
                }),

            backRequested ??
                (() =>
                {
                }));
    }


    private static InspectionEntryData
        CreateEntryData(
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
            InspectionStatus status =
                InspectionStatus.InProgress,
            IReadOnlyList<
                InspectionEntryItemData>?
                items = null)
    {
        return new InspectionEntryData(
            ScheduleId:
                ScheduleId,

            InspectionId:
                Guid.Parse(
                    "22222222-3333-4444-5555-666666666666"),

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

            Status:
                status,

            Items:
                items ??
                []);
    }


    private static InspectionEntryItemViewModel
        CreateChoiceItemViewModel(
            Guid? templateItemId = null,
            bool isRequired = true,
            bool? checkValue = true)
    {
        return new InspectionEntryItemViewModel(
            CreateChoiceItemData(
                templateItemId:
                    templateItemId,
                isRequired:
                    isRequired,
                checkValue:
                    checkValue));
    }


    private static InspectionEntryItemData
        CreateChoiceItemData(
            Guid? templateItemId = null,
            int displayOrder = 1,
            string itemName =
                "異音確認",
            bool isRequired = true,
            bool? checkValue = true)
    {
        return new InspectionEntryItemData(
            TemplateItemId:
                templateItemId ??
                ChoiceItemId,

            DisplayOrder:
                displayOrder,

            ItemName:
                itemName,

            InputType:
                InspectionInputType
                    .NormalAbnormal,

            Unit:
                null,

            MinimumValue:
                null,

            MaximumValue:
                null,

            IsRequired:
                isRequired,

            Description:
                null,

            CheckValue:
                checkValue,

            NumericValue:
                null,

            TextValue:
                null,

            Comment:
                null);
    }


    private static InspectionEntryItemViewModel
        CreateNumericItemViewModel(
            Guid? templateItemId = null,
            bool isRequired = true,
            decimal? numericValue = 10m)
    {
        return new InspectionEntryItemViewModel(
            CreateNumericItemData(
                templateItemId:
                    templateItemId,
                isRequired:
                    isRequired,
                numericValue:
                    numericValue));
    }


    private static InspectionEntryItemData
        CreateNumericItemData(
            Guid? templateItemId = null,
            int displayOrder = 2,
            string itemName =
                "吐出圧力",
            bool isRequired = true,
            decimal? numericValue = 10m)
    {
        return new InspectionEntryItemData(
            TemplateItemId:
                templateItemId ??
                NumericItemId,

            DisplayOrder:
                displayOrder,

            ItemName:
                itemName,

            InputType:
                InspectionInputType
                    .Numeric,

            Unit:
                "MPa",

            MinimumValue:
                0,

            MaximumValue:
                20,

            IsRequired:
                isRequired,

            Description:
                null,

            CheckValue:
                null,

            NumericValue:
                numericValue,

            TextValue:
                null,

            Comment:
                null);
    }


    private static InspectionEntryItemViewModel
        CreateTextItemViewModel(
            Guid? templateItemId = null,
            bool isRequired = true,
            string? textValue =
                "確認済み")
    {
        return new InspectionEntryItemViewModel(
            new InspectionEntryItemData(
                TemplateItemId:
                    templateItemId ??
                    TextItemId,

                DisplayOrder:
                    3,

                ItemName:
                    "備考",

                InputType:
                    InspectionInputType.Text,

                Unit:
                    null,

                MinimumValue:
                    null,

                MaximumValue:
                    null,

                IsRequired:
                    isRequired,

                Description:
                    null,

                CheckValue:
                    null,

                NumericValue:
                    null,

                TextValue:
                    textValue,

                Comment:
                    null));
    }
}