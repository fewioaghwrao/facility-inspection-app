using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Equipments;
using FacilityInspection.Domain.InspectionTemplates;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class InspectionTemplateManagementViewModel
    : ViewModelBase
{
    private readonly InspectionTemplateRepository
        _inspectionTemplateRepository;

    public InspectionTemplateManagementViewModel(
        InspectionTemplateRepository inspectionTemplateRepository)
    {
        ArgumentNullException.ThrowIfNull(
            inspectionTemplateRepository);

        _inspectionTemplateRepository =
            inspectionTemplateRepository;

        // 初期表示時にSQLiteから読み込む
        _ = LoadTemplatesAsync();
    }

    public ObservableCollection<InspectionTemplateListItemViewModel>
        Templates
    { get; } = [];

    public ObservableCollection<InspectionTemplateItemEditorViewModel>
        EditingItems
    { get; } = [];

    [ObservableProperty]
    private InspectionTemplateListItemViewModel?
        selectedTemplate;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    private bool isEditDialogOpen;

    [ObservableProperty]
    private string editTemplateName = string.Empty;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditError))]
    private string? editErrorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOperationMessage))]
    private string? operationMessage;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasEditError =>
        !string.IsNullOrWhiteSpace(EditErrorMessage);

    public bool HasOperationMessage =>
        !string.IsNullOrWhiteSpace(OperationMessage);

    public bool IsEmpty =>
        !IsLoading &&
        Templates.Count == 0 &&
        !HasError;

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
            IsLoading = true;
            ErrorMessage = null;

            var templates =
                await _inspectionTemplateRepository
                    .GetAllAsync();

            Templates.Clear();

            foreach (var template in templates)
            {
                Templates.Add(
                    CreateListItemViewModel(template));
            }

            SelectedTemplate =
                previouslySelectedId.HasValue
                    ? Templates.FirstOrDefault(
                        x => x.Id == previouslySelectedId.Value)
                    : null;

            SelectedTemplate ??=
                Templates.FirstOrDefault();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"点検票テンプレートを読み込めませんでした。" +
                $"{Environment.NewLine}{exception.Message}";
        }
        finally
        {
            IsLoading = false;

            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private void OpenEditDialog()
    {
        if (SelectedTemplate is null)
        {
            ErrorMessage =
                "編集するテンプレートを選択してください。";

            return;
        }

        EditTemplateName =
            SelectedTemplate.Name;

        EditErrorMessage = null;
        OperationMessage = null;

        EditingItems.Clear();

        foreach (var item in
                 SelectedTemplate.Items
                     .OrderBy(x => x.DisplayOrder))
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
                    item.IsActive));
        }

        IsEditDialogOpen = true;
    }

    [RelayCommand]
    private void CancelEdit()
    {
        if (IsSaving)
        {
            return;
        }

        IsEditDialogOpen = false;
        EditErrorMessage = null;
        EditingItems.Clear();
    }

    [RelayCommand]
    private async Task SaveEditAsync()
    {
        if (SelectedTemplate is null ||
            IsSaving)
        {
            return;
        }

        if (!ValidateEditInput())
        {
            return;
        }

        try
        {
            IsSaving = true;
            EditErrorMessage = null;
            OperationMessage = null;

            var updateItems =
                EditingItems
                    .OrderBy(x => x.DisplayOrder)
                    .Select(x =>
                        new InspectionTemplateItemUpdateData(
                            x.Id,
                            x.DisplayOrder,
                            x.ItemName.Trim(),
                            x.GetInputType(),
                            NormalizeText(x.Unit),
                            x.MinimumValue,
                            x.MaximumValue,
                            x.IsRequired,
                            x.IsActive))
                    .ToList();

            await _inspectionTemplateRepository
                .UpdateAsync(
                    SelectedTemplate.Id,
                    EditTemplateName.Trim(),
                    updateItems);

            IsEditDialogOpen = false;
            EditingItems.Clear();

            OperationMessage =
                "点検票テンプレートを更新しました。";

            await LoadTemplatesAsync();
        }
        catch (Exception exception)
        {
            EditErrorMessage =
                $"点検票テンプレートを更新できませんでした。" +
                $"{Environment.NewLine}{exception.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

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
            IsSaving = true;
            ErrorMessage = null;
            OperationMessage = null;

            await _inspectionTemplateRepository
                .SetActiveAsync(
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
                $"テンプレートの状態を変更できませんでした。" +
                $"{Environment.NewLine}{exception.Message}";
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool ValidateEditInput()
    {
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
                x => string.IsNullOrWhiteSpace(
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
                .GroupBy(x => x.DisplayOrder)
                .FirstOrDefault(x => x.Count() > 1);

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

    private static string? NormalizeText(
        string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }

    private static InspectionTemplateListItemViewModel
        CreateListItemViewModel(
            InspectionTemplate template)
    {
        var items =
            template.Items
                .OrderBy(x => x.DisplayOrder)
                .Select(item =>
                    new InspectionTemplateItemRowViewModel(
                        item.Id,
                        item.DisplayOrder,
                        item.ItemName,
                        item.InputType,
                        GetInputTypeName(item.InputType),
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

            _ => equipmentType.ToString()
        };
    }

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

            _ => inputType.ToString()
        };
    }
}