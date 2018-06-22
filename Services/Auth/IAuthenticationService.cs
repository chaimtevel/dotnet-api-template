using Shared.Auth;
using Shared.Models;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAuthenticationService
    {
        Task<IApiUser> Login(BasicAuthentication authentication);
        Task<IApiUser> Login(AuthProviders authProvider, ExternalAuthentication authentication);
    }
}
