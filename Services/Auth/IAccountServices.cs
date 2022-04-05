using Shared.Models.DB.Identity;
using SysClaims = System.Security.Claims;

namespace Services.Auth;

public interface IAccountServices
{
    Task<IEnumerable<SysClaims.Claim>> GetAllClaimsAsync(User user, CancellationToken token);
}