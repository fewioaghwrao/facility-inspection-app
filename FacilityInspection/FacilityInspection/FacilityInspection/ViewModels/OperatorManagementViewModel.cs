using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using FacilityInspection.Data;
using FacilityInspection.Domain.Operators;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

namespace FacilityInspection.ViewModels;

public sealed partial class OperatorManagementViewModel
    : ViewModelBase
{
    private const int MinimumPasswordLength = 8;


    // ============================================
    // Dependencies
    // ============================================

    private readonly Guid
        _currentOperatorId;

    private readonly Func<
        Task<IReadOnlyList<Operator>>>
        _getAllAsync;

    private readonly Func<
        string,
        string,
        OperatorRole,
        string,
        Task>
        _createAsync;

    private readonly Func<
        Guid,
        string,
        string,
        OperatorRole,
        string?,
        Task>
        _updateAsync;

    private readonly Func<
        Guid,
        bool,
        Task>
        _setActiveAsync;


    // ============================================
    // Constructor
    // 本番用
    // ============================================

    public OperatorManagementViewModel(
        OperatorRepository operatorRepository,
        Guid currentOperatorId)
    {
        ArgumentNullException.ThrowIfNull(
            operatorRepository);

        if (currentOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "現在の担当者IDを指定してください。",
                nameof(currentOperatorId));
        }


        _currentOperatorId =
            currentOperatorId;


        /*
         * Repository側のCancellationTokenや
         * 戻り値をViewModelから切り離す。
         */
        _getAllAsync =
            async () =>
                await operatorRepository
                    .GetAllAsync();


        _createAsync =
            async (
                loginId,
                displayName,
                role,
                password) =>
            {
                await operatorRepository
                    .CreateAsync(
                        loginId,
                        displayName,
                        role,
                        password);
            };


        _updateAsync =
            async (
                operatorId,
                loginId,
                displayName,
                role,
                newPassword) =>
            {
                await operatorRepository
                    .UpdateAsync(
                        operatorId,
                        loginId,
                        displayName,
                        role,
                        newPassword);
            };


        _setActiveAsync =
            async (
                operatorId,
                isActive) =>
            {
                await operatorRepository
                    .SetActiveAsync(
                        operatorId,
                        isActive);
            };


        /*
         * 本番では従来どおり
         * 生成時に自動ロードする。
         */
        _ = LoadOperatorsAsync();
    }


    // ============================================
    // Constructor
    // テスト用
    // ============================================

    internal OperatorManagementViewModel(
        Guid currentOperatorId,
        Func<
            Task<IReadOnlyList<Operator>>>
            getAllAsync,
        Func<
            string,
            string,
            OperatorRole,
            string,
            Task>
            createAsync,
        Func<
            Guid,
            string,
            string,
            OperatorRole,
            string?,
            Task>
            updateAsync,
        Func<
            Guid,
            bool,
            Task>
            setActiveAsync)
    {
        if (currentOperatorId == Guid.Empty)
        {
            throw new ArgumentException(
                "現在の担当者IDを指定してください。",
                nameof(currentOperatorId));
        }

        ArgumentNullException.ThrowIfNull(
            getAllAsync);

        ArgumentNullException.ThrowIfNull(
            createAsync);

        ArgumentNullException.ThrowIfNull(
            updateAsync);

        ArgumentNullException.ThrowIfNull(
            setActiveAsync);


        _currentOperatorId =
            currentOperatorId;

        _getAllAsync =
            getAllAsync;

        _createAsync =
            createAsync;

        _updateAsync =
            updateAsync;

        _setActiveAsync =
            setActiveAsync;


        /*
         * テスト用では自動ロードしない。
         */
    }


    // ============================================
    // Operators
    // ============================================

    public ObservableCollection<
        OperatorListItemViewModel>
        Operators
    {
        get;
    } = [];


    // ============================================
    // Role
    // ============================================

    public IReadOnlyList<string>
        RoleChoices
    {
        get;
    } =
    [
        "点検担当者",
        "保全責任者"
    ];


    // ============================================
    // Loading
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(IsEmpty))]
    private bool isLoading;


    // ============================================
    // Error
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasError))]
    private string? errorMessage;


    // ============================================
    // Operation Message
    // ============================================

    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasOperationMessage))]
    private string? operationMessage;


    // ============================================
    // Editor
    // ============================================

    [ObservableProperty]
    private bool isEditorOpen;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(EditorTitle))]
    [NotifyPropertyChangedFor(
        nameof(EditorDescription))]
    [NotifyPropertyChangedFor(
        nameof(SaveButtonText))]
    [NotifyPropertyChangedFor(
        nameof(PasswordLabel))]
    [NotifyPropertyChangedFor(
        nameof(PasswordWatermark))]
    private bool isCreateMode;


    [ObservableProperty]
    private Guid? editingOperatorId;


    [ObservableProperty]
    private OperatorRole?
        originalEditingRole;


    [ObservableProperty]
    private string editLoginId =
        string.Empty;


    [ObservableProperty]
    private string editDisplayName =
        string.Empty;


    [ObservableProperty]
    private string selectedRoleName =
        "点検担当者";


    [ObservableProperty]
    private string editPassword =
        string.Empty;


    [ObservableProperty]
    private string editPasswordConfirmation =
        string.Empty;


    [ObservableProperty]
    private bool isSaving;


    [ObservableProperty]
    [NotifyPropertyChangedFor(
        nameof(HasEditorError))]
    private string? editorErrorMessage;


    // ============================================
    // Calculated
    // ============================================

    public bool HasError =>
        !string.IsNullOrWhiteSpace(
            ErrorMessage);


    public bool HasOperationMessage =>
        !string.IsNullOrWhiteSpace(
            OperationMessage);


    public bool HasEditorError =>
        !string.IsNullOrWhiteSpace(
            EditorErrorMessage);


    public bool IsEmpty =>
        !IsLoading &&
        Operators.Count == 0 &&
        !HasError;


    // ============================================
    // Editor Display
    // ============================================

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


    // ============================================
    // Load
    // ============================================

    [RelayCommand]
    internal async Task LoadOperatorsAsync()
    {
        if (IsLoading)
        {
            return;
        }


        try
        {
            IsLoading =
                true;

            ErrorMessage =
                null;


            var operators =
                await _getAllAsync();


            Operators.Clear();


            foreach (var operatorEntity in
                     operators)
            {
                Operators.Add(
                    CreateListItemViewModel(
                        operatorEntity));
            }
        }
        catch (Exception exception)
        {
            ErrorMessage =
                "担当者一覧を読み込めませんでした。" +
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
    // Create Editor
    // ============================================

    [RelayCommand]
    private void OpenCreateEditor()
    {
        if (IsSaving)
        {
            return;
        }


        IsCreateMode =
            true;

        EditingOperatorId =
            null;

        OriginalEditingRole =
            null;


        EditLoginId =
            string.Empty;

        EditDisplayName =
            string.Empty;

        SelectedRoleName =
            RoleChoices[0];

        EditPassword =
            string.Empty;

        EditPasswordConfirmation =
            string.Empty;


        EditorErrorMessage =
            null;

        OperationMessage =
            null;

        IsEditorOpen =
            true;
    }


    // ============================================
    // Edit Editor
    // ============================================

    private void OpenEditEditor(
        OperatorListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(
            item);


        if (IsSaving)
        {
            return;
        }


        IsCreateMode =
            false;

        EditingOperatorId =
            item.Id;

        OriginalEditingRole =
            item.Role;


        EditLoginId =
            item.LoginId;

        EditDisplayName =
            item.DisplayName;

        SelectedRoleName =
            ConvertRoleToName(
                item.Role);


        EditPassword =
            string.Empty;

        EditPasswordConfirmation =
            string.Empty;


        EditorErrorMessage =
            null;

        OperationMessage =
            null;

        IsEditorOpen =
            true;
    }


    // ============================================
    // Cancel Editor
    // ============================================

    [RelayCommand]
    private void CancelEditor()
    {
        if (IsSaving)
        {
            return;
        }


        IsEditorOpen =
            false;

        EditorErrorMessage =
            null;


        ClearEditorValues();
    }


    // ============================================
    // Save
    // ============================================

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
            IsSaving =
                true;

            EditorErrorMessage =
                null;

            OperationMessage =
                null;


            var role =
                ConvertNameToRole(
                    SelectedRoleName);


            // ------------------------------------
            // Create
            // ------------------------------------

            if (IsCreateMode)
            {
                await _createAsync(
                    EditLoginId.Trim(),
                    EditDisplayName.Trim(),
                    role,
                    EditPassword);


                OperationMessage =
                    "担当者を登録しました。";
            }

            // ------------------------------------
            // Update
            // ------------------------------------

            else
            {
                if (!EditingOperatorId.HasValue)
                {
                    throw new InvalidOperationException(
                        "編集対象の担当者が選択されていません。");
                }


                /*
                 * 現在ログイン中の担当者は
                 * 自分自身の権限を変更できない。
                 */
                if (EditingOperatorId.Value ==
                        _currentOperatorId &&
                    OriginalEditingRole.HasValue &&
                    role !=
                        OriginalEditingRole.Value)
                {
                    throw new InvalidOperationException(
                        "現在ログイン中の担当者は、自分自身の権限を変更できません。");
                }


                var newPassword =
                    string.IsNullOrEmpty(
                        EditPassword)
                        ? null
                        : EditPassword;


                await _updateAsync(
                    EditingOperatorId.Value,
                    EditLoginId.Trim(),
                    EditDisplayName.Trim(),
                    role,
                    newPassword);


                OperationMessage =
                    "担当者情報を更新しました。";
            }


            IsEditorOpen =
                false;


            ClearEditorValues();


            /*
             * DB更新後に最新一覧を再取得する。
             */
            await LoadOperatorsAsync();
        }
        catch (Exception exception)
        {
            EditorErrorMessage =
                IsCreateMode
                    ? "担当者を登録できませんでした。" +
                      Environment.NewLine +
                      exception.Message

                    : "担当者情報を更新できませんでした。" +
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

    private async Task ToggleActiveAsync(
        OperatorListItemViewModel item)
    {
        ArgumentNullException.ThrowIfNull(
            item);


        if (IsSaving)
        {
            return;
        }


        /*
         * OperatorListItemViewModel側でも
         * CanExecute=falseになるが、
         * ViewModel側でも防御する。
         */
        if (item.Id ==
            _currentOperatorId)
        {
            ErrorMessage =
                "現在ログイン中の担当者は無効化できません。";

            return;
        }


        try
        {
            IsSaving =
                true;

            ErrorMessage =
                null;

            OperationMessage =
                null;


            var newActiveState =
                !item.IsActive;


            await _setActiveAsync(
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
                "担当者の状態を変更できませんでした。" +
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
    // Validation
    // ============================================

    private bool ValidateEditor()
    {
        // ----------------------------------------
        // Login ID
        // ----------------------------------------

        if (string.IsNullOrWhiteSpace(
                EditLoginId))
        {
            EditorErrorMessage =
                "ログインIDを入力してください。";

            return false;
        }


        if (EditLoginId.Trim().Length >
            50)
        {
            EditorErrorMessage =
                "ログインIDは50文字以内で入力してください。";

            return false;
        }


        // ----------------------------------------
        // Display Name
        // ----------------------------------------

        if (string.IsNullOrWhiteSpace(
                EditDisplayName))
        {
            EditorErrorMessage =
                "表示名を入力してください。";

            return false;
        }


        if (EditDisplayName.Trim().Length >
            100)
        {
            EditorErrorMessage =
                "表示名は100文字以内で入力してください。";

            return false;
        }


        // ----------------------------------------
        // Role
        // ----------------------------------------

        if (string.IsNullOrWhiteSpace(
                SelectedRoleName))
        {
            EditorErrorMessage =
                "権限を選択してください。";

            return false;
        }


        // ----------------------------------------
        // Password
        // ----------------------------------------

        var passwordEntered =
            !string.IsNullOrEmpty(
                EditPassword) ||
            !string.IsNullOrEmpty(
                EditPasswordConfirmation);


        /*
         * 新規登録の場合、
         * パスワードは必須。
         */
        if (IsCreateMode &&
            !passwordEntered)
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


    // ============================================
    // Create List Item
    // ============================================

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


    // ============================================
    // Clear Editor
    // ============================================

    private void ClearEditorValues()
    {
        EditingOperatorId =
            null;

        OriginalEditingRole =
            null;

        EditLoginId =
            string.Empty;

        EditDisplayName =
            string.Empty;

        SelectedRoleName =
            RoleChoices[0];

        EditPassword =
            string.Empty;

        EditPasswordConfirmation =
            string.Empty;
    }


    // ============================================
    // Role Name -> Role
    // ============================================

    private static OperatorRole
        ConvertNameToRole(
            string roleName)
    {
        return roleName switch
        {
            "点検担当者" =>
                OperatorRole.Inspector,

            "保全責任者" =>
                OperatorRole.MaintenanceManager,

            _ =>
                throw new InvalidOperationException(
                    $"未対応の権限です: {roleName}")
        };
    }


    // ============================================
    // Role -> Role Name
    // ============================================

    private static string
        ConvertRoleToName(
            OperatorRole role)
    {
        return role switch
        {
            OperatorRole.Inspector =>
                "点検担当者",

            OperatorRole.MaintenanceManager =>
                "保全責任者",

            _ =>
                role.ToString()
        };
    }
}