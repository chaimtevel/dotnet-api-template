using Shared.Constants;
using System.Security.Claims;
using static Shared.Extensions.ParsingExtensions;

namespace Shared.Auth
{
    public class AuthenticatedUser
    {
        public AuthenticatedUser(ClaimsPrincipal principal)
        {
            Principal = principal;
        }

        public ClaimsPrincipal Principal { get; }

        public int? ID => (Principal
            .FindFirstValue(ClaimTypes.NameIdentifier)
            ?.ToInt());

        public string Email => Principal.FindFirstValue(ClaimTypes.Email);
        public string? UserName => Principal?.Identity?.Name;

        public bool RequiresMfa => (Principal
            .FindFirstValue(Permissions.RequiresMfaClaim)
            ?.ToBool())
            .GetValueOrDefault();

        public IEnumerable<string> PermissionClaims => Principal
            .Claims
            .Where(c => c.Type == Permissions.PermissionClaim)
            .Select(c => c.Value);

        public bool HasPermissionClaim(string claimValue)
            => PermissionClaims.Contains(claimValue, StringComparer.OrdinalIgnoreCase);

    }
}
