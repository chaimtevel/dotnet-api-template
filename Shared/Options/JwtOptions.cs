using Microsoft.IdentityModel.Tokens;
using System.Text;

namespace Shared.Options;

public class JwtOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string JwtSecret { get; set; }
    public int TokenDurationInMinutes { get; set; }

    public SymmetricSecurityKey SecretToKey => new(Encoding.UTF8.GetBytes(JwtSecret));

}
