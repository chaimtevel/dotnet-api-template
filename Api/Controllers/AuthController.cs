using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Auth;
using Shared.Auth;
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
        public async Task<ActionResult<AuthenticationResponse>> Login(string authProvider, [FromBody] ExternalAuthentication authentication)
        {
            var provider = System.Enum.Parse<AuthProviders>(authProvider, true);

            (bool isAuthenticated, IApiUser user) = await AuthenticationService.LoginAsync(provider, authentication);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = PrepareAuthenticationResponse(user);

            return Ok(response);
        }
        
        [HttpPost("login/basic")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] BasicAuthentication basicAuth)
        {
            (bool isAuthenticated, IApiUser user) = await AuthenticationService.LoginAsync(basicAuth);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = PrepareAuthenticationResponse(user);

            return Ok(response);
        }

        private AuthenticationResponse PrepareAuthenticationResponse(IApiUser user)
        {
            // generate our token
            
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Email),
                new Claim(ClaimTypes.NameIdentifier, user.ID.ToString()) // just an example
            };

            var ourToken = JwtService.GenerateToken(claims);

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
                Expiration = ourToken.ValidTo
            };
        }

    }
}
