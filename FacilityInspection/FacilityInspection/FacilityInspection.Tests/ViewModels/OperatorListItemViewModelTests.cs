using FacilityInspection.Domain.Operators;
using FacilityInspection.ViewModels;
using System;
using System.Threading.Tasks;
using Xunit;

namespace FacilityInspection.Tests.ViewModels;

public sealed class OperatorListItemViewModelTests
{
    private static readonly Guid
        OperatorId =
            Guid.Parse(
                "11111111-2222-3333-4444-555555555555");


    // ============================================
    // Constructor Validation
    // ============================================

    [Fact]
    public void Constructor_WithNullLoginId_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            null!,

                        displayName:
                            "点検担当者A",

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            _ =>
                            {
                            },

                        toggleActiveRequested:
                            _ =>
                                Task.CompletedTask));


        // Assert
        Assert.Equal(
            "loginId",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankLoginId_ThrowsArgumentException(
        string loginId)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            loginId,

                        displayName:
                            "点検担当者A",

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            _ =>
                            {
                            },

                        toggleActiveRequested:
                            _ =>
                                Task.CompletedTask));


        // Assert
        Assert.Equal(
            "loginId",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullDisplayName_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<ArgumentNullException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            "inspector01",

                        displayName:
                            null!,

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            _ =>
                            {
                            },

                        toggleActiveRequested:
                            _ =>
                                Task.CompletedTask));


        // Assert
        Assert.Equal(
            "displayName",
            exception.ParamName);
    }


    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_WithBlankDisplayName_ThrowsArgumentException(
        string displayName)
    {
        // Act
        var exception =
            Assert.Throws<ArgumentException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            "inspector01",

                        displayName:
                            displayName,

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            _ =>
                            {
                            },

                        toggleActiveRequested:
                            _ =>
                                Task.CompletedTask));


        // Assert
        Assert.Equal(
            "displayName",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullEditRequested_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            "inspector01",

                        displayName:
                            "点検担当者A",

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            null!,

                        toggleActiveRequested:
                            _ =>
                                Task.CompletedTask));


        // Assert
        Assert.Equal(
            "editRequested",
            exception.ParamName);
    }


    [Fact]
    public void Constructor_WithNullToggleActiveRequested_ThrowsArgumentNullException()
    {
        // Act
        var exception =
            Assert.Throws<
                ArgumentNullException>(
                () =>
                    new OperatorListItemViewModel(
                        id:
                            OperatorId,

                        loginId:
                            "inspector01",

                        displayName:
                            "点検担当者A",

                        role:
                            OperatorRole.Inspector,

                        isActive:
                            true,

                        lastLoginAt:
                            null,

                        isCurrentUser:
                            false,

                        editRequested:
                            _ =>
                            {
                            },

                        toggleActiveRequested:
                            null!));


        // Assert
        Assert.Equal(
            "toggleActiveRequested",
            exception.ParamName);
    }


    // ============================================
    // Properties
    // ============================================

    [Fact]
    public void Constructor_SetsProperties()
    {
        // Arrange
        var lastLoginAt =
            new DateTimeOffset(
                2026,
                8,
                20,
                6,
                30,
                0,
                TimeSpan.FromHours(
                    9));


        // Act
        var sut =
            CreateViewModel(
                loginId:
                    "inspector01",

                displayName:
                    "点検担当者A",

                role:
                    OperatorRole.Inspector,

                isActive:
                    true,

                lastLoginAt:
                    lastLoginAt,

                isCurrentUser:
                    false);


        // Assert
        Assert.Equal(
            OperatorId,
            sut.Id);

        Assert.Equal(
            "inspector01",
            sut.LoginId);

        Assert.Equal(
            "点検担当者A",
            sut.DisplayName);

        Assert.Equal(
            OperatorRole.Inspector,
            sut.Role);

        Assert.True(
            sut.IsActive);

        Assert.Equal(
            lastLoginAt,
            sut.LastLoginAt);

        Assert.False(
            sut.IsCurrentUser);
    }


    // ============================================
    // Role
    // ============================================

    [Theory]
    [InlineData(
        OperatorRole.Inspector,
        "点検担当者")]
    [InlineData(
        OperatorRole.MaintenanceManager,
        "保全責任者")]
    public void RoleName_WithKnownRole_ReturnsExpectedText(
        OperatorRole role,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                role:
                    role);


        // Assert
        Assert.Equal(
            expected,
            sut.RoleName);
    }


    [Fact]
    public void RoleName_WithUnknownRole_ReturnsEnumValueText()
    {
        // Arrange
        var sut =
            CreateViewModel(
                role:
                    (OperatorRole)999);


        // Assert
        Assert.Equal(
            "999",
            sut.RoleName);
    }


    // ============================================
    // Status
    // ============================================

    [Theory]
    [InlineData(
        true,
        "有効")]
    [InlineData(
        false,
        "無効")]
    public void StatusText_ReturnsExpectedText(
        bool isActive,
        string expected)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isActive:
                    isActive);


        // Assert
        Assert.Equal(
            expected,
            sut.StatusText);
    }


    // ============================================
    // Last Login
    // ============================================

    [Fact]
    public void LastLoginAtText_ReturnsExpectedText()
    {
        // Arrange
        var noLogin =
            CreateViewModel(
                lastLoginAt:
                    null);


        var lastLoginAt =
            new DateTimeOffset(
                2026,
                8,
                20,
                6,
                30,
                0,
                TimeSpan.FromHours(
                    9));


        var loggedIn =
            CreateViewModel(
                lastLoginAt:
                    lastLoginAt);


        var expected =
            lastLoginAt
                .ToLocalTime()
                .ToString(
                    "yyyy/MM/dd HH:mm");


        // Assert
        Assert.Equal(
            "未ログイン",
            noLogin.LastLoginAtText);

        Assert.Equal(
            expected,
            loggedIn.LastLoginAtText);
    }


    // ============================================
    // Toggle State
    // ============================================

    [Theory]
    [InlineData(
        true,
        false,
        true,
        "無効化")]
    [InlineData(
        false,
        false,
        true,
        "有効化")]
    [InlineData(
        true,
        true,
        false,
        "ログイン中")]
    [InlineData(
        false,
        true,
        false,
        "ログイン中")]
    public void ToggleActiveState_ReturnsExpectedValues(
        bool isActive,
        bool isCurrentUser,
        bool expectedCanToggle,
        string expectedText)
    {
        // Arrange
        var sut =
            CreateViewModel(
                isActive:
                    isActive,

                isCurrentUser:
                    isCurrentUser);


        // Assert
        Assert.Equal(
            expectedCanToggle,
            sut.CanToggleActive);

        Assert.Equal(
            expectedText,
            sut.ToggleActiveText);

        Assert.Equal(
            expectedCanToggle,
            sut.ToggleActiveCommand
                .CanExecute(null));
    }


    // ============================================
    // Edit Command
    // ============================================

    [Fact]
    public void EditCommand_PassesOwnInstanceToCallback()
    {
        // Arrange
        OperatorListItemViewModel?
            capturedItem =
                null;


        var sut =
            CreateViewModel(
                editRequested:
                    item =>
                        capturedItem =
                            item);


        // Act
        sut.EditCommand
            .Execute(null);


        // Assert
        Assert.Same(
            sut,
            capturedItem);
    }


    // ============================================
    // Toggle Active Command
    // ============================================

    [Fact]
    public async Task ToggleActiveCommand_WhenAllowed_PassesOwnInstanceToCallback()
    {
        // Arrange
        OperatorListItemViewModel?
            capturedItem =
                null;

        var callbackCallCount =
            0;


        var sut =
            CreateViewModel(
                isCurrentUser:
                    false,

                toggleActiveRequested:
                    item =>
                    {
                        callbackCallCount++;

                        capturedItem =
                            item;

                        return Task.CompletedTask;
                    });


        Assert.True(
            sut.ToggleActiveCommand
                .CanExecute(null));


        // Act
        await sut.ToggleActiveCommand
            .ExecuteAsync(null);


        // Assert
        Assert.Equal(
            1,
            callbackCallCount);

        Assert.Same(
            sut,
            capturedItem);
    }


    [Fact]
    public void ToggleActiveCommand_WhenCurrentUser_CannotExecute()
    {
        // Arrange
        var callbackCallCount =
            0;


        var sut =
            CreateViewModel(
                isCurrentUser:
                    true,

                toggleActiveRequested:
                    _ =>
                    {
                        callbackCallCount++;

                        return Task.CompletedTask;
                    });


        // Assert
        Assert.False(
            sut.CanToggleActive);

        Assert.Equal(
            "ログイン中",
            sut.ToggleActiveText);

        Assert.False(
            sut.ToggleActiveCommand
                .CanExecute(null));

        Assert.Equal(
            0,
            callbackCallCount);
    }


    // ============================================
    // Helpers
    // ============================================

    private static OperatorListItemViewModel
        CreateViewModel(
            string loginId =
                "inspector01",
            string displayName =
                "点検担当者A",
            OperatorRole role =
                OperatorRole.Inspector,
            bool isActive =
                true,
            DateTimeOffset? lastLoginAt =
                null,
            bool isCurrentUser =
                false,
            Action<OperatorListItemViewModel>?
                editRequested =
                    null,
            Func<OperatorListItemViewModel, Task>?
                toggleActiveRequested =
                    null)
    {
        return new OperatorListItemViewModel(
            id:
                OperatorId,

            loginId:
                loginId,

            displayName:
                displayName,

            role:
                role,

            isActive:
                isActive,

            lastLoginAt:
                lastLoginAt,

            isCurrentUser:
                isCurrentUser,

            editRequested:
                editRequested ??
                (_ =>
                {
                }),

            toggleActiveRequested:
                toggleActiveRequested ??
                (_ =>
                    Task.CompletedTask));
    }
}