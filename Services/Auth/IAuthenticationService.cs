using Shared.Auth;
using Shared.Models;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAuthenticationService
    {
        Task<(bool isAuthenticated, IApiUser user)> LoginAsync(BasicAuthentication authentication);
        Task<(bool isAuthenticated, IApiUser user)> LoginAsync(AuthProviders authProvider, ExternalAuthentication authentication);
    }
}
