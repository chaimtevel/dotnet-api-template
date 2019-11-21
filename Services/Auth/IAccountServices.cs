using Shared.Models.DB;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Services.Auth
{
    public interface IAccountServices
    {
        Task<IEnumerable<Claim>> GetAllClaimsAsync(User user, CancellationToken token);
    }
}
