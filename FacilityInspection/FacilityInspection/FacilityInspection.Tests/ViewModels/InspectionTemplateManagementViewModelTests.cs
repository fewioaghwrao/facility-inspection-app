using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class
    InspectionTemplateManagementViewModelTests
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
                    new InspectionTemplateManagementViewModel(
                        (Func<
                            Task<
                                IReadOnlyList<
                                    InspectionTemplate>>>)
                        null!,
                        (_, _, _) =>
                            Task.FromResult(
                                Guid.NewGuid()),
                        (_, _, _) =>
                            Task.CompletedTask,
                        (_, _) =>
                            Task.CompletedTask));


        // Assert
        Assert.Equal(
            "loadTemplatesAsync",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_InitializesExpectedState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel();


        // Assert
        Assert.Empty(
            sut.Templates);

        Assert.Empty(
            sut.EditingItems);

        Assert.Null(
            sut.SelectedTemplate);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.True(
            sut.IsEmpty);

        Assert.False(
            sut.IsEditDialogOpen);

        Assert.False(
            sut.IsCreateMode);

        Assert.False(
            sut.IsSaving);

        Assert.False(
            sut.HasEditError);

        Assert.False(
            sut.HasOperationMessage);

        Assert.Equal(
            "エアコンプレッサー",
            sut.SelectedEquipmentTypeName);

        Assert.Collection(
            sut.EquipmentTypeChoices,
            x =>
                Assert.Equal(
                    "エアコンプレッサー",
                    x),
            x =>
                Assert.Equal(
                    "冷却水ポンプ",
                    x),
            x =>
                Assert.Equal(
                    "換気設備",
                    x),
            x =>
                Assert.Equal(
                    "集塵機",
                    x),
            x =>
                Assert.Equal(
                    "その他",
                    x));
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadTemplatesCommand_WhenSuccessful_LoadsAndSelectsFirstTemplate()
    {
        // Arrange
        var first =
            CreateDomainTemplate(
                name:
                    "コンプレッサー日常点検",
                equipmentType:
                    EquipmentType.AirCompressor,
                version:
                    2,
                isActive:
                    true,
                items:
                [
                    CreateDomainItem(
                        displayOrder:
                            2,
                        itemName:
                            "異音確認",
                        inputType:
                            InspectionInputType.NormalAbnormal),

                    CreateDomainItem(
                        displayOrder:
                            1,
                        itemName:
                            "吐出圧力",
                        inputType:
                            InspectionInputType.Numeric,
                        unit:
                            "MPa",
                        minimumValue:
                            0.5,
                        maximumValue:
                            1.0)
                ]);

        var second =
            CreateDomainTemplate(
                name:
                    "ポンプ点検",
                equipmentType:
                    EquipmentType.CoolingWaterPump);


        IReadOnlyList<InspectionTemplate>
            templates =
            [
                first,
                second
            ];


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult(
                            templates));


        // Act
        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsEmpty);

        Assert.Equal(
            2,
            sut.Templates.Count);

        Assert.NotNull(
            sut.SelectedTemplate);

        Assert.Equal(
            first.Id,
            sut.SelectedTemplate!.Id);

        Assert.Equal(
            "コンプレッサー日常点検",
            sut.Templates[0].Name);

        Assert.Equal(
            "エアコンプレッサー",
            sut.Templates[0]
                .EquipmentTypeName);

        Assert.Equal(
            2,
            sut.Templates[0].Version);

        Assert.True(
            sut.Templates[0].IsActive);

        /*
         * Domain側のItemsは2→1の順で追加したが、
         * ViewModelではDisplayOrder順になる。
         */
        Assert.Equal(
            2,
            sut.Templates[0]
                .Items.Count);

        Assert.Equal(
            1,
            sut.Templates[0]
                .Items[0]
                .DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            sut.Templates[0]
                .Items[0]
                .ItemName);

        Assert.Equal(
            "数値",
            sut.Templates[0]
                .Items[0]
                .InputTypeName);

        Assert.Equal(
            2,
            sut.Templates[0]
                .Items[1]
                .DisplayOrder);
    }


    [Fact]
    public async Task LoadTemplatesCommand_WhenLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromException<
                            IReadOnlyList<
                                InspectionTemplate>>(
                            new InvalidOperationException(
                                "読込テストエラー")));


        // Act
        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsLoading);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "点検票テンプレートを読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "読込テストエラー",
            sut.ErrorMessage);

        /*
         * エラー表示中なので
         * IsEmptyはfalse。
         */
        Assert.False(
            sut.IsEmpty);
    }


    [Fact]
    public async Task LoadTemplatesCommand_PreservesPreviousSelection()
    {
        // Arrange
        var first =
            CreateDomainTemplate(
                name:
                    "テンプレートA");

        var second =
            CreateDomainTemplate(
                name:
                    "テンプレートB");


        IReadOnlyList<InspectionTemplate>
            templates =
            [
                first,
                second
            ];


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult(
                            templates));


        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        sut.SelectedTemplate =
            sut.Templates[1];


        // Act
        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Assert
        Assert.NotNull(
            sut.SelectedTemplate);

        Assert.Equal(
            second.Id,
            sut.SelectedTemplate!.Id);
    }


    // ============================================
    // Domain -> List VM conversion
    // ============================================

    [Theory]
    [InlineData(
        EquipmentType.AirCompressor,
        "エアコンプレッサー")]
    [InlineData(
        EquipmentType.CoolingWaterPump,
        "冷却水ポンプ")]
    [InlineData(
        EquipmentType.Ventilation,
        "換気設備")]
    [InlineData(
        EquipmentType.DustCollector,
        "集塵機")]
    [InlineData(
        EquipmentType.Other,
        "その他")]
    public async Task LoadTemplatesCommand_ConvertsEquipmentTypeName(
        EquipmentType equipmentType,
        string expected)
    {
        // Arrange
        var template =
            CreateDomainTemplate(
                equipmentType:
                    equipmentType);


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]));


        // Act
        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            expected,
            sut.Templates[0]
                .EquipmentTypeName);
    }


    [Theory]
    [InlineData(
        InspectionInputType.NormalAbnormal,
        "正常・異常")]
    [InlineData(
        InspectionInputType.DoneNotDone,
        "実施・未実施")]
    [InlineData(
        InspectionInputType.Numeric,
        "数値")]
    [InlineData(
        InspectionInputType.Text,
        "文字入力")]
    public async Task LoadTemplatesCommand_ConvertsInputTypeName(
        InspectionInputType inputType,
        string expected)
    {
        // Arrange
        var template =
            CreateDomainTemplate(
                items:
                [
                    CreateDomainItem(
                        displayOrder:
                            1,
                        itemName:
                            "項目",
                        inputType:
                            inputType)
                ]);


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]));


        // Act
        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            expected,
            sut.Templates[0]
                .Items[0]
                .InputTypeName);
    }


    // ============================================
    // Create Dialog
    // ============================================

    [Fact]
    public void OpenCreateDialogCommand_InitializesCreateState()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.EditTemplateName =
            "古い名前";

        sut.SelectedEquipmentTypeName =
            "その他";

        sut.EditErrorMessage =
            "エラー";

        sut.OperationMessage =
            "メッセージ";


        // Act
        sut.OpenCreateDialogCommand
            .Execute(null);


        // Assert
        Assert.True(
            sut.IsCreateMode);

        Assert.True(
            sut.IsEditDialogOpen);

        Assert.Equal(
            string.Empty,
            sut.EditTemplateName);

        Assert.Equal(
            "エアコンプレッサー",
            sut.SelectedEquipmentTypeName);

        Assert.Null(
            sut.EditErrorMessage);

        Assert.Null(
            sut.OperationMessage);

        Assert.Equal(
            "点検票テンプレート新規作成",
            sut.DialogTitle);

        Assert.Equal(
            "作成",
            sut.SaveButtonText);

        Assert.True(
            sut.IsEquipmentTypeEditable);

        Assert.Single(
            sut.EditingItems);

        var item =
            sut.EditingItems[0];

        Assert.Equal(
            Guid.Empty,
            item.Id);

        Assert.Equal(
            1,
            item.DisplayOrder);

        Assert.Equal(
            string.Empty,
            item.ItemName);

        Assert.Equal(
            "正常・異常",
            item.InputTypeName);

        Assert.True(
            item.IsRequired);

        Assert.True(
            item.IsActive);
    }


    // ============================================
    // Edit Dialog
    // ============================================

    [Fact]
    public void OpenEditDialogCommand_WhenNoSelection_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.OpenEditDialogCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            "編集するテンプレートを選択してください。",
            sut.ErrorMessage);

        Assert.False(
            sut.IsEditDialogOpen);
    }


    [Fact]
    public void OpenEditDialogCommand_CopiesSelectedTemplateIntoEditor()
    {
        // Arrange
        var item2 =
            CreateRowItem(
                displayOrder:
                    2,
                itemName:
                    "項目2");

        var item1 =
            CreateRowItem(
                displayOrder:
                    1,
                itemName:
                    "項目1");


        var selected =
            new InspectionTemplateListItemViewModel(
                Guid.NewGuid(),
                "ポンプ日常点検",
                "冷却水ポンプ",
                3,
                true,
                [
                    item2,
                    item1
                ]);


        var sut =
            CreateViewModel();

        sut.SelectedTemplate =
            selected;


        // Act
        sut.OpenEditDialogCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsCreateMode);

        Assert.True(
            sut.IsEditDialogOpen);

        Assert.Equal(
            "ポンプ日常点検",
            sut.EditTemplateName);

        Assert.Equal(
            "冷却水ポンプ",
            sut.SelectedEquipmentTypeName);

        Assert.Equal(
            "点検票テンプレート編集",
            sut.DialogTitle);

        Assert.Equal(
            "保存",
            sut.SaveButtonText);

        Assert.False(
            sut.IsEquipmentTypeEditable);

        Assert.Equal(
            2,
            sut.EditingItems.Count);

        Assert.Equal(
            1,
            sut.EditingItems[0]
                .DisplayOrder);

        Assert.Equal(
            "項目1",
            sut.EditingItems[0]
                .ItemName);

        Assert.Equal(
            2,
            sut.EditingItems[1]
                .DisplayOrder);

        Assert.Equal(
            "項目2",
            sut.EditingItems[1]
                .ItemName);
    }


    // ============================================
    // Editing Item
    // ============================================

    [Fact]
    public void AddEditingItemCommand_AddsNextDisplayOrder()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.OpenCreateDialogCommand
            .Execute(null);


        // Act
        sut.AddEditingItemCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            sut.EditingItems.Count);

        Assert.Equal(
            1,
            sut.EditingItems[0]
                .DisplayOrder);

        Assert.Equal(
            2,
            sut.EditingItems[1]
                .DisplayOrder);

        Assert.Null(
            sut.EditErrorMessage);
    }


    [Fact]
    public void RemoveCommand_WhenOnlyOneItem_DoesNotRemove()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.OpenCreateDialogCommand
            .Execute(null);

        var item =
            sut.EditingItems[0];


        // Act
        item.RemoveCommand
            .Execute(null);


        // Assert
        Assert.Single(
            sut.EditingItems);

        Assert.Same(
            item,
            sut.EditingItems[0]);

        Assert.Equal(
            "点検項目は1件以上必要です。",
            sut.EditErrorMessage);
    }


    [Fact]
    public void RemoveCommand_RemovesItemAndRenumbersRemainingItems()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.OpenCreateDialogCommand
            .Execute(null);

        sut.AddEditingItemCommand
            .Execute(null);

        sut.AddEditingItemCommand
            .Execute(null);


        var first =
            sut.EditingItems[0];

        var second =
            sut.EditingItems[1];

        var third =
            sut.EditingItems[2];


        // Act
        second.RemoveCommand
            .Execute(null);


        // Assert
        Assert.Equal(
            2,
            sut.EditingItems.Count);

        Assert.Same(
            first,
            sut.EditingItems[0]);

        Assert.Same(
            third,
            sut.EditingItems[1]);

        Assert.Equal(
            1,
            sut.EditingItems[0]
                .DisplayOrder);

        Assert.Equal(
            2,
            sut.EditingItems[1]
                .DisplayOrder);

        Assert.Null(
            sut.EditErrorMessage);
    }


    // ============================================
    // Cancel
    // ============================================

    [Fact]
    public void CancelEditCommand_ClosesDialogAndClearsItems()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.OpenCreateDialogCommand
            .Execute(null);

        sut.EditErrorMessage =
            "テストエラー";


        // Act
        sut.CancelEditCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsEditDialogOpen);

        Assert.Null(
            sut.EditErrorMessage);

        Assert.Empty(
            sut.EditingItems);
    }


    // ============================================
    // Validation
    // ============================================

    [Fact]
    public async Task SaveEditCommand_CreateWithoutEquipmentType_SetsValidationError()
    {
        // Arrange
        var createCallCount =
            0;

        var sut =
            CreateViewModel(
                createTemplateAsync:
                    (_, _, _) =>
                    {
                        createCallCount++;

                        return Task.FromResult(
                            Guid.NewGuid());
                    });

        PrepareValidCreate(
            sut);

        sut.SelectedEquipmentTypeName =
            "   ";


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "設備種別を選択してください。",
            sut.EditErrorMessage);

        Assert.Equal(
            0,
            createCallCount);
    }


    [Fact]
    public async Task SaveEditCommand_WithoutTemplateName_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        sut.EditTemplateName =
            "   ";


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "テンプレート名を入力してください。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_WithoutItems_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        sut.EditingItems.Clear();


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "点検項目が1件もありません。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_WithEmptyItemName_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        sut.EditingItems[0]
            .ItemName =
            "   ";


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "表示順 1 の点検項目名を入力してください。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_WithDuplicateDisplayOrder_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        sut.EditingItems.Clear();

        sut.EditingItems.Add(
            CreateEditorItem(
                displayOrder:
                    1,
                itemName:
                    "項目1"));

        sut.EditingItems.Add(
            CreateEditorItem(
                displayOrder:
                    1,
                itemName:
                    "項目2"));


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "表示順 1 が重複しています。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_WhenMinimumExceedsMaximum_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        var item =
            sut.EditingItems[0];

        item.InputTypeName =
            "数値";

        item.MinimumValue =
            10;

        item.MaximumValue =
            5;


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "「点検項目」の基準下限が基準上限を超えています。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_WhenNonNumericItemHasRange_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();

        PrepareValidCreate(
            sut);

        var item =
            sut.EditingItems[0];

        item.InputTypeName =
            "正常・異常";

        item.MinimumValue =
            1;


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "「点検項目」の基準値は、入力方式が数値の場合のみ設定できます。",
            sut.EditErrorMessage);
    }


    [Fact]
    public async Task SaveEditCommand_EditModeWithoutSelectedTemplate_SetsError()
    {
        // Arrange
        var updateCallCount =
            0;

        var sut =
            CreateViewModel(
                updateTemplateAsync:
                    (_, _, _) =>
                    {
                        updateCallCount++;

                        return Task.CompletedTask;
                    });

        sut.IsCreateMode =
            false;


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "編集対象のテンプレートが見つかりません。",
            sut.EditErrorMessage);

        Assert.Equal(
            0,
            updateCallCount);
    }


    // ============================================
    // Create
    // ============================================

    [Fact]
    public async Task SaveEditCommand_Create_PassesNormalizedDataAndReloads()
    {
        // Arrange
        EquipmentType?
            capturedEquipmentType =
                null;

        string?
            capturedName =
                null;

        IReadOnlyList<
            InspectionTemplateItemCreateData>?
            capturedItems =
                null;


        var createdTemplate =
            CreateDomainTemplate(
                name:
                    "ポンプ点検",
                equipmentType:
                    EquipmentType.CoolingWaterPump);


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            createdTemplate
                        ]),

                createTemplateAsync:
                    (
                        equipmentType,
                        name,
                        items) =>
                    {
                        capturedEquipmentType =
                            equipmentType;

                        capturedName =
                            name;

                        capturedItems =
                            items;

                        return Task.FromResult(
                            createdTemplate.Id);
                    });


        sut.OpenCreateDialogCommand
            .Execute(null);

        sut.SelectedEquipmentTypeName =
            "冷却水ポンプ";

        sut.EditTemplateName =
            "  ポンプ点検  ";


        sut.EditingItems.Clear();

        /*
         * Collectionの順番は2→1。
         * 保存時にはDisplayOrder順の1→2になることを確認する。
         */
        sut.EditingItems.Add(
            CreateEditorItem(
                displayOrder:
                    2,
                itemName:
                    "  備考  ",
                inputType:
                    InspectionInputType.Text,
                unit:
                    "   ",
                isRequired:
                    false));

        sut.EditingItems.Add(
            CreateEditorItem(
                displayOrder:
                    1,
                itemName:
                    "  吐出圧力  ",
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "  MPa  ",
                minimumValue:
                    0.5,
                maximumValue:
                    1.0,
                isRequired:
                    true));


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            EquipmentType.CoolingWaterPump,
            capturedEquipmentType);

        Assert.Equal(
            "ポンプ点検",
            capturedName);

        Assert.NotNull(
            capturedItems);

        Assert.Equal(
            2,
            capturedItems!.Count);


        var first =
            capturedItems[0];

        Assert.Equal(
            1,
            first.DisplayOrder);

        Assert.Equal(
            "吐出圧力",
            first.ItemName);

        Assert.Equal(
            InspectionInputType.Numeric,
            first.InputType);

        Assert.Equal(
            "MPa",
            first.Unit);

        Assert.Equal(
            0.5,
            first.MinimumValue);

        Assert.Equal(
            1.0,
            first.MaximumValue);

        Assert.True(
            first.IsRequired);


        var second =
            capturedItems[1];

        Assert.Equal(
            2,
            second.DisplayOrder);

        Assert.Equal(
            "備考",
            second.ItemName);

        Assert.Equal(
            InspectionInputType.Text,
            second.InputType);

        Assert.Null(
            second.Unit);

        Assert.False(
            second.IsRequired);


        Assert.False(
            sut.IsSaving);

        Assert.False(
            sut.IsEditDialogOpen);

        Assert.Empty(
            sut.EditingItems);

        Assert.Equal(
            "点検票テンプレートを作成しました。",
            sut.OperationMessage);

        Assert.True(
            sut.HasOperationMessage);

        Assert.NotNull(
            sut.SelectedTemplate);

        Assert.Equal(
            createdTemplate.Id,
            sut.SelectedTemplate!.Id);
    }


    [Theory]
    [InlineData(
        "エアコンプレッサー",
        EquipmentType.AirCompressor)]
    [InlineData(
        "冷却水ポンプ",
        EquipmentType.CoolingWaterPump)]
    [InlineData(
        "換気設備",
        EquipmentType.Ventilation)]
    [InlineData(
        "集塵機",
        EquipmentType.DustCollector)]
    [InlineData(
        "その他",
        EquipmentType.Other)]
    public async Task SaveEditCommand_Create_ConvertsEquipmentType(
        string equipmentTypeName,
        EquipmentType expected)
    {
        // Arrange
        EquipmentType?
            captured =
                null;

        var sut =
            CreateViewModel(
                createTemplateAsync:
                    (
                        equipmentType,
                        _,
                        _) =>
                    {
                        captured =
                            equipmentType;

                        return Task.FromResult(
                            Guid.NewGuid());
                    });


        PrepareValidCreate(
            sut);

        sut.SelectedEquipmentTypeName =
            equipmentTypeName;


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            expected,
            captured);
    }


    [Fact]
    public async Task SaveEditCommand_WhenCreateFails_SetsEditErrorAndKeepsDialogOpen()
    {
        // Arrange
        var sut =
            CreateViewModel(
                createTemplateAsync:
                    (_, _, _) =>
                        Task.FromException<Guid>(
                            new InvalidOperationException(
                                "作成テストエラー")));


        PrepareValidCreate(
            sut);


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.IsEditDialogOpen);

        Assert.Single(
            sut.EditingItems);

        Assert.True(
            sut.HasEditError);

        Assert.NotNull(
            sut.EditErrorMessage);

        Assert.Contains(
            "点検票テンプレートを作成できませんでした。",
            sut.EditErrorMessage);

        Assert.Contains(
            "作成テストエラー",
            sut.EditErrorMessage);

        Assert.Null(
            sut.OperationMessage);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public async Task SaveEditCommand_Update_PassesNormalizedDataAndReloads()
    {
        // Arrange
        var domainItem =
            CreateDomainItem(
                displayOrder:
                    1,
                itemName:
                    "吐出圧力",
                inputType:
                    InspectionInputType.Numeric,
                unit:
                    "MPa",
                minimumValue:
                    0.5,
                maximumValue:
                    1.0);


        var template =
            CreateDomainTemplate(
                name:
                    "旧テンプレート",
                equipmentType:
                    EquipmentType.AirCompressor,
                items:
                [
                    domainItem
                ]);


        Guid?
            capturedTemplateId =
                null;

        string?
            capturedName =
                null;

        IReadOnlyList<
            InspectionTemplateItemUpdateData>?
            capturedItems =
                null;


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]),

                updateTemplateAsync:
                    (
                        templateId,
                        name,
                        items) =>
                    {
                        capturedTemplateId =
                            templateId;

                        capturedName =
                            name;

                        capturedItems =
                            items;

                        return Task.CompletedTask;
                    });


        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);

        sut.OpenEditDialogCommand
            .Execute(null);


        sut.EditTemplateName =
            "  新テンプレート名  ";

        sut.EditingItems[0]
            .ItemName =
            "  更新後項目  ";

        sut.EditingItems[0]
            .Unit =
            "   ";

        sut.EditingItems[0]
            .MinimumValue =
            1;

        sut.EditingItems[0]
            .MaximumValue =
            5;

        sut.EditingItems[0]
            .IsRequired =
            false;

        sut.EditingItems[0]
            .IsActive =
            false;


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            template.Id,
            capturedTemplateId);

        Assert.Equal(
            "新テンプレート名",
            capturedName);

        Assert.NotNull(
            capturedItems);

        Assert.Single(
            capturedItems!);


        var item =
            capturedItems![0];

        Assert.Equal(
            domainItem.Id,
            item.Id);

        Assert.Equal(
            1,
            item.DisplayOrder);

        Assert.Equal(
            "更新後項目",
            item.ItemName);

        Assert.Equal(
            InspectionInputType.Numeric,
            item.InputType);

        Assert.Null(
            item.Unit);

        Assert.Equal(
            1,
            item.MinimumValue);

        Assert.Equal(
            5,
            item.MaximumValue);

        Assert.False(
            item.IsRequired);

        Assert.False(
            item.IsActive);


        Assert.False(
            sut.IsSaving);

        Assert.False(
            sut.IsEditDialogOpen);

        Assert.Empty(
            sut.EditingItems);

        Assert.Equal(
            "点検票テンプレートを更新しました。",
            sut.OperationMessage);

        Assert.NotNull(
            sut.SelectedTemplate);

        Assert.Equal(
            template.Id,
            sut.SelectedTemplate!.Id);
    }


    [Fact]
    public async Task SaveEditCommand_WhenUpdateFails_SetsEditErrorAndKeepsDialogOpen()
    {
        // Arrange
        var template =
            CreateDomainTemplate(
                name:
                    "テンプレート",
                items:
                [
                    CreateDomainItem(
                        displayOrder:
                            1,
                        itemName:
                            "点検項目")
                ]);


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]),

                updateTemplateAsync:
                    (_, _, _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "更新テストエラー")));


        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);

        sut.OpenEditDialogCommand
            .Execute(null);


        // Act
        await sut.SaveEditCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.IsEditDialogOpen);

        Assert.Single(
            sut.EditingItems);

        Assert.True(
            sut.HasEditError);

        Assert.NotNull(
            sut.EditErrorMessage);

        Assert.Contains(
            "点検票テンプレートを更新できませんでした。",
            sut.EditErrorMessage);

        Assert.Contains(
            "更新テストエラー",
            sut.EditErrorMessage);
    }


    // ============================================
    // Toggle Active
    // ============================================

    [Theory]
    [InlineData(
        true,
        false,
        "点検票テンプレートを無効化しました。")]
    [InlineData(
        false,
        true,
        "点検票テンプレートを有効化しました。")]
    public async Task ToggleActiveCommand_PassesOppositeStateAndReloads(
        bool currentState,
        bool expectedNewState,
        string expectedMessage)
    {
        // Arrange
        var template =
            CreateDomainTemplate(
                isActive:
                    currentState);


        Guid?
            capturedId =
                null;

        bool?
            capturedActive =
                null;


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]),

                setActiveAsync:
                    (
                        templateId,
                        isActive) =>
                    {
                        capturedId =
                            templateId;

                        capturedActive =
                            isActive;

                        return Task.CompletedTask;
                    });


        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Act
        await sut.ToggleActiveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            template.Id,
            capturedId);

        Assert.Equal(
            expectedNewState,
            capturedActive);

        Assert.Equal(
            expectedMessage,
            sut.OperationMessage);

        Assert.True(
            sut.HasOperationMessage);

        Assert.False(
            sut.IsSaving);

        Assert.NotNull(
            sut.SelectedTemplate);

        Assert.Equal(
            template.Id,
            sut.SelectedTemplate!.Id);
    }


    [Fact]
    public async Task ToggleActiveCommand_WhenOperationFails_SetsError()
    {
        // Arrange
        var template =
            CreateDomainTemplate();


        var sut =
            CreateViewModel(
                loadTemplatesAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<
                                InspectionTemplate>>(
                        [
                            template
                        ]),

                setActiveAsync:
                    (_, _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "状態変更テストエラー")));


        await sut.LoadTemplatesCommand
            .ExecuteAsync(null);


        // Act
        await sut.ToggleActiveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.False(
            sut.IsSaving);

        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "テンプレートの状態を変更できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "状態変更テストエラー",
            sut.ErrorMessage);

        Assert.Null(
            sut.OperationMessage);
    }


    // ============================================
    // Helpers
    // ============================================

    private static InspectionTemplateManagementViewModel
        CreateViewModel(
            Func<
                Task<
                    IReadOnlyList<
                        InspectionTemplate>>>?
                loadTemplatesAsync = null,

            Func<
                EquipmentType,
                string,
                IReadOnlyList<
                    InspectionTemplateItemCreateData>,
                Task<Guid>>?
                createTemplateAsync = null,

            Func<
                Guid,
                string,
                IReadOnlyList<
                    InspectionTemplateItemUpdateData>,
                Task>?
                updateTemplateAsync = null,

            Func<
                Guid,
                bool,
                Task>?
                setActiveAsync = null)
    {
        return new InspectionTemplateManagementViewModel(
            loadTemplatesAsync ??
            (() =>
                Task.FromResult<
                    IReadOnlyList<
                        InspectionTemplate>>(
                    Array.Empty<
                        InspectionTemplate>())),

            createTemplateAsync ??
            ((_, _, _) =>
                Task.FromResult(
                    Guid.NewGuid())),

            updateTemplateAsync ??
            ((_, _, _) =>
                Task.CompletedTask),

            setActiveAsync ??
            ((_, _) =>
                Task.CompletedTask));
    }


    private static void PrepareValidCreate(
        InspectionTemplateManagementViewModel sut)
    {
        sut.OpenCreateDialogCommand
            .Execute(null);

        sut.EditTemplateName =
            "テストテンプレート";

        sut.SelectedEquipmentTypeName =
            "エアコンプレッサー";

        sut.EditingItems[0]
            .ItemName =
            "点検項目";
    }


    private static InspectionTemplate
        CreateDomainTemplate(
            string name =
                "テストテンプレート",
            EquipmentType equipmentType =
                EquipmentType.AirCompressor,
            int version = 1,
            bool isActive = true,
            IReadOnlyList<
                InspectionTemplateItem>?
                items = null)
    {
        var template =
            new InspectionTemplate
            {
                Name =
                    name,

                EquipmentType =
                    equipmentType,

                Version =
                    version,

                IsActive =
                    isActive,

                CreatedAt =
                    new DateTime(
                        2026,
                        8,
                        19,
                        0,
                        0,
                        0,
                        DateTimeKind.Utc)
            };


        if (items is not null)
        {
            foreach (var item
                     in items)
            {
                item.InspectionTemplateId =
                    template.Id;

                item.InspectionTemplate =
                    template;

                template.Items.Add(
                    item);
            }
        }


        return template;
    }


    private static InspectionTemplateItem
        CreateDomainItem(
            int displayOrder,
            string itemName,
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = true,
            bool isActive = true)
    {
        return new InspectionTemplateItem
        {
            DisplayOrder =
                displayOrder,

            ItemName =
                itemName,

            InputType =
                inputType,

            Unit =
                unit,

            MinimumValue =
                minimumValue,

            MaximumValue =
                maximumValue,

            IsRequired =
                isRequired,

            IsActive =
                isActive
        };
    }


    private static InspectionTemplateItemEditorViewModel
        CreateEditorItem(
            int displayOrder,
            string itemName,
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal,
            string? unit = null,
            double? minimumValue = null,
            double? maximumValue = null,
            bool isRequired = true,
            bool isActive = true)
    {
        return new InspectionTemplateItemEditorViewModel(
            id:
                Guid.NewGuid(),

            displayOrder:
                displayOrder,

            itemName:
                itemName,

            inputType:
                inputType,

            unit:
                unit,

            minimumValue:
                minimumValue,

            maximumValue:
                maximumValue,

            isRequired:
                isRequired,

            isActive:
                isActive);
    }


    private static InspectionTemplateItemRowViewModel
        CreateRowItem(
            int displayOrder,
            string itemName,
            InspectionInputType inputType =
                InspectionInputType.NormalAbnormal)
    {
        var inputTypeName =
            inputType switch
            {
                InspectionInputType.NormalAbnormal =>
                    "正常・異常",

                InspectionInputType.DoneNotDone =>
                    "実施・未実施",

                InspectionInputType.Numeric =>
                    "数値",

                InspectionInputType.Text =>
                    "文字入力",

                _ =>
                    inputType.ToString()
            };


        return new InspectionTemplateItemRowViewModel(
            id:
                Guid.NewGuid(),

            displayOrder:
                displayOrder,

            itemName:
                itemName,

            inputType:
                inputType,

            inputTypeName:
                inputTypeName,

            unit:
                null,

            minimumValue:
                null,

            maximumValue:
                null,

            isRequired:
                true,

            isActive:
                true);
    }
}