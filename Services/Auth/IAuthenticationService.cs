using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAuthenticationService
    {
        Task<(bool isAuthenticated, User user)> LoginAsync(BasicAuthenticationRequest authentication);
        Task<(bool isAuthenticated, User user)> LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication);
    }
}
