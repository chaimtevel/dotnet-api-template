using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Services.Auth;
using Shared.Auth;
using Shared.Models.DB;
using Shared.Models.Requests;
using Shared.Models.Responses;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Api.Controllers
{
    [Route("api/[controller]"), ApiController]
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

        [HttpPost("login/{authProvider}")]
        public async Task<ActionResult<AuthenticationResponse>> Login(AuthProviders authProvider, [FromBody] ExternalAuthenticationRequest authentication)
        {
            var (isAuthenticated, user) = await AuthenticationService.LoginAsync(authProvider, authentication);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = PrepareAuthenticationResponse(user);

            return Ok(response);
        }
        
        [HttpPost("login/basic")]
        public async Task<ActionResult<AuthenticationResponse>> Login([FromBody] BasicAuthenticationRequest basicAuth)
        {
            var (isAuthenticated, user) = await AuthenticationService.LoginAsync(basicAuth);

            if (!isAuthenticated)
            {
                return Unauthorized();
            }

            AuthenticationResponse response = PrepareAuthenticationResponse(user);

            return Ok(response);
        }

        private AuthenticationResponse PrepareAuthenticationResponse(User user)
        {
            // generate our token
            
            var claims = new List<System.Security.Claims.Claim>
            {
                new System.Security.Claims.Claim(ClaimTypes.Email, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.Name, user.Email),
                new System.Security.Claims.Claim(ClaimTypes.NameIdentifier, user.Id.ToString()) // just an example
            };

            var mergedClaims = user
                .UserRoles
                .SelectMany(r => r.Role.RoleClaims.Select(c => c.Claim))
                .Concat(user.Claims.Select(c => c.Claim))
                .Select(c => new System.Security.Claims.Claim(c.ClaimType, c.ClaimValue));

            var ourToken = JwtService.GenerateToken(claims.Concat(mergedClaims).ToArray());

            return new AuthenticationResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
                Expiration = ourToken.ValidTo
            };
        }

    }
}
