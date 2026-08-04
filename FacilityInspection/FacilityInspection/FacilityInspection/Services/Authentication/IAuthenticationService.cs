using System.Threading;
using System.Threading.Tasks;

namespace FacilityInspection.Services.Authentication;

public interface IAuthenticationService
{
    Task<AuthenticationResult> SignInAsync(
        string loginId,
        string password,
        CancellationToken cancellationToken = default);
}