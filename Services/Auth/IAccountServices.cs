using Shared.Models.DB.Identity;
using System.Collections.Generic;
using SysClaims = System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAccountServices
    {
        Task<IEnumerable<SysClaims.Claim>> GetAllClaimsAsync(User user, CancellationToken token);
    }
}
