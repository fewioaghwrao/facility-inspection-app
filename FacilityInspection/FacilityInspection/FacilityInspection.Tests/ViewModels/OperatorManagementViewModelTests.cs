using FacilityInspection.Domain.Operators;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class OperatorManagementViewModelTests
{
    private static readonly Guid
        DefaultCurrentOperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithEmptyCurrentOperatorId_ThrowsArgumentException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new OperatorManagementViewModel(
                        Guid.Empty,
                        EmptyOperators,
                        (
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask,
                        (
                            _,
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask,
                        (
                            _,
                            _) =>
                            Task.CompletedTask));


        // Assert
        Assert.Equal(
            "currentOperatorId",
            exception.ParamName);

        Assert.Contains(
            "現在の担当者IDを指定してください。",
            exception.Message);
    }


    [Fact]
    public void Constructor_WithNullGetAll_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new OperatorManagementViewModel(
                        DefaultCurrentOperatorId,
                        null!,
                        (
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask,
                        (
                            _,
                            _,
                            _,
                            _,
                            _) =>
                            Task.CompletedTask,
                        (
                            _,
                            _) =>
                            Task.CompletedTask));


        // Assert
        Assert.Equal(
            "getAllAsync",
            exception.ParamName);
    }


    [Theory]
    [InlineData("create")]
    [InlineData("update")]
    [InlineData("setActive")]
    public void Constructor_WithNullWriteDelegate_ThrowsArgumentNullException(
        string target)
    {
        // Arrange
        Func<
            string,
            string,
            OperatorRole,
            string,
            Task>
            createAsync =
                (
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask;


        Func<
            Guid,
            string,
            string,
            OperatorRole,
            string?,
            Task>
            updateAsync =
                (
                    _,
                    _,
                    _,
                    _,
                    _) =>
                    Task.CompletedTask;


        Func<
            Guid,
            bool,
            Task>
            setActiveAsync =
                (
                    _,
                    _) =>
                    Task.CompletedTask;


        switch (target)
        {
            case "create":
                createAsync =
                    null!;
                break;

            case "update":
                updateAsync =
                    null!;
                break;

            case "setActive":
                setActiveAsync =
                    null!;
                break;
        }


        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new OperatorManagementViewModel(
                        DefaultCurrentOperatorId,
                        EmptyOperators,
                        createAsync,
                        updateAsync,
                        setActiveAsync));


        // Assert
        var expectedParameterName =
            target switch
            {
                "create" =>
                    "createAsync",

                "update" =>
                    "updateAsync",

                "setActive" =>
                    "setActiveAsync",

                _ =>
                    throw new InvalidOperationException()
            };


        Assert.Equal(
            expectedParameterName,
            exception.ParamName);
    }


    [Fact]
    public void Constructor_InitializesExpectedStateWithoutAutoLoad()
    {
        // Arrange
        var getAllCallCount =
            0;


        // Act
        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return EmptyOperators();
                    });


        // Assert
        Assert.Equal(
            0,
            getAllCallCount);

        Assert.Empty(
            sut.Operators);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.IsSaving);

        Assert.False(
            sut.IsEditorOpen);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.HasOperationMessage);

        Assert.False(
            sut.HasEditorError);

        Assert.True(
            sut.IsEmpty);

        Assert.Equal(
            "点検担当者",
            sut.SelectedRoleName);


        Assert.Collection(
            sut.RoleChoices,

            role =>
                Assert.Equal(
                    "点検担当者",
                    role),

            role =>
                Assert.Equal(
                    "保全責任者",
                    role));
    }


    // ============================================
    // Load
    // ============================================

    [Fact]
    public async Task LoadOperatorsAsync_LoadsOperatorsAndMarksCurrentUser()
    {
        // Arrange
        var current =
            CreateOperator(
                loginId:
                    "manager",

                displayName:
                    "保全責任者",

                role:
                    OperatorRole.MaintenanceManager);


        var other =
            CreateOperator(
                loginId:
                    "inspector01",

                displayName:
                    "点検担当者A",

                role:
                    OperatorRole.Inspector);


        IReadOnlyList<Operator>
            operators =
            [
                current,
                other
            ];


        var sut =
            CreateViewModel(
                currentOperatorId:
                    current.Id,

                getAllAsync:
                    () =>
                        Task.FromResult(
                            operators));


        // Act
        await sut.LoadOperatorsAsync();


        // Assert
        Assert.Equal(
            2,
            sut.Operators.Count);

        Assert.False(
            sut.IsLoading);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsEmpty);


        var currentItem =
            sut.Operators.Single(
                x =>
                    x.Id ==
                    current.Id);


        var otherItem =
            sut.Operators.Single(
                x =>
                    x.Id ==
                    other.Id);


        Assert.True(
            currentItem.IsCurrentUser);

        Assert.False(
            currentItem.CanToggleActive);

        Assert.Equal(
            "ログイン中",
            currentItem.ToggleActiveText);


        Assert.False(
            otherItem.IsCurrentUser);

        Assert.True(
            otherItem.CanToggleActive);
    }


    [Fact]
    public async Task LoadOperatorsAsync_WhileLoading_IgnoresSecondRequest()
    {
        // Arrange
        var callCount =
            0;


        var completionSource =
            new TaskCompletionSource<
                IReadOnlyList<Operator>>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        callCount++;

                        return completionSource.Task;
                    });


        // Act
        var firstLoad =
            sut.LoadOperatorsAsync();


        Assert.True(
            sut.IsLoading);


        var secondLoad =
            sut.LoadOperatorsAsync();


        await secondLoad;


        // Assert
        Assert.Equal(
            1,
            callCount);

        Assert.True(
            sut.IsLoading);


        completionSource.SetResult(
            []);


        await firstLoad;


        Assert.False(
            sut.IsLoading);

        Assert.Equal(
            1,
            callCount);
    }


    [Fact]
    public async Task LoadOperatorsAsync_WhenLoaderFails_SetsError()
    {
        // Arrange
        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                        Task.FromException<
                            IReadOnlyList<Operator>>(
                            new InvalidOperationException(
                                "一覧取得テストエラー")));


        // Act
        await sut.LoadOperatorsAsync();


        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "担当者一覧を読み込めませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "一覧取得テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsLoading);

        /*
         * エラー表示中なので
         * IsEmptyはfalse。
         */
        Assert.False(
            sut.IsEmpty);
    }


    [Fact]
    public async Task LoadOperatorsAsync_AfterFailureThenSuccess_ClearsError()
    {
        // Arrange
        var shouldFail =
            true;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        if (shouldFail)
                        {
                            return Task.FromException<
                                IReadOnlyList<Operator>>(
                                new InvalidOperationException(
                                    "一時エラー"));
                        }


                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            CreateOperator()
                        ]);
                    });


        await sut.LoadOperatorsAsync();


        Assert.True(
            sut.HasError);


        // Act
        shouldFail =
            false;


        await sut.LoadOperatorsAsync();


        // Assert
        Assert.False(
            sut.HasError);

        Assert.Null(
            sut.ErrorMessage);

        Assert.Single(
            sut.Operators);

        Assert.False(
            sut.IsEmpty);
    }


    // ============================================
    // Open Create
    // ============================================

    [Fact]
    public void OpenCreateEditorCommand_ResetsEditorAndOpensCreateMode()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.IsCreateMode =
            false;

        sut.EditingOperatorId =
            Guid.NewGuid();

        sut.OriginalEditingRole =
            OperatorRole.MaintenanceManager;

        sut.EditLoginId =
            "old-login";

        sut.EditDisplayName =
            "old-name";

        sut.SelectedRoleName =
            "保全責任者";

        sut.EditPassword =
            "OldPassword";

        sut.EditPasswordConfirmation =
            "OldPassword";

        sut.EditorErrorMessage =
            "旧エラー";

        sut.OperationMessage =
            "旧メッセージ";


        // Act
        sut.OpenCreateEditorCommand
            .Execute(null);


        // Assert
        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.IsCreateMode);

        Assert.Null(
            sut.EditingOperatorId);

        Assert.Null(
            sut.OriginalEditingRole);

        Assert.Equal(
            string.Empty,
            sut.EditLoginId);

        Assert.Equal(
            string.Empty,
            sut.EditDisplayName);

        Assert.Equal(
            "点検担当者",
            sut.SelectedRoleName);

        Assert.Equal(
            string.Empty,
            sut.EditPassword);

        Assert.Equal(
            string.Empty,
            sut.EditPasswordConfirmation);

        Assert.Null(
            sut.EditorErrorMessage);

        Assert.Null(
            sut.OperationMessage);


        Assert.Equal(
            "担当者の新規登録",
            sut.EditorTitle);

        Assert.Equal(
            "登録",
            sut.SaveButtonText);

        Assert.Equal(
            "パスワード",
            sut.PasswordLabel);

        Assert.Equal(
            "8文字以上で入力",
            sut.PasswordWatermark);
    }


    [Fact]
    public void OpenCreateEditorCommand_WhileSaving_DoesNothing()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.IsSaving =
            true;

        sut.EditLoginId =
            "existing";

        sut.IsEditorOpen =
            false;


        // Act
        sut.OpenCreateEditorCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsEditorOpen);

        Assert.Equal(
            "existing",
            sut.EditLoginId);
    }


    // ============================================
    // Cancel
    // ============================================

    [Fact]
    public void CancelEditorCommand_ClosesAndClearsEditor()
    {
        // Arrange
        var sut =
            CreateViewModel();


        sut.OpenCreateEditorCommand
            .Execute(null);


        sut.EditLoginId =
            "test";

        sut.EditDisplayName =
            "テスト";

        sut.SelectedRoleName =
            "保全責任者";

        sut.EditPassword =
            "Password1";

        sut.EditPasswordConfirmation =
            "Password1";

        sut.EditorErrorMessage =
            "エラー";


        // Act
        sut.CancelEditorCommand
            .Execute(null);


        // Assert
        Assert.False(
            sut.IsEditorOpen);

        Assert.Null(
            sut.EditorErrorMessage);

        Assert.Null(
            sut.EditingOperatorId);

        Assert.Null(
            sut.OriginalEditingRole);

        Assert.Equal(
            string.Empty,
            sut.EditLoginId);

        Assert.Equal(
            string.Empty,
            sut.EditDisplayName);

        Assert.Equal(
            "点検担当者",
            sut.SelectedRoleName);

        Assert.Equal(
            string.Empty,
            sut.EditPassword);

        Assert.Equal(
            string.Empty,
            sut.EditPasswordConfirmation);
    }


    // ============================================
    // Validation - Login ID
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_WithBlankLoginId_SetsValidationError()
    {
        // Arrange
        var createCallCount =
            0;


        var sut =
            CreateViewModel(
                createAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                    {
                        createCallCount++;

                        return Task.CompletedTask;
                    });


        PrepareValidCreateEditor(
            sut);


        sut.EditLoginId =
            "   ";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "ログインIDを入力してください。",
            sut.EditorErrorMessage);

        Assert.Equal(
            0,
            createCallCount);

        Assert.False(
            sut.IsSaving);
    }


    [Fact]
    public async Task SaveOperatorCommand_WithLoginIdOver50Characters_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditLoginId =
            new string(
                'a',
                51);


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "ログインIDは50文字以内で入力してください。",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Validation - Display Name
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_WithBlankDisplayName_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditDisplayName =
            "   ";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "表示名を入力してください。",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveOperatorCommand_WithDisplayNameOver100Characters_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditDisplayName =
            new string(
                'あ',
                101);


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "表示名は100文字以内で入力してください。",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Validation - Role
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_WithBlankRole_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.SelectedRoleName =
            "   ";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "権限を選択してください。",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Validation - Password
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_CreateWithoutPassword_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditPassword =
            string.Empty;

        sut.EditPasswordConfirmation =
            string.Empty;


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "パスワードを入力してください。",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveOperatorCommand_WithShortPassword_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditPassword =
            "1234567";

        sut.EditPasswordConfirmation =
            "1234567";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "パスワードは8文字以上で入力してください。",
            sut.EditorErrorMessage);
    }


    [Fact]
    public async Task SaveOperatorCommand_WithPasswordMismatch_SetsValidationError()
    {
        // Arrange
        var sut =
            CreateViewModel();


        PrepareValidCreateEditor(
            sut);


        sut.EditPassword =
            "Password1";

        sut.EditPasswordConfirmation =
            "Password2";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "パスワードと確認用パスワードが一致しません。",
            sut.EditorErrorMessage);
    }


    // ============================================
    // Create
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_Create_SendsTrimmedValuesAndReloads()
    {
        // Arrange
        string? capturedLoginId =
            null;

        string? capturedDisplayName =
            null;

        OperatorRole?
            capturedRole =
                null;

        string? capturedPassword =
            null;

        var createCallCount =
            0;

        var getAllCallCount =
            0;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return EmptyOperators();
                    },

                createAsync:
                    (
                        loginId,
                        displayName,
                        role,
                        password) =>
                    {
                        createCallCount++;

                        capturedLoginId =
                            loginId;

                        capturedDisplayName =
                            displayName;

                        capturedRole =
                            role;

                        capturedPassword =
                            password;


                        return Task.CompletedTask;
                    });


        PrepareValidCreateEditor(
            sut);


        sut.EditLoginId =
            "  manager02  ";

        sut.EditDisplayName =
            "  保全責任者B  ";

        sut.SelectedRoleName =
            "保全責任者";

        sut.EditPassword =
            "Password1";

        sut.EditPasswordConfirmation =
            "Password1";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            createCallCount);

        Assert.Equal(
            "manager02",
            capturedLoginId);

        Assert.Equal(
            "保全責任者B",
            capturedDisplayName);

        Assert.Equal(
            OperatorRole.MaintenanceManager,
            capturedRole);

        Assert.Equal(
            "Password1",
            capturedPassword);


        Assert.Equal(
            "担当者を登録しました。",
            sut.OperationMessage);

        Assert.True(
            sut.HasOperationMessage);

        Assert.False(
            sut.IsEditorOpen);

        Assert.False(
            sut.IsSaving);

        Assert.Null(
            sut.EditorErrorMessage);


        /*
         * 登録成功後に一覧再取得。
         */
        Assert.Equal(
            1,
            getAllCallCount);


        Assert.Equal(
            string.Empty,
            sut.EditLoginId);

        Assert.Equal(
            string.Empty,
            sut.EditDisplayName);

        Assert.Equal(
            "点検担当者",
            sut.SelectedRoleName);

        Assert.Equal(
            string.Empty,
            sut.EditPassword);
    }


    [Fact]
    public async Task SaveOperatorCommand_WhenCreateFails_SetsEditorErrorAndKeepsEditorOpen()
    {
        // Arrange
        var getAllCallCount =
            0;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return EmptyOperators();
                    },

                createAsync:
                    (
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "登録テストエラー")));


        PrepareValidCreateEditor(
            sut);


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasEditorError);

        Assert.NotNull(
            sut.EditorErrorMessage);

        Assert.Contains(
            "担当者を登録できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "登録テストエラー",
            sut.EditorErrorMessage);

        Assert.Equal(
            0,
            getAllCallCount);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // Open Edit
    // ============================================

    [Fact]
    public async Task EditCommand_OpensEditorWithExistingValues()
    {
        // Arrange
        var target =
            CreateOperator(
                loginId:
                    "inspector01",

                displayName:
                    "点検担当者A",

                role:
                    OperatorRole.Inspector);


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]));


        await sut.LoadOperatorsAsync();


        var item =
            Assert.Single(
                sut.Operators);


        // Act
        item.EditCommand
            .Execute(null);


        // Assert
        Assert.True(
            sut.IsEditorOpen);

        Assert.False(
            sut.IsCreateMode);

        Assert.Equal(
            target.Id,
            sut.EditingOperatorId);

        Assert.Equal(
            OperatorRole.Inspector,
            sut.OriginalEditingRole);

        Assert.Equal(
            "inspector01",
            sut.EditLoginId);

        Assert.Equal(
            "点検担当者A",
            sut.EditDisplayName);

        Assert.Equal(
            "点検担当者",
            sut.SelectedRoleName);

        Assert.Equal(
            string.Empty,
            sut.EditPassword);

        Assert.Equal(
            string.Empty,
            sut.EditPasswordConfirmation);


        Assert.Equal(
            "担当者の編集",
            sut.EditorTitle);

        Assert.Equal(
            "保存",
            sut.SaveButtonText);

        Assert.Equal(
            "新しいパスワード",
            sut.PasswordLabel);

        Assert.Equal(
            "変更しない場合は空欄",
            sut.PasswordWatermark);
    }


    // ============================================
    // Update
    // ============================================

    [Fact]
    public async Task SaveOperatorCommand_UpdateWithoutPassword_PassesNullPasswordAndReloads()
    {
        // Arrange
        var target =
            CreateOperator();


        var getAllCallCount =
            0;

        var updateCallCount =
            0;

        Guid capturedId =
            Guid.Empty;

        string? capturedLoginId =
            null;

        string? capturedDisplayName =
            null;

        string? capturedPassword =
            "not-null";


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]);
                    },

                updateAsync:
                    (
                        id,
                        loginId,
                        displayName,
                        _,
                        password) =>
                    {
                        updateCallCount++;

                        capturedId =
                            id;

                        capturedLoginId =
                            loginId;

                        capturedDisplayName =
                            displayName;

                        capturedPassword =
                            password;


                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        sut.Operators[0]
            .EditCommand
            .Execute(null);


        sut.EditLoginId =
            "  inspector-new  ";

        sut.EditDisplayName =
            "  新しい表示名  ";

        sut.EditPassword =
            string.Empty;

        sut.EditPasswordConfirmation =
            string.Empty;


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            updateCallCount);

        Assert.Equal(
            target.Id,
            capturedId);

        Assert.Equal(
            "inspector-new",
            capturedLoginId);

        Assert.Equal(
            "新しい表示名",
            capturedDisplayName);

        Assert.Null(
            capturedPassword);


        Assert.Equal(
            "担当者情報を更新しました。",
            sut.OperationMessage);

        Assert.False(
            sut.IsEditorOpen);

        /*
         * 最初のLoad + 更新後Load
         */
        Assert.Equal(
            2,
            getAllCallCount);
    }


    [Fact]
    public async Task SaveOperatorCommand_UpdateWithPassword_PassesNewPassword()
    {
        // Arrange
        var target =
            CreateOperator();


        string? capturedPassword =
            null;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]),

                updateAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        password) =>
                    {
                        capturedPassword =
                            password;

                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        sut.Operators[0]
            .EditCommand
            .Execute(null);


        sut.EditPassword =
            "NewPassword1";

        sut.EditPasswordConfirmation =
            "NewPassword1";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "NewPassword1",
            capturedPassword);
    }


    [Fact]
    public async Task SaveOperatorCommand_CurrentUserCannotChangeOwnRole()
    {
        // Arrange
        var current =
            CreateOperator(
                role:
                    OperatorRole.Inspector);


        var updateCallCount =
            0;

        var getAllCallCount =
            0;


        var sut =
            CreateViewModel(
                currentOperatorId:
                    current.Id,

                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            current
                        ]);
                    },

                updateAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _) =>
                    {
                        updateCallCount++;

                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        sut.Operators[0]
            .EditCommand
            .Execute(null);


        sut.SelectedRoleName =
            "保全責任者";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            updateCallCount);

        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasEditorError);

        Assert.NotNull(
            sut.EditorErrorMessage);

        Assert.Contains(
            "担当者情報を更新できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "現在ログイン中の担当者は、自分自身の権限を変更できません。",
            sut.EditorErrorMessage);

        /*
         * 保存成功していないので
         * 再ロードなし。
         */
        Assert.Equal(
            1,
            getAllCallCount);
    }


    [Fact]
    public async Task SaveOperatorCommand_OtherUserCanChangeRole()
    {
        // Arrange
        var target =
            CreateOperator(
                role:
                    OperatorRole.Inspector);


        OperatorRole?
            capturedRole =
                null;


        var sut =
            CreateViewModel(
                currentOperatorId:
                    DefaultCurrentOperatorId,

                getAllAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]),

                updateAsync:
                    (
                        _,
                        _,
                        _,
                        role,
                        _) =>
                    {
                        capturedRole =
                            role;

                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        sut.Operators[0]
            .EditCommand
            .Execute(null);


        sut.SelectedRoleName =
            "保全責任者";


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            OperatorRole.MaintenanceManager,
            capturedRole);

        Assert.Equal(
            "担当者情報を更新しました。",
            sut.OperationMessage);
    }


    [Fact]
    public async Task SaveOperatorCommand_WhenUpdateFails_SetsEditorError()
    {
        // Arrange
        var target =
            CreateOperator();


        var getAllCallCount =
            0;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]);
                    },

                updateAsync:
                    (
                        _,
                        _,
                        _,
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "更新テストエラー")));


        await sut.LoadOperatorsAsync();


        sut.Operators[0]
            .EditCommand
            .Execute(null);


        // Act
        await sut.SaveOperatorCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.IsEditorOpen);

        Assert.True(
            sut.HasEditorError);

        Assert.NotNull(
            sut.EditorErrorMessage);

        Assert.Contains(
            "担当者情報を更新できませんでした。",
            sut.EditorErrorMessage);

        Assert.Contains(
            "更新テストエラー",
            sut.EditorErrorMessage);

        Assert.Equal(
            1,
            getAllCallCount);

        Assert.False(
            sut.IsSaving);
    }


    // ============================================
    // Toggle Active
    // ============================================

    [Theory]
    [InlineData(
        true,
        false,
        "担当者を無効化しました。")]
    [InlineData(
        false,
        true,
        "担当者を有効化しました。")]
    public async Task ToggleActiveCommand_ChangesStateAndReloads(
        bool initialIsActive,
        bool expectedNewState,
        string expectedMessage)
    {
        // Arrange
        var target =
            CreateOperator(
                isActive:
                    initialIsActive);


        var getAllCallCount =
            0;

        var setActiveCallCount =
            0;

        Guid capturedId =
            Guid.Empty;

        bool? capturedState =
            null;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]);
                    },

                setActiveAsync:
                    (
                        id,
                        isActive) =>
                    {
                        setActiveCallCount++;

                        capturedId =
                            id;

                        capturedState =
                            isActive;


                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        var item =
            Assert.Single(
                sut.Operators);


        Assert.True(
            item.ToggleActiveCommand
                .CanExecute(null));


        // Act
        await item.ToggleActiveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            setActiveCallCount);

        Assert.Equal(
            target.Id,
            capturedId);

        Assert.Equal(
            expectedNewState,
            capturedState);

        Assert.Equal(
            expectedMessage,
            sut.OperationMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsSaving);

        /*
         * 最初のLoad + 状態変更後Load
         */
        Assert.Equal(
            2,
            getAllCallCount);
    }


    [Fact]
    public async Task ToggleActiveCommand_WhenSetActiveFails_SetsError()
    {
        // Arrange
        var target =
            CreateOperator();


        var getAllCallCount =
            0;


        var sut =
            CreateViewModel(
                getAllAsync:
                    () =>
                    {
                        getAllCallCount++;

                        return Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            target
                        ]);
                    },

                setActiveAsync:
                    (
                        _,
                        _) =>
                        Task.FromException(
                            new InvalidOperationException(
                                "状態変更テストエラー")));


        await sut.LoadOperatorsAsync();


        var item =
            Assert.Single(
                sut.Operators);


        // Act
        await item.ToggleActiveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.True(
            sut.HasError);

        Assert.NotNull(
            sut.ErrorMessage);

        Assert.Contains(
            "担当者の状態を変更できませんでした。",
            sut.ErrorMessage);

        Assert.Contains(
            "状態変更テストエラー",
            sut.ErrorMessage);

        Assert.False(
            sut.IsSaving);

        /*
         * 失敗したため再ロードなし。
         */
        Assert.Equal(
            1,
            getAllCallCount);
    }


    [Fact]
    public async Task CurrentUserItem_CannotExecuteToggleActiveCommand()
    {
        // Arrange
        var current =
            CreateOperator(
                isActive:
                    true);


        var setActiveCallCount =
            0;


        var sut =
            CreateViewModel(
                currentOperatorId:
                    current.Id,

                getAllAsync:
                    () =>
                        Task.FromResult<
                            IReadOnlyList<Operator>>(
                        [
                            current
                        ]),

                setActiveAsync:
                    (
                        _,
                        _) =>
                    {
                        setActiveCallCount++;

                        return Task.CompletedTask;
                    });


        await sut.LoadOperatorsAsync();


        var item =
            Assert.Single(
                sut.Operators);


        // Assert
        Assert.True(
            item.IsCurrentUser);

        Assert.False(
            item.CanToggleActive);

        Assert.Equal(
            "ログイン中",
            item.ToggleActiveText);

        Assert.False(
            item.ToggleActiveCommand
                .CanExecute(null));

        Assert.Equal(
            0,
            setActiveCallCount);
    }


    // ============================================
    // Helpers
    // ============================================
    private static OperatorManagementViewModel CreateViewModel(
        Guid? currentOperatorId = null,
        Func<Task<IReadOnlyList<Operator>>>? getAllAsync = null,
        Func<string, string, OperatorRole, string, Task>? createAsync = null,
        Func<Guid, string, string, OperatorRole, string?, Task>? updateAsync = null,
        Func<Guid, bool, Task>? setActiveAsync = null)
    {
        getAllAsync ??=
            EmptyOperators;

        createAsync ??=
            (
                _,
                _,
                _,
                _) =>
                Task.CompletedTask;

        updateAsync ??=
            (
                _,
                _,
                _,
                _,
                _) =>
                Task.CompletedTask;

        setActiveAsync ??=
            (
                _,
                _) =>
                Task.CompletedTask;

        return new OperatorManagementViewModel(
            currentOperatorId ??
                DefaultCurrentOperatorId,
            getAllAsync,
            createAsync,
            updateAsync,
            setActiveAsync);
    }


    private static Task<
        IReadOnlyList<Operator>>
        EmptyOperators()
    {
        return Task.FromResult<
            IReadOnlyList<Operator>>(
            []);
    }


    private static Operator
        CreateOperator(
            string loginId =
                "inspector01",
            string displayName =
                "点検担当者A",
            OperatorRole role =
                OperatorRole.Inspector,
            bool isActive =
                true,
            DateTimeOffset? lastLoginAt =
                null)
    {
        return new Operator
        {
            LoginId =
                loginId,

            NormalizedLoginId =
                loginId
                    .Trim()
                    .ToUpperInvariant(),

            DisplayName =
                displayName,

            PasswordHash =
                "TEST_HASH",

            Role =
                role,

            IsActive =
                isActive,

            LastLoginAt =
                lastLoginAt
        };
    }


    private static void
        PrepareValidCreateEditor(
            OperatorManagementViewModel sut)
    {
        sut.OpenCreateEditorCommand
            .Execute(null);


        sut.EditLoginId =
            "inspector02";

        sut.EditDisplayName =
            "点検担当者B";

        sut.SelectedRoleName =
            "点検担当者";

        sut.EditPassword =
            "Password1";

        sut.EditPasswordConfirmation =
            "Password1";
    }
}