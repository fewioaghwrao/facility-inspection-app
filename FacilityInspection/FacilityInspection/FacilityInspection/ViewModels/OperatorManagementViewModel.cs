using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class OperatorManagementViewModel
    : ViewModelBase
{
    private const int MinimumPasswordLength = 8;

    private readonly OperatorRepository
        _operatorRepository;

    private readonly Guid _currentOperatorId;

    public OperatorManagementViewModel(
        OperatorRepository operatorRepository,
        Guid currentOperatorId)
    {
        ArgumentNullException.ThrowIfNull(
            operatorRepository);

        _operatorRepository = operatorRepository;
        _currentOperatorId = currentOperatorId;

        _ = LoadOperatorsAsync();
    }

    public ObservableCollection<OperatorListItemViewModel>
        Operators
    { get; } = [];

    public IReadOnlyList<string> RoleChoices { get; } =
    [
        "点検担当者",
        "保全責任者"
    ];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsEmpty))]
    private bool isLoading;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasError))]
    private string? errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasOperationMessage))]
    private string? operationMessage;

    [ObservableProperty]
    private bool isEditorOpen;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EditorTitle))]
    [NotifyPropertyChangedFor(nameof(EditorDescription))]
    [NotifyPropertyChangedFor(nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(nameof(PasswordLabel))]
    [NotifyPropertyChangedFor(nameof(PasswordWatermark))]
    private bool isCreateMode;

    [ObservableProperty]
    private Guid? editingOperatorId;

    [ObservableProperty]
    private OperatorRole? originalEditingRole;

    [ObservableProperty]
    private string editLoginId = string.Empty;

    [ObservableProperty]
    private string editDisplayName = string.Empty;

    [ObservableProperty]
    private string selectedRoleName = "点検担当者";

    [ObservableProperty]
    private string editPassword = string.Empty;

    [ObservableProperty]
    private string editPasswordConfirmation =
        string.Empty;

    [ObservableProperty]
    private bool isSaving;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasEditorError))]
    private string? editorErrorMessage;

    public bool HasError =>
        !string.IsNullOrWhiteSpace(ErrorMessage);

    public bool HasOperationMessage =>
        !string.IsNullOrWhiteSpace(OperationMessage);

    public bool HasEditorError =>
        !string.IsNullOrWhiteSpace(
            EditorErrorMessage);

    public bool IsEmpty =>
        !IsLoading &&
        Operators.Count == 0 &&
        !HasError;

    public string EditorTitle =>
        IsCreateMode
            ? "担当者の新規登録"
            : "担当者の編集";

    public string EditorDescription =>
        IsCreateMode
            ? "ログイン情報と権限を設定して担当者を登録します。"
            : "担当者名・ログインID・権限を変更します。";

    public string SaveButtonText =>
        IsCreateMode
            ? "登録"
            : "保存";

    public string PasswordLabel =>
        IsCreateMode
            ? "パスワード"
            : "新しいパスワード";

    public string PasswordWatermark =>
        IsCreateMode
            ? "8文字以上で入力"
            : "変更しない場合は空欄";

    [RelayCommand]
    private async Task LoadOperatorsAsync()
    {
        if (IsLoading)
        {
            return;
        }

        try
        {
            IsLoading = true;
            ErrorMessage = null;

            var operators =
                await _operatorRepository.GetAllAsync();

            Operators.Clear();

            foreach (var operatorEntity in operators)
            {
                Operators.Add(
                    CreateListItemViewModel(
                        operatorEntity));
            }
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"担当者一覧を読み込めませんでした。" +
                $"{Environment.NewLine}" +
                exception.Message;
        }
        finally
        {
            IsLoading = false;

            OnPropertyChanged(nameof(IsEmpty));
        }
    }

    [RelayCommand]
    private void OpenCreateEditor()
    {
        if (IsSaving)
        {
            return;
        }

        IsCreateMode = true;
        EditingOperatorId = null;
        OriginalEditingRole = null;

        EditLoginId = string.Empty;
        EditDisplayName = string.Empty;
        SelectedRoleName = RoleChoices[0];
        EditPassword = string.Empty;
        EditPasswordConfirmation = string.Empty;

        EditorErrorMessage = null;
        OperationMessage = null;
        IsEditorOpen = true;
    }

    private void OpenEditEditor(
        OperatorListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsSaving)
        {
            return;
        }

        IsCreateMode = false;
        EditingOperatorId = item.Id;
        OriginalEditingRole = item.Role;

        EditLoginId = item.LoginId;
        EditDisplayName = item.DisplayName;
        SelectedRoleName =
            ConvertRoleToName(item.Role);

        EditPassword = string.Empty;
        EditPasswordConfirmation = string.Empty;

        EditorErrorMessage = null;
        OperationMessage = null;
        IsEditorOpen = true;
    }

    [RelayCommand]
    private void CancelEditor()
    {
        if (IsSaving)
        {
            return;
        }

        IsEditorOpen = false;
        EditorErrorMessage = null;
        ClearEditorValues();
    }

    [RelayCommand]
    private async Task SaveOperatorAsync()
    {
        if (IsSaving)
        {
            return;
        }

        if (!ValidateEditor())
        {
            return;
        }

        try
        {
            IsSaving = true;
            EditorErrorMessage = null;
            OperationMessage = null;

            var role =
                ConvertNameToRole(
                    SelectedRoleName);

            if (IsCreateMode)
            {
                await _operatorRepository.CreateAsync(
                    EditLoginId.Trim(),
                    EditDisplayName.Trim(),
                    role,
                    EditPassword);

                OperationMessage =
                    "担当者を登録しました。";
            }
            else
            {
                if (!EditingOperatorId.HasValue)
                {
                    throw new InvalidOperationException(
                        "編集対象の担当者が選択されていません。");
                }

                if (EditingOperatorId.Value ==
                        _currentOperatorId &&
                    OriginalEditingRole.HasValue &&
                    role != OriginalEditingRole.Value)
                {
                    throw new InvalidOperationException(
                        "現在ログイン中の担当者は、自分自身の権限を変更できません。");
                }

                var newPassword =
                    string.IsNullOrEmpty(EditPassword)
                        ? null
                        : EditPassword;

                await _operatorRepository.UpdateAsync(
                    EditingOperatorId.Value,
                    EditLoginId.Trim(),
                    EditDisplayName.Trim(),
                    role,
                    newPassword);

                OperationMessage =
                    "担当者情報を更新しました。";
            }

            IsEditorOpen = false;
            ClearEditorValues();

            await LoadOperatorsAsync();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                IsCreateMode
                    ? $"担当者を登録できませんでした。" +
                      $"{Environment.NewLine}" +
                      exception.Message
                    : $"担当者情報を更新できませんでした。" +
                      $"{Environment.NewLine}" +
                      exception.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private async Task ToggleActiveAsync(
        OperatorListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(item);

        if (IsSaving)
        {
            return;
        }

        if (item.Id == _currentOperatorId)
        {
            ErrorMessage =
                "現在ログイン中の担当者は無効化できません。";

            return;
        }

        try
        {
            IsSaving = true;
            ErrorMessage = null;
            OperationMessage = null;

            var newActiveState =
                !item.IsActive;

            await _operatorRepository.SetActiveAsync(
                item.Id,
                newActiveState);

            OperationMessage =
                newActiveState
                    ? "担当者を有効化しました。"
                    : "担当者を無効化しました。";

            await LoadOperatorsAsync();
        }
        catch (Exception exception)
        {
            ErrorMessage =
                $"担当者の状態を変更できませんでした。" +
                $"{Environment.NewLine}" +
                exception.Message;
        }
        finally
        {
            IsSaving = false;
        }
    }

    private bool ValidateEditor()
    {
        if (string.IsNullOrWhiteSpace(
                EditLoginId))
        {
            EditorErrorMessage =
                "ログインIDを入力してください。";

            return false;
        }

        if (EditLoginId.Trim().Length > 50)
        {
            EditorErrorMessage =
                "ログインIDは50文字以内で入力してください。";

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                EditDisplayName))
        {
            EditorErrorMessage =
                "表示名を入力してください。";

            return false;
        }

        if (EditDisplayName.Trim().Length > 100)
        {
            EditorErrorMessage =
                "表示名は100文字以内で入力してください。";

            return false;
        }

        if (string.IsNullOrWhiteSpace(
                SelectedRoleName))
        {
            EditorErrorMessage =
                "権限を選択してください。";

            return false;
        }

        var passwordEntered =
            !string.IsNullOrEmpty(EditPassword) ||
            !string.IsNullOrEmpty(
                EditPasswordConfirmation);

        if (IsCreateMode && !passwordEntered)
        {
            EditorErrorMessage =
                "パスワードを入力してください。";

            return false;
        }

        if (passwordEntered)
        {
            if (EditPassword.Length <
                MinimumPasswordLength)
            {
                EditorErrorMessage =
                    $"パスワードは{MinimumPasswordLength}文字以上で入力してください。";

                return false;
            }

            if (EditPassword !=
                EditPasswordConfirmation)
            {
                EditorErrorMessage =
                    "パスワードと確認用パスワードが一致しません。";

                return false;
            }
        }

        return true;
    }

    private OperatorListItemViewModel
        CreateListItemViewModel(
            Operator operatorEntity)
    {
        return new OperatorListItemViewModel(
            operatorEntity.Id,
            operatorEntity.LoginId,
            operatorEntity.DisplayName,
            operatorEntity.Role,
            operatorEntity.IsActive,
            operatorEntity.LastLoginAt,
            operatorEntity.Id ==
                _currentOperatorId,
            OpenEditEditor,
            ToggleActiveAsync);
    }

    private void ClearEditorValues()
    {
        EditingOperatorId = null;
        OriginalEditingRole = null;
        EditLoginId = string.Empty;
        EditDisplayName = string.Empty;
        SelectedRoleName = RoleChoices[0];
        EditPassword = string.Empty;
        EditPasswordConfirmation = string.Empty;
    }

    private static OperatorRole ConvertNameToRole(
        string roleName)
    {
        return roleName switch
        {
            "点検担当者" =>
                OperatorRole.Inspector,

            "保全責任者" =>
                OperatorRole.MaintenanceManager,

            _ => throw new InvalidOperationException(
                $"未対応の権限です: {roleName}")
        };
    }

    private static string ConvertRoleToName(
        OperatorRole role)
    {
        return role switch
        {
            OperatorRole.Inspector =>
                "点検担当者",

            OperatorRole.MaintenanceManager =>
                "保全責任者",

            _ => role.ToString()
        };
    }
}
