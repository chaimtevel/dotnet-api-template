using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Shared.Options;

public class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string JwtSecret { get; init; } = string.Empty;
    public int TokenDurationInMinutes { get; init; }

    public SymmetricSecurityKey SecretToKey => new(Encoding.UTF8.GetBytes(JwtSecret));

}
