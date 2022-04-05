using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;

namespace Services.Auth;

public interface IAuthenticationService
{
    Task<(bool isAuthenticated, User? user)> LoginAsync(BasicAuthenticationRequest authentication);
    Task<(bool isAuthenticated, User? user)> LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication);
}