using Microsoft.AspNetCore.Mvc;
using Services.Auth;
using Services.HelperServices;
using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;
using Shared.Models.Responses;
using System.IdentityModel.Tokens.Jwt;
using static Services.Extensions.MessagingExtensions;
using static Shared.Constants.Permissions;
using static Shared.Extensions.StringExtensions;
using static Shared.Extensions.UserExtensions;
using static Shared.Extensions.ValidationExtensions;

namespace Api.Controllers;

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
    public async Task<ActionResult<AuthenticationResponse>> LoginAsync([FromBody] BasicAuthenticationRequest basicAuth
            , [FromServices] IMessagingService messagingService, CancellationToken token)
    {
        var (isAuthenticated, user, multiFactorCode) = await AuthenticationService.LoginAsync(basicAuth);

        AuthenticationResponse response = new();

        if (!isAuthenticated || user is null)
        {
            return Unauthorized();
        }
        else if (multiFactorCode.HasValue())
        {
            // we need to send a 2fa message
            var (validation, maskedSentToValue) = await messagingService
                .SendMfaMessageAsync(user, multiFactorCode!, Logger, token);

            if (!validation.IsValid) { return BadRequest(validation.CopyToModelState(ModelState)); }

            response = PrepareAuthenticationResponseForMFAAsync(user, maskedSentToValue);
        }
        else
        {
            response = await PrepareAuthenticationResponseAsync(user, token);
        }

        return Ok(response);
    }

    private async Task<AuthenticationResponse> PrepareAuthenticationResponseAsync(User user, CancellationToken token)
    {
        var claims = user.GetBaseClaims();
        claims.AddRange(await AccountServices.GetAllClaimsAsync(user, token));

        var ourToken = JwtService.GenerateToken(claims);

        return new AuthenticationResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
            Expiration = ourToken.ValidTo
        };
    }

    private AuthenticationResponse PrepareAuthenticationResponseForMFAAsync(User user, string maskedSentToValue)
    {
        var claims = user.GetBaseClaims();
        claims.Add(new System.Security.Claims.Claim(RequiresMfaClaim, bool.TrueString));

        var ourToken = JwtService.GenerateToken(claims);

        return new AuthenticationResponse
        {
            Token = new JwtSecurityTokenHandler().WriteToken(ourToken),
            Expiration = ourToken.ValidTo,
            RequiresMfa = true,
            MfaInfo = new() { MaskedSentToValue = maskedSentToValue }
        };
    }

}
