using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionTemplateManagementViewModel
    : ViewModelBase
{
    // ============================================
    // Dependencies
    // ============================================

    private readonly Func<
        Task<IReadOnlyList<InspectionTemplate>>>
        _loadTemplatesAsync;

    private readonly Func<
        EquipmentType,
        string,
        IReadOnlyList<InspectionTemplateItemCreateData>,
        Task<Guid>>
        _createTemplateAsync;

    private readonly Func<
        Guid,
        string,
        IReadOnlyList<InspectionTemplateItemUpdateData>,
        Task>
        _updateTemplateAsync;

    private readonly Func<
        Guid,
        bool,
        Task>
        _setActiveAsync;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public InspectionTemplateManagementViewModel(
        InspectionTemplateRepository
            inspectionTemplateRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionTemplateRepository);

        /*
         * Repository側はoptional CancellationTokenを持つため
         * method groupではなくlambdaでラップする。
         */
        _loadTemplatesAsync =
            () =>
                inspectionTemplateRepository
                    .GetAllAsync();

        _createTemplateAsync =
            (equipmentType, name, items) =>
                inspectionTemplateRepository
                    .CreateAsync(
                        equipmentType,
                        name,
                        items);

        _updateTemplateAsync =
            (templateId, name, items) =>
                inspectionTemplateRepository
                    .UpdateAsync(
                        templateId,
                        name,
                        items);

        _setActiveAsync =
            (templateId, isActive) =>
                inspectionTemplateRepository
                    .SetActiveAsync(
                        templateId,
                        isActive);

        /*
         * 本番では従来どおり
         * コンストラクタ生成後に自動ロード。
         */
        _ = LoadTemplatesAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal InspectionTemplateManagementViewModel(
        Func<Task<IReadOnlyList<InspectionTemplate>>>
            loadTemplatesAsync,
        Func<
            EquipmentType,
            string,
            IReadOnlyList<InspectionTemplateItemCreateData>,
            Task<Guid>>
            createTemplateAsync,
        Func<
            Guid,
            string,
            IReadOnlyList<InspectionTemplateItemUpdateData>,
            Task>
            updateTemplateAsync,
        Func<Guid, bool, Task>
            setActiveAsync)
    {
        ArgumentNullException.ThrowIfNull(
            loadTemplatesAsync);

        ArgumentNullException.ThrowIfNull(
            createTemplateAsync);

        ArgumentNullException.ThrowIfNull(
            updateTemplateAsync);

        ArgumentNullException.ThrowIfNull(
            setActiveAsync);

        _loadTemplatesAsync =
            loadTemplatesAsync;

        _createTemplateAsync =
            createTemplateAsync;

        _updateTemplateAsync =
            updateTemplateAsync;

        _setActiveAsync =
            setActiveAsync;

        /*
         * テスト用では自動ロードしない。
         * LoadTemplatesCommandを明示的に実行する。
         */
    }


    // ============================================
    // Collections
    // ============================================

    public ObservableCollection<
        InspectionTemplateListItemViewModel>
        Templates
    { get; } = [];


    public ObservableCollection<
        InspectionTemplateItemEditorViewModel>
        EditingItems
    { get; } = [];


    public IReadOnlyList<string>
        EquipmentTypeChoices
    { get; } =
    [
        "エアコンプレッサー",
        "冷却水ポンプ",
        "換気設備",
        "集塵機",
        "その他"
    ];


    // ============================================
    // Selected Template
    // ============================================

    [ObservableProperty]
    private InspectionTemplateListItemViewModel?
        selectedTemplate;


    // ============================================
    // Loading
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEmpty))]
    private bool
        isLoading;


    // ============================================
    // Error
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string?
        errorMessage;


    // ============================================
    // Edit Dialog
    // ============================================

    [ObservableProperty]
    private bool
        isEditDialogOpen;


    [ObservableProperty]
    private string
        editTemplateName =
            string.Empty;


    [ObservableProperty]
    private string
        selectedEquipmentTypeName =
            "エアコンプレッサー";


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(DialogTitle))]
    [NotifyPropertyChangedFor(
        nameof(DialogDescription))]
    [NotifyPropertyChangedFor(
        nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(
        nameof(IsEquipmentTypeEditable))]
    private bool
        isCreateMode;


    [ObservableProperty]
    private bool
        isSaving;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasEditError))]
    private string?
        editErrorMessage;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasOperationMessage))]
    private string?
        operationMessage;


    // ============================================
    // Calculated Properties
    // ============================================

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    public bool HasEditError =>
        !string.IsNullOrWhiteSpace(
            EditErrorMessage);


    public bool HasOperationMessage =>
        !string.IsNullOrWhiteSpace(
            OperationMessage);


    public bool IsEmpty =>
        !IsLoading &&
        Templates.Count == 0 &&
        !HasError;


    public string DialogTitle =>
        IsCreateMode
            ? "点検票テンプレート新規作成"
            : "点検票テンプレート編集";


    public string DialogDescription =>
        IsCreateMode
            ? "設備種別と点検項目を入力して、新しい点検票を作成します。"
            : "テンプレート名と各点検項目を編集します。";


    public string SaveButtonText =>
        IsCreateMode
            ? "作成"
            : "保存";


    public bool IsEquipmentTypeEditable =>
        IsCreateMode;


    // ============================================
    // Load
    // ============================================

    [RelayCommand]
    private async Task LoadTemplatesAsync()
    {
        if (IsLoading)
        {
            return;
        }

        var previouslySelectedId =
            SelectedTemplate?.Id;

        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;

            var templates =
                await _loadTemplatesAsync();

            Templates.Clear();

            foreach (var template
                     in templates)
            {
                Templates.Add(
                    CreateListItemViewModel(
                        template));
            }

            SelectedTemplate =
                previouslySelectedId.HasValue
                    ? Templates.FirstOrDefault(
                        x =>
                            x.Id ==
                            previouslySelectedId.Value)
                    : null;

            SelectedTemplate ??=
                Templates.FirstOrDefault();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "点検票テンプレートを読み込めませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsLoading =
                false;

            OnPropertyChanged(
                nameof(IsEmpty));
        }
    }


    // ============================================
    // Open Create Dialog
    // ============================================

    [RelayCommand]
    private void OpenCreateDialog()
    {
        if (IsSaving)
        {
            return;
        }

        IsCreateMode =
            true;

        EditTemplateName =
            string.Empty;

        SelectedEquipmentTypeName =
            EquipmentTypeChoices[0];

        EditErrorMessage =
            null;

        OperationMessage =
            null;

        EditingItems.Clear();

        AddBlankEditingItem();

        IsEditDialogOpen =
            true;
    }


    // ============================================
    // Open Edit Dialog
    // ============================================

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedTemplate is null)
        {
            ErrorMessage =
                "編集するテンプレートを選択してください。";

            return;
        }

        IsCreateMode =
            false;

        EditTemplateName =
            SelectedTemplate.Name;

        SelectedEquipmentTypeName =
            SelectedTemplate.EquipmentTypeName;

        EditErrorMessage =
            null;

        OperationMessage =
            null;

        EditingItems.Clear();

        foreach (var item in
                 SelectedTemplate.Items
                     .OrderBy(
                         x =>
                             x.DisplayOrder))
        {
            EditingItems.Add(
                new InspectionTemplateItemEditorViewModel(
                    item.Id,
                    item.DisplayOrder,
                    item.ItemName,
                    item.InputType,
                    item.Unit,
                    item.MinimumValue,
                    item.MaximumValue,
                    item.IsRequired,
                    item.IsActive,
                    RemoveEditingItem));
        }

        IsEditDialogOpen =
            true;
    }


    // ============================================
    // Add Editing Item
    // ============================================

    [RelayCommand]
    private void AddEditingItem()
    {
        AddBlankEditingItem();

        EditErrorMessage =
            null;
    }


    // ============================================
    // Cancel Edit
    // ============================================

    [RelayCommand]
    private void CancelEdit()
    {
        if (IsSaving)
        {
            return;
        }

        IsEditDialogOpen =
            false;

        EditErrorMessage =
            null;

        EditingItems.Clear();
    }


    // ============================================
    // Save
    // ============================================

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (IsSaving)
        {
            return;
        }

        if (!IsCreateMode &&
            SelectedTemplate is null)
        {
            EditErrorMessage =
                "編集対象のテンプレートが見つかりません。";

            return;
        }

        if (!ValidateEditInput())
        {
            return;
        }

        Guid? createdTemplateId =
            null;

        try
        {
            IsSaving =
                true;

            EditErrorMessage =
                null;

            OperationMessage =
                null;

            // ====================================
            // Create
            // ====================================

            if (IsCreateMode)
            {
                var createItems =
                    EditingItems
                        .OrderBy(
                            x =>
                                x.DisplayOrder)
                        .Select(
                            x =>
                                new InspectionTemplateItemCreateData(
                                    x.DisplayOrder,
                                    x.ItemName.Trim(),
                                    x.GetInputType(),
                                    NormalizeText(
                                        x.Unit),
                                    x.MinimumValue,
                                    x.MaximumValue,
                                    x.IsRequired,
                                    x.IsActive))
                        .ToList();

                createdTemplateId =
                    await _createTemplateAsync(
                        GetSelectedEquipmentType(),
                        EditTemplateName.Trim(),
                        createItems);

                OperationMessage =
                    "点検票テンプレートを作成しました。";
            }

            // ====================================
            // Update
            // ====================================

            else
            {
                var updateItems =
                    EditingItems
                        .OrderBy(
                            x =>
                                x.DisplayOrder)
                        .Select(
                            x =>
                                new InspectionTemplateItemUpdateData(
                                    x.Id,
                                    x.DisplayOrder,
                                    x.ItemName.Trim(),
                                    x.GetInputType(),
                                    NormalizeText(
                                        x.Unit),
                                    x.MinimumValue,
                                    x.MaximumValue,
                                    x.IsRequired,
                                    x.IsActive))
                        .ToList();

                await _updateTemplateAsync(
                    SelectedTemplate!.Id,
                    EditTemplateName.Trim(),
                    updateItems);

                OperationMessage =
                    "点検票テンプレートを更新しました。";
            }

            IsEditDialogOpen =
                false;

            EditingItems.Clear();

            await LoadTemplatesAsync();

            /*
             * 新規作成時はReload後に
             * 作成したテンプレートを選択する。
             */
            if (createdTemplateId.HasValue)
            {
                SelectedTemplate =
                    Templates.FirstOrDefault(
                        x =>
                            x.Id ==
                            createdTemplateId.Value);
            }
        }
        catch (Exception exception)
        {
            EditErrorMessage =
                IsCreateMode
                    ? "点検票テンプレートを作成できませんでした。" +
                      Environment.NewLine +
                      exception.Message
                    : "点検票テンプレートを更新できませんでした。" +
                      Environment.NewLine +
                      exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Toggle Active
    // ============================================

    [RelayCommand]
    private async Task ToggleActiveAsync()
    {
        if (SelectedTemplate is null ||
            IsSaving)
        {
            return;
        }

        var templateId =
            SelectedTemplate.Id;

        var newActiveState =
            !SelectedTemplate.IsActive;

        try
        {
            IsSaving =
                true;

            ErrorMessage =
                null;

            OperationMessage =
                null;

            await _setActiveAsync(
                templateId,
                newActiveState);

            OperationMessage =
                newActiveState
                    ? "点検票テンプレートを有効化しました。"
                    : "点検票テンプレートを無効化しました。";

            await LoadTemplatesAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "テンプレートの状態を変更できませんでした。" +
                Environment.NewLine +
                exception.Message;
        }
        finally
        {
            IsSaving =
                false;
        }
    }


    // ============================================
    // Add Blank Item
    // ============================================

    private void AddBlankEditingItem()
    {
        EditingItems.Add(
            new InspectionTemplateItemEditorViewModel(
                Guid.Empty,
                EditingItems.Count + 1,
                string.Empty,
                InspectionInputType.NormalAbnormal,
                null,
                null,
                null,
                true,
                true,
                RemoveEditingItem));
    }


    // ============================================
    // Remove Item
    // ============================================

    private void RemoveEditingItem(
        InspectionTemplateItemEditorViewModel item)
    {
        if (EditingItems.Count <= 1)
        {
            EditErrorMessage =
                "点検項目は1件以上必要です。";

            return;
        }

        EditingItems.Remove(
            item);

        RenumberEditingItems();

        EditErrorMessage =
            null;
    }


    // ============================================
    // Renumber
    // ============================================

    private void RenumberEditingItems()
    {
        for (var index = 0;
             index < EditingItems.Count;
             index++)
        {
            EditingItems[index]
                .SetDisplayOrder(
                    index + 1);
        }
    }


    // ============================================
    // Validation
    // ============================================

    private bool ValidateEditInput()
    {
        if (IsCreateMode &&
            string.IsNullOrWhiteSpace(
                SelectedEquipmentTypeName))
        {
            EditErrorMessage =
                "設備種別を選択してください。";

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                EditTemplateName))
        {
            EditErrorMessage =
                "テンプレート名を入力してください。";

            return false;
        }

        if (EditingItems.Count == 0)
        {
            EditErrorMessage =
                "点検項目が1件もありません。";

            return false;
        }

        var emptyItem =
            EditingItems.FirstOrDefault(
                x =>
                    string.IsNullOrWhiteSpace(
                        x.ItemName));

        if (emptyItem is not null)
        {
            EditErrorMessage =
                $"表示順 {emptyItem.DisplayOrder} の" +
                "点検項目名を入力してください。";

            return false;
        }

        var invalidOrder =
            EditingItems
                .GroupBy(
                    x =>
                        x.DisplayOrder)
                .FirstOrDefault(
                    x =>
                        x.Count() > 1);

        if (invalidOrder is not null)
        {
            EditErrorMessage =
                $"表示順 {invalidOrder.Key} が重複しています。";

            return false;
        }

        var invalidRangeItem =
            EditingItems.FirstOrDefault(
                x =>
                    x.MinimumValue.HasValue &&
                    x.MaximumValue.HasValue &&
                    x.MinimumValue.Value >
                    x.MaximumValue.Value);

        if (invalidRangeItem is not null)
        {
            EditErrorMessage =
                $"「{invalidRangeItem.ItemName}」の" +
                "基準下限が基準上限を超えています。";

            return false;
        }

        var invalidNumericItem =
            EditingItems.FirstOrDefault(
                x =>
                    x.GetInputType() !=
                    InspectionInputType.Numeric &&
                    (x.MinimumValue.HasValue ||
                     x.MaximumValue.HasValue));

        if (invalidNumericItem is not null)
        {
            EditErrorMessage =
                $"「{invalidNumericItem.ItemName}」の" +
                "基準値は、入力方式が数値の場合のみ設定できます。";

            return false;
        }

        return true;
    }


    // ============================================
    // Equipment Type Conversion
    // ============================================

    private EquipmentType GetSelectedEquipmentType()
    {
        return SelectedEquipmentTypeName switch
        {
            "エアコンプレッサー" =>
                EquipmentType.AirCompressor,

            "冷却水ポンプ" =>
                EquipmentType.CoolingWaterPump,

            "換気設備" =>
                EquipmentType.Ventilation,

            "集塵機" =>
                EquipmentType.DustCollector,

            "その他" =>
                EquipmentType.Other,

            _ =>
                throw new InvalidOperationException(
                    "未対応の設備種別です: " +
                    SelectedEquipmentTypeName)
        };
    }


    // ============================================
    // Normalize
    // ============================================

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(
                value)
            ? null
            : value.Trim();
    }


    // ============================================
    // List Item Conversion
    // ============================================

    private static InspectionTemplateListItemViewModel
        CreateListItemViewModel(
            InspectionTemplate template)
    {
        var items =
            template.Items
                .OrderBy(
                    x =>
                        x.DisplayOrder)
                .Select(
                    item =>
                        new InspectionTemplateItemRowViewModel(
                            item.Id,
                            item.DisplayOrder,
                            item.ItemName,
                            item.InputType,
                            GetInputTypeName(
                                item.InputType),
                            item.Unit,
                            item.MinimumValue,
                            item.MaximumValue,
                            item.IsRequired,
                            item.IsActive))
                .ToList();

        return new InspectionTemplateListItemViewModel(
            template.Id,
            template.Name,
            GetEquipmentTypeName(
                template.EquipmentType),
            template.Version,
            template.IsActive,
            items);
    }


    // ============================================
    // Equipment Type Display
    // ============================================

    private static string GetEquipmentTypeName(
        EquipmentType equipmentType)
    {
        return equipmentType switch
        {
            EquipmentType.AirCompressor =>
                "エアコンプレッサー",

            EquipmentType.CoolingWaterPump =>
                "冷却水ポンプ",

            EquipmentType.Ventilation =>
                "換気設備",

            EquipmentType.DustCollector =>
                "集塵機",

            EquipmentType.Other =>
                "その他",

            _ =>
                equipmentType.ToString()
        };
    }


    // ============================================
    // Input Type Display
    // ============================================

    private static string GetInputTypeName(
        InspectionInputType inputType)
    {
        return inputType switch
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
    }
}