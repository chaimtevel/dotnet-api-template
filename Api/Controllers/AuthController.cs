using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Auth;
using Shared.Models;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Produces("application/json")]
    [Route("api/Auth")]
    [ApiController]
    public class AuthController : Controller
    {
        public AuthController(IAuthenticationService authenticationService, IJwtService jwtService, ILogger<AuthController> logger)
        {
            AuthenticationService = authenticationService;
            JwtService = jwtService;
            Logger = logger;
        }

        public IAuthenticationService AuthenticationService { get; }
        public IJwtService JwtService { get; }
        public ILogger<AuthController> Logger { get; }

        [HttpPost("login/{authProvider:authprovider}")]
        public async Task<IActionResult> Login(string authProvider, [FromBody] ExternalAuthentication authentication)
        {
            var user = await AuthenticationService.Login(Shared.Auth.AuthProviders.Google, authentication);

            // generate our token
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, default(int).ToString()) // just an example
            };

            var ourToken = JwtService.GenerateToken(claims);

            return Ok(new
            {
                Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
                Expiration = ourToken.ValidTo
            });
        }

    }
}
