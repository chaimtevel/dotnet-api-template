using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Services.Auth;

public interface IJwtService
{
    JwtSecurityToken GenerateToken(IEnumerable<Claim> claims);
}