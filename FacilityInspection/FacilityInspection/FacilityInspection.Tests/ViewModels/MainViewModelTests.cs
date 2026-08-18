using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using FacilityInspection.ViewModels;
using System;
using System.Collections.Generic;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class MainViewModelTests
{
    // ============================================
    // Constructor
    // ============================================

    [Fact]
    public void Constructor_WithNullSignIn_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MainViewModel(
                        signIn:
                            null!,

                        signOut:
                            () =>
                            {
                            },

                        createLoginViewModel:
                            _ =>
                                new StubViewModel(
                                    "Login"),

                        createMemberShellViewModel:
                            (_, _) =>
                                new StubViewModel(
                                    "Member"),

                        createAdminShellViewModel:
                            (_, _, _) =>
                                new StubViewModel(
                                    "Admin")));


        // Assert
        Assert.Equal(
            "signIn",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullLoginFactory_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new MainViewModel(
                        signIn:
                            _ =>
                            {
                            },

                        signOut:
                            () =>
                            {
                            },

                        createLoginViewModel:
                            null!,

                        createMemberShellViewModel:
                            (_, _) =>
                                new StubViewModel(
                                    "Member"),

                        createAdminShellViewModel:
                            (_, _, _) =>
                                new StubViewModel(
                                    "Admin")));


        // Assert
        Assert.Equal(
            "createLoginViewModel",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_CreatesInitialLoginPageAndPassesLoginCallback()
    {
        // Arrange & Act
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Assert
        Assert.Equal(
            1,
            recorder.LoginFactoryCallCount);

        Assert.NotNull(
            recorder.LoginSucceeded);

        Assert.NotNull(
            recorder.LatestLoginPage);

        Assert.Same(
            recorder.LatestLoginPage,
            sut.CurrentPage);

        Assert.Equal(
            0,
            recorder.MemberFactoryCallCount);

        Assert.Equal(
            0,
            recorder.AdminFactoryCallCount);
    }


    // ============================================
    // Navigate
    // ============================================

    [Fact]
    public void NavigateTo_ChangesCurrentPage()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var destination =
            new StubViewModel(
                "Destination");


        // Act
        sut.NavigateTo(
            destination);


        // Assert
        Assert.Same(
            destination,
            sut.CurrentPage);
    }


    [Fact]
    public void NavigateTo_WithNullDestination_ThrowsArgumentNullException()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    sut.NavigateTo(
                        null!));


        // Assert
        Assert.Equal(
            "destination",
            exception.ParamName);
    }


    // ============================================
    // Login
    // ============================================

    [Fact]
    public void LoginSucceeded_WithNullUser_ThrowsArgumentNullException()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        _ =
            recorder.CreateViewModel();


        Assert.NotNull(
            recorder.LoginSucceeded);


        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    recorder.LoginSucceeded!(
                        null!));


        // Assert
        Assert.Equal(
            "signedInOperator",
            exception.ParamName);

        Assert.Equal(
            0,
            recorder.SignInCallCount);

        Assert.Equal(
            0,
            recorder.MemberFactoryCallCount);

        Assert.Equal(
            0,
            recorder.AdminFactoryCallCount);
    }


    // ============================================
    // Inspector
    // ============================================

    [Fact]
    public void LoginSucceeded_AsInspector_SignsInAndNavigatesToMemberShell()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var user =
            CreateSignedInOperator(
                OperatorRole.Inspector);


        // Act
        recorder.LoginSucceeded!(
            user);


        // Assert
        Assert.Equal(
            1,
            recorder.SignInCallCount);

        Assert.Same(
            user,
            recorder.SignedInUser);

        Assert.Equal(
            1,
            recorder.MemberFactoryCallCount);

        Assert.Equal(
            0,
            recorder.AdminFactoryCallCount);

        Assert.Same(
            user,
            recorder.MemberUser);

        Assert.NotNull(
            recorder.MemberLogout);

        Assert.NotNull(
            recorder.LatestMemberPage);

        Assert.Same(
            recorder.LatestMemberPage,
            sut.CurrentPage);
    }


    [Fact]
    public void LoginSucceeded_AsInspector_SignsInBeforeCreatingMemberShell()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        _ =
            recorder.CreateViewModel();

        recorder.CallOrder.Clear();


        var user =
            CreateSignedInOperator(
                OperatorRole.Inspector);


        // Act
        recorder.LoginSucceeded!(
            user);


        // Assert
        Assert.Collection(
            recorder.CallOrder,

            item =>
                Assert.Equal(
                    "SignIn",
                    item),

            item =>
                Assert.Equal(
                    "MemberFactory",
                    item));
    }


    // ============================================
    // Maintenance Manager
    // ============================================

    [Fact]
    public void LoginSucceeded_AsMaintenanceManager_SignsInAndNavigatesToAdminShell()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var user =
            CreateSignedInOperator(
                OperatorRole.MaintenanceManager);


        // Act
        recorder.LoginSucceeded!(
            user);


        // Assert
        Assert.Equal(
            1,
            recorder.SignInCallCount);

        Assert.Same(
            user,
            recorder.SignedInUser);

        Assert.Equal(
            0,
            recorder.MemberFactoryCallCount);

        Assert.Equal(
            1,
            recorder.AdminFactoryCallCount);

        Assert.Same(
            user,
            recorder.AdminUser);

        Assert.NotNull(
            recorder.AdminLogout);

        Assert.NotNull(
            recorder.RestoreCompleted);

        Assert.NotNull(
            recorder.LatestAdminPage);

        Assert.Same(
            recorder.LatestAdminPage,
            sut.CurrentPage);
    }


    // ============================================
    // Unsupported Role
    // ============================================

    [Fact]
    public void LoginSucceeded_WithUnsupportedRole_ThrowsInvalidOperationException()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var loginPage =
            sut.CurrentPage;


        var unsupportedRole =
            (OperatorRole)999;

        var user =
            CreateSignedInOperator(
                unsupportedRole);


        // Act
        var exception =
            Assert.Throws<
                InvalidOperationException>(
                () =>
                    recorder.LoginSucceeded!(
                        user));


        // Assert
        Assert.Equal(
            $"未対応の権限です: {unsupportedRole}",
            exception.Message);


        /*
         * 現在の本体仕様では
         * Role判定より先にSignInする。
         */
        Assert.Equal(
            1,
            recorder.SignInCallCount);

        Assert.Equal(
            0,
            recorder.MemberFactoryCallCount);

        Assert.Equal(
            0,
            recorder.AdminFactoryCallCount);


        /*
         * NavigateToまでは到達していないため
         * Login画面のまま。
         */
        Assert.Same(
            loginPage,
            sut.CurrentPage);
    }


    // ============================================
    // Logout
    // ============================================

    [Fact]
    public void Logout_SignsOutAndCreatesNewLoginPage()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var firstLoginPage =
            sut.CurrentPage;


        Assert.Equal(
            1,
            recorder.LoginFactoryCallCount);


        // Act
        sut.Logout();


        // Assert
        Assert.Equal(
            1,
            recorder.SignOutCallCount);

        Assert.Equal(
            2,
            recorder.LoginFactoryCallCount);

        Assert.NotNull(
            recorder.LatestLoginPage);

        Assert.NotSame(
            firstLoginPage,
            sut.CurrentPage);

        Assert.Same(
            recorder.LatestLoginPage,
            sut.CurrentPage);
    }


    [Fact]
    public void MemberShellLogoutCallback_SignsOutAndReturnsToLogin()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        recorder.LoginSucceeded!(
            CreateSignedInOperator(
                OperatorRole.Inspector));


        Assert.Same(
            recorder.LatestMemberPage,
            sut.CurrentPage);

        Assert.NotNull(
            recorder.MemberLogout);


        // Act
        recorder.MemberLogout!();


        // Assert
        Assert.Equal(
            1,
            recorder.SignOutCallCount);

        Assert.Equal(
            2,
            recorder.LoginFactoryCallCount);

        Assert.Same(
            recorder.LatestLoginPage,
            sut.CurrentPage);
    }


    [Fact]
    public void Logout_NewLoginPageCanLoginAgain()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        var firstUser =
            CreateSignedInOperator(
                OperatorRole.Inspector);


        recorder.LoginSucceeded!(
            firstUser);


        Assert.Equal(
            1,
            recorder.MemberFactoryCallCount);


        // Act - Logout
        sut.Logout();


        var secondLoginSucceeded =
            recorder.LoginSucceeded;


        Assert.NotNull(
            secondLoginSucceeded);


        var secondUser =
            new SignedInOperator(
                Guid.Parse(
                    "AAAAAAAA-BBBB-CCCC-DDDD-EEEEEEEEEEEE"),
                "inspector02",
                "点検担当者B",
                OperatorRole.Inspector);


        // Act - Login again
        secondLoginSucceeded!(
            secondUser);


        // Assert
        Assert.Equal(
            2,
            recorder.SignInCallCount);

        Assert.Equal(
            2,
            recorder.MemberFactoryCallCount);

        Assert.Same(
            secondUser,
            recorder.MemberUser);

        Assert.Same(
            recorder.LatestMemberPage,
            sut.CurrentPage);
    }


    // ============================================
    // Restore
    // ============================================

    [Fact]
    public void AdminRestoreCompleted_RecreatesAdminShellWithoutSigningInAgain()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();

        var user =
            CreateSignedInOperator(
                OperatorRole.MaintenanceManager);


        recorder.LoginSucceeded!(
            user);


        Assert.Equal(
            1,
            recorder.SignInCallCount);

        Assert.Equal(
            1,
            recorder.AdminFactoryCallCount);


        var firstAdminPage =
            sut.CurrentPage;

        var firstRestoreCompleted =
            recorder.RestoreCompleted;


        Assert.NotNull(
            firstRestoreCompleted);


        // Act
        firstRestoreCompleted!();


        // Assert
        Assert.Equal(
            2,
            recorder.AdminFactoryCallCount);


        /*
         * DB復元は再ログインではないので
         * SignInは再実行しない。
         */
        Assert.Equal(
            1,
            recorder.SignInCallCount);

        Assert.NotSame(
            firstAdminPage,
            sut.CurrentPage);

        Assert.Same(
            recorder.LatestAdminPage,
            sut.CurrentPage);

        Assert.Same(
            user,
            recorder.AdminUser);


        /*
         * 新しいAdminShell用の
         * RestoreCompleted callbackも再設定される。
         */
        Assert.NotNull(
            recorder.RestoreCompleted);
    }


    // ============================================
    // Admin Logout
    // ============================================

    [Fact]
    public void AdminShellLogoutCallback_SignsOutAndReturnsToLogin()
    {
        // Arrange
        var recorder =
            new FactoryRecorder();

        var sut =
            recorder.CreateViewModel();


        recorder.LoginSucceeded!(
            CreateSignedInOperator(
                OperatorRole.MaintenanceManager));


        Assert.Same(
            recorder.LatestAdminPage,
            sut.CurrentPage);

        Assert.NotNull(
            recorder.AdminLogout);


        // Act
        recorder.AdminLogout!();


        // Assert
        Assert.Equal(
            1,
            recorder.SignOutCallCount);

        Assert.Equal(
            2,
            recorder.LoginFactoryCallCount);

        Assert.Same(
            recorder.LatestLoginPage,
            sut.CurrentPage);
    }


    // ============================================
    // Helpers
    // ============================================

    private static SignedInOperator
        CreateSignedInOperator(
            OperatorRole role)
    {
        return new SignedInOperator(
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555"),
            "test-user",
            "テストユーザー",
            role);
    }


    // ============================================
    // Test ViewModel
    // ============================================

    private sealed class StubViewModel
        : ViewModelBase
    {
        public StubViewModel(
            string name)
        {
            Name =
                name;
        }


        public string Name
        {
            get;
        }
    }


    // ============================================
    // Factory Recorder
    // ============================================

    private sealed class FactoryRecorder
    {
        private int
            _loginPageNumber;

        private int
            _memberPageNumber;

        private int
            _adminPageNumber;


        // ----------------------------------------
        // Calls
        // ----------------------------------------

        public int SignInCallCount
        {
            get;
            private set;
        }


        public int SignOutCallCount
        {
            get;
            private set;
        }


        public int LoginFactoryCallCount
        {
            get;
            private set;
        }


        public int MemberFactoryCallCount
        {
            get;
            private set;
        }


        public int AdminFactoryCallCount
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Arguments
        // ----------------------------------------

        public SignedInOperator?
            SignedInUser
        {
            get;
            private set;
        }


        public SignedInOperator?
            MemberUser
        {
            get;
            private set;
        }


        public SignedInOperator?
            AdminUser
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Callbacks
        // ----------------------------------------

        public Action<SignedInOperator>?
            LoginSucceeded
        {
            get;
            private set;
        }


        public Action?
            MemberLogout
        {
            get;
            private set;
        }


        public Action?
            AdminLogout
        {
            get;
            private set;
        }


        public Action?
            RestoreCompleted
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Pages
        // ----------------------------------------

        public StubViewModel?
            LatestLoginPage
        {
            get;
            private set;
        }


        public StubViewModel?
            LatestMemberPage
        {
            get;
            private set;
        }


        public StubViewModel?
            LatestAdminPage
        {
            get;
            private set;
        }


        // ----------------------------------------
        // Order
        // ----------------------------------------

        public List<string>
            CallOrder
        {
            get;
        } = [];


        // ----------------------------------------
        // Create MainViewModel
        // ----------------------------------------

        public MainViewModel
            CreateViewModel()
        {
            return new MainViewModel(
                signIn:
                    user =>
                    {
                        SignInCallCount++;

                        SignedInUser =
                            user;

                        CallOrder.Add(
                            "SignIn");
                    },

                signOut:
                    () =>
                    {
                        SignOutCallCount++;

                        CallOrder.Add(
                            "SignOut");
                    },

                createLoginViewModel:
                    loginSucceeded =>
                    {
                        LoginFactoryCallCount++;

                        LoginSucceeded =
                            loginSucceeded;

                        _loginPageNumber++;


                        LatestLoginPage =
                            new StubViewModel(
                                $"Login-{_loginPageNumber}");


                        return LatestLoginPage;
                    },

                createMemberShellViewModel:
                    (
                        user,
                        logout) =>
                    {
                        MemberFactoryCallCount++;

                        MemberUser =
                            user;

                        MemberLogout =
                            logout;

                        CallOrder.Add(
                            "MemberFactory");

                        _memberPageNumber++;


                        LatestMemberPage =
                            new StubViewModel(
                                $"Member-{_memberPageNumber}");


                        return LatestMemberPage;
                    },

                createAdminShellViewModel:
                    (
                        user,
                        logout,
                        restoreCompleted) =>
                    {
                        AdminFactoryCallCount++;

                        AdminUser =
                            user;

                        AdminLogout =
                            logout;

                        RestoreCompleted =
                            restoreCompleted;

                        CallOrder.Add(
                            "AdminFactory");

                        _adminPageNumber++;


                        LatestAdminPage =
                            new StubViewModel(
                                $"Admin-{_adminPageNumber}");


                        return LatestAdminPage;
                    });
        }
    }
}