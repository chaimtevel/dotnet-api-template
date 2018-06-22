using Shared.Auth;
using Shared.Models;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAuthenticationService
    {
        Task<User> Login(BasicAuthentication authentication);
        Task<User> Login(AuthProviders authProvider, ExternalAuthentication authentication);
    }
}
