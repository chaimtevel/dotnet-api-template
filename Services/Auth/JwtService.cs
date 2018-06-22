using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Shared.Options;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Services.Auth
{
    internal class JwtService : IJwtService
    {

        public JwtService(IOptions<JwtOptions> jwtOptions)
        {
            JwtOptions = jwtOptions;
        }

        public IOptions<JwtOptions> JwtOptions { get; }


        public JwtSecurityToken GenerateToken(IEnumerable<Claim> claims)
        {
            var options = JwtOptions.Value;

            return new JwtSecurityToken(
                issuer: options.Issuer,
                audience: options.Audience,
                expires: DateTime.Now.AddMinutes(options.TokenDurationInMinutes),
                signingCredentials: new SigningCredentials(options.SecretToKey, SecurityAlgorithms.HmacSha256),
                claims: claims
            );
        }
    }
}
