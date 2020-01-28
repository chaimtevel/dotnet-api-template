using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Auth;
using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;
using Shared.Models.Responses;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace Api.Controllers
{
    [Route("api/[controller]"), ApiController]
    public class AuthController : Controller
    {
        public AuthController(
            IAuthenticationService authenticationService
            , IJwtService jwtService
            , IAccountServices accountServices
            , ILogger<AuthController> logger)
        {
            AuthenticationService = authenticationService;
            JwtService = jwtService;
            AccountServices = accountServices;
            Logger = logger;
        }

        public IAuthenticationService AuthenticationService { get; }
        public IJwtService JwtService { get; }
        public IAccountServices AccountServices { get; }
        public ILogger<AuthController> Logger { get; }

        [HttpPost("login/{authProvider}")]
        public async Task<ActionResult<AuthenticationResponse>>
            LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication, CancellationToken token)
        {
            var (isAuthenticated, user) = await AuthenticationService.LoginAsync(authProvider, authentication);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = await PrepareAuthenticationResponseAsync(user, token);

            return Ok(response);
        }
        
        [HttpPost("login/basic")]
        public async Task<ActionResult<AuthenticationResponse>>
            LoginAsync([FromBody] BasicAuthenticationRequest basicAuth, CancellationToken token)
        {
            var (isAuthenticated, user) = await AuthenticationService.LoginAsync(basicAuth);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = await PrepareAuthenticationResponseAsync(user, token);

            return Ok(response);
        }

        private async Task<AuthenticationResponse> PrepareAuthenticationResponseAsync(User user, CancellationToken token)
        {
            // generate our token
            
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // just an example
            };

            var allClaims = await AccountServices.GetAllClaimsAsync(user, token);

            var ourToken = JwtService.GenerateToken(claims.Concat(allClaims).ToArray());

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
                Expiration = ourToken.ValidTo
            };
        }

    }
}
