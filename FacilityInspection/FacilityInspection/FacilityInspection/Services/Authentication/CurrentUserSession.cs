namespace FacilityInspection.Services.Authentication;

public sealed class CurrentUserSession
{
    public SignedInOperator? CurrentUser { get; private set; }

    public bool IsAuthenticated => CurrentUser is not null;

    public void SignIn(SignedInOperator user)
    {
        CurrentUser = user;
    }

    public void SignOut()
    {
        CurrentUser = null;
    }
}
