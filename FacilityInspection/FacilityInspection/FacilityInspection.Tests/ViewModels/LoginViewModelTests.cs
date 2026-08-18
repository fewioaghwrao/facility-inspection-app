using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using FacilityInspection.ViewModels;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class LoginViewModelTests
{
    // ============================================
    // Initial State
    // ============================================

    [Fact]
    public void Constructor_InitializesExpectedState()
    {
        // Arrange & Act
        var sut =
            CreateViewModel();


        // Assert
        Assert.Equal(
            string.Empty,
            sut.LoginId);

        Assert.Equal(
            string.Empty,
            sut.Password);

        Assert.False(
            sut.IsPasswordVisible);

        Assert.Equal(
            "表示",
            sut.PasswordToggleLabel);

        Assert.Equal(
            string.Empty,
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsBusy);

        Assert.False(
            sut.LoginCommand
                .CanExecute(null));
    }


    // ============================================
    // CanExecute
    // ============================================

    [Fact]
    public void LoginCommand_WhenCredentialsAreEntered_CanExecute()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        // Assert
        Assert.True(
            sut.LoginCommand
                .CanExecute(null));
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LoginCommand_WhenLoginIdIsBlank_CannotExecute(
        string loginId)
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.LoginId =
            loginId;

        sut.Password =
            "Password123!";


        // Assert
        Assert.False(
            sut.LoginCommand
                .CanExecute(null));
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void LoginCommand_WhenPasswordIsBlank_CannotExecute(
        string password)
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.LoginId =
            "inspector01";

        sut.Password =
            password;


        // Assert
        Assert.False(
            sut.LoginCommand
                .CanExecute(null));
    }


    // ============================================
    // Password Display
    // ============================================

    [Theory]
    [InlineData(
        false,
        "表示")]
    [InlineData(
        true,
        "隠す")]
    public void PasswordToggleLabel_ReturnsExpectedText(
        bool isPasswordVisible,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.IsPasswordVisible =
            isPasswordVisible;


        // Assert
        Assert.Equal(
            expected,
            sut.PasswordToggleLabel);
    }


    // ============================================
    // Error
    // ============================================

    [Theory]
    [InlineData(
        "",
        false)]
    [InlineData(
        "   ",
        false)]
    [InlineData(
        "ログインエラー",
        true)]
    public void HasError_ReturnsExpectedValue(
        string errorMessage,
        bool expected)
    {
        // Arrange
        var sut =
            CreateViewModel();


        // Act
        sut.ErrorMessage =
            errorMessage;


        // Assert
        Assert.Equal(
            expected,
            sut.HasError);
    }


    // ============================================
    // Authentication Arguments
    // ============================================

    [Fact]
    public async Task LoginCommand_PassesCredentialsToAuthenticationService()
    {
        // Arrange
        string?
            capturedLoginId =
                null;

        string?
            capturedPassword =
                null;


        var service =
            new FakeAuthenticationService(
                (
                    loginId,
                    password,
                    _) =>
                {
                    capturedLoginId =
                        loginId;

                    capturedPassword =
                        password;

                    return Task.FromResult(
                        AuthenticationResult.Failure(
                            "認証失敗"));
                });


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        // Act
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "inspector01",
            capturedLoginId);

        Assert.Equal(
            "Password123!",
            capturedPassword);

        Assert.Equal(
            1,
            service.CallCount);
    }


    // ============================================
    // Authentication Failure
    // ============================================

    [Fact]
    public async Task LoginCommand_WhenAuthenticationFails_SetsErrorAndDoesNotRaiseLoginSucceeded()
    {
        // Arrange
        var service =
            CreateAuthenticationService(
                AuthenticationResult.Failure(
                    "ログインIDまたはパスワードが正しくありません。"));


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "WrongPassword";

        sut.IsPasswordVisible =
            true;


        var succeededCallCount =
            0;

        sut.LoginSucceeded =
            _ =>
                succeededCallCount++;


        // Act
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            0,
            succeededCallCount);

        Assert.Equal(
            "ログインIDまたはパスワードが正しくありません。",
            sut.ErrorMessage);

        Assert.True(
            sut.HasError);

        /*
         * 認証失敗時はPasswordを消さない。
         */
        Assert.Equal(
            "WrongPassword",
            sut.Password);

        Assert.True(
            sut.IsPasswordVisible);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Authentication Success
    // ============================================

    [Fact]
    public async Task LoginCommand_WhenAuthenticationSucceeds_RaisesLoginSucceededAndResetsPasswordState()
    {
        // Arrange
        var signedInOperator =
            CreateSignedInOperator();


        var service =
            CreateAuthenticationService(
                AuthenticationResult.Success(
                    signedInOperator));


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";

        sut.IsPasswordVisible =
            true;


        SignedInOperator?
            notifiedUser =
                null;


        sut.LoginSucceeded =
            user =>
                notifiedUser =
                    user;


        // Act
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Same(
            signedInOperator,
            notifiedUser);

        Assert.Equal(
            string.Empty,
            sut.Password);

        Assert.False(
            sut.IsPasswordVisible);

        Assert.Equal(
            "表示",
            sut.PasswordToggleLabel);

        Assert.Equal(
            string.Empty,
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Busy
    // ============================================

    [Fact]
    public async Task LoginCommand_WhileAuthenticating_SetsBusyAndDisablesCommand()
    {
        // Arrange
        var completionSource =
            new TaskCompletionSource<
                AuthenticationResult>(
                TaskCreationOptions
                    .RunContinuationsAsynchronously);


        var service =
            new FakeAuthenticationService(
                (_, _, _) =>
                    completionSource.Task);


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        // Act
        var loginTask =
            sut.LoginCommand
                .ExecuteAsync(null);


        // Assert - processing
        Assert.True(
            sut.IsBusy);

        Assert.False(
            sut.LoginCommand
                .CanExecute(null));


        // Complete
        completionSource.SetResult(
            AuthenticationResult.Failure(
                "認証失敗"));


        await loginTask;


        // Assert - completed
        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Exception
    // ============================================

    [Fact]
    public async Task LoginCommand_WhenAuthenticationServiceThrows_SetsGeneralError()
    {
        // Arrange
        var service =
            new FakeAuthenticationService(
                (_, _, _) =>
                    Task.FromException<
                        AuthenticationResult>(
                        new InvalidOperationException(
                            "DB接続エラー")));


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        var succeededCallCount =
            0;

        sut.LoginSucceeded =
            _ =>
                succeededCallCount++;


        // Act
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            "ログイン処理中にエラーが発生しました。",
            sut.ErrorMessage);

        Assert.True(
            sut.HasError);

        Assert.False(
            sut.IsBusy);

        Assert.Equal(
            0,
            succeededCallCount);
    }


    // ============================================
    // Retry
    // ============================================

    [Fact]
    public async Task LoginCommand_AfterFailureThenSuccess_ClearsPreviousError()
    {
        // Arrange
        var signedInOperator =
            CreateSignedInOperator();


        var callCount =
            0;


        var service =
            new FakeAuthenticationService(
                (_, _, _) =>
                {
                    callCount++;


                    if (callCount == 1)
                    {
                        return Task.FromResult(
                            AuthenticationResult.Failure(
                                "1回目の認証失敗"));
                    }


                    return Task.FromResult(
                        AuthenticationResult.Success(
                            signedInOperator));
                });


        var sut =
            new LoginViewModel(
                service);

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        // Act - first
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert - failure
        Assert.True(
            sut.HasError);

        Assert.Equal(
            "1回目の認証失敗",
            sut.ErrorMessage);


        /*
         * 失敗時にはPasswordが保持されるため
         * そのまま再試行可能。
         */
        Assert.True(
            sut.LoginCommand
                .CanExecute(null));


        // Act - retry
        await sut.LoginCommand
            .ExecuteAsync(null);


        // Assert - success
        Assert.Equal(
            2,
            callCount);

        Assert.Equal(
            string.Empty,
            sut.ErrorMessage);

        Assert.False(
            sut.HasError);

        Assert.Equal(
            string.Empty,
            sut.Password);

        Assert.False(
            sut.IsBusy);
    }


    // ============================================
    // Busy CanExecute
    // ============================================

    [Fact]
    public void LoginCommand_WhenBusy_CannotExecute()
    {
        // Arrange
        var sut =
            CreateViewModel();

        sut.LoginId =
            "inspector01";

        sut.Password =
            "Password123!";


        Assert.True(
            sut.LoginCommand
                .CanExecute(null));


        // Act
        sut.IsBusy =
            true;


        // Assert
        Assert.False(
            sut.LoginCommand
                .CanExecute(null));
    }


    // ============================================
    // Helpers
    // ============================================

    private static LoginViewModel
        CreateViewModel()
    {
        return new LoginViewModel(
            CreateAuthenticationService(
                AuthenticationResult.Failure(
                    "認証失敗")));
    }


    private static FakeAuthenticationService
        CreateAuthenticationService(
            AuthenticationResult result)
    {
        return new FakeAuthenticationService(
            (_, _, _) =>
                Task.FromResult(
                    result));
    }


    private static SignedInOperator
        CreateSignedInOperator()
    {
        return new SignedInOperator(
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555"),
            "inspector01",
            "点検担当者A",
            OperatorRole.Inspector);
    }


    // ============================================
    // Fake Authentication Service
    // ============================================

    private sealed class FakeAuthenticationService
        : IAuthenticationService
    {
        private readonly Func<
            string,
            string,
            CancellationToken,
            Task<AuthenticationResult>>
            _signInAsync;


        public FakeAuthenticationService(
            Func<
                string,
                string,
                CancellationToken,
                Task<AuthenticationResult>>
                signInAsync)
        {
            ArgumentNullException.ThrowIfNull(
                signInAsync);

            _signInAsync =
                signInAsync;
        }


        public int CallCount
        {
            get;
            private set;
        }


        public Task<AuthenticationResult>
            SignInAsync(
                string loginId,
                string password,
                CancellationToken cancellationToken = default)
        {
            CallCount++;

            return _signInAsync(
                loginId,
                password,
                cancellationToken);
        }
    }
}