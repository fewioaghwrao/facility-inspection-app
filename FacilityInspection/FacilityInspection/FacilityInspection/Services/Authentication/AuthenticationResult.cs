namespace FacilityInspection.Services.Authentication;

public sealed record AuthenticationResult(
    bool Succeeded,
    SignedInOperator? User,
    string ErrorMessage)
{
    public static AuthenticationResult Success(
        SignedInOperator user)
    {
        return new AuthenticationResult(
            true,
            user,
            string.Empty);
    }

    public static AuthenticationResult Failure(
        string errorMessage)
    {
        return new AuthenticationResult(
            false,
            null,
            errorMessage);
    }
}
