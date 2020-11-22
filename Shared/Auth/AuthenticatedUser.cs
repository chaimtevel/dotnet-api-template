using Shared.Constants;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;

namespace Shared.Auth
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser(ClaimsPrincipal principal)
        {
            Principal = principal;
        }

        public ClaimsPrincipal Principal { get; }

        public int? ID
        {
            get
            {
                if (int.TryParse(Principal.FindFirstValue(ClaimTypes.NameIdentifier), out int id))
                {
                    return id;
                }

                return default;
            }
        }

        public string Email => Principal.FindFirstValue(ClaimTypes.Email);
        public string UserName => Principal?.Identity?.Name;

        public IEnumerable<string> PermissionClaims => Principal
            .Claims
            .Where(c => c.Type == Permissions.PermissionClaim)
            .Select(c => c.Value);

        public bool HasPermissionClaim(string claimValue)
            => PermissionClaims.Contains(claimValue, StringComparer.OrdinalIgnoreCase);

    }
}
