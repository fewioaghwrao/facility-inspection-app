using FacilityInspection.Domain.Operators;
using FacilityInspection.Services.Authentication;
using Xunit;

namespace FacilityInspection.Tests.Services.Authentication;

public sealed class CurrentUserSessionTests
{
    [Fact]
    public void Constructor_InitializesAsUnauthenticated()
    {
        // Act
        var session =
            new CurrentUserSession();

        // Assert
        Assert.Null(
            session.CurrentUser);

        Assert.False(
            session.IsAuthenticated);
    }


    [Fact]
    public void SignIn_SetsCurrentUserAndAuthenticates()
    {
        // Arrange
        var session =
            new CurrentUserSession();

        var user =
            new SignedInOperator(
                Guid.NewGuid(),
                "inspector",
                "点検担当者1",
                OperatorRole.Inspector);

        // Act
        session.SignIn(
            user);

        // Assert
        Assert.Same(
            user,
            session.CurrentUser);

        Assert.True(
            session.IsAuthenticated);
    }


    [Fact]
    public void SignOut_ClearsCurrentUserAndUnauthenticates()
    {
        // Arrange
        var session =
            new CurrentUserSession();

        var user =
            new SignedInOperator(
                Guid.NewGuid(),
                "inspector",
                "点検担当者1",
                OperatorRole.Inspector);

        session.SignIn(
            user);

        // Act
        session.SignOut();

        // Assert
        Assert.Null(
            session.CurrentUser);

        Assert.False(
            session.IsAuthenticated);
    }
}