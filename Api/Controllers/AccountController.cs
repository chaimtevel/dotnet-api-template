using Microsoft.AspNetCore.Mvc;
using Services.Auth;
using Services.HelperServices;
using Shared.Models.Requests;
using static Services.Extensions.MessagingExtensions;
using static Shared.Extensions.ConfigExtensions;
using static Shared.Extensions.EnumerableExtensions;

namespace Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class AccountController : ControllerBase
{
    public AccountController(IAccountServices accountServices
        , IConfiguration configuration
        , ILogger<AccountController> logger)
    {
        AccountServices = accountServices;
        Configuration = configuration;
        Logger = logger;
    }

    public IAccountServices AccountServices { get; }
    public IConfiguration Configuration { get; }
    public ILogger<AccountController> Logger { get; }


    [HttpPost("forgotPw")]
    public async Task<ActionResult> ForgotPasswordAsync(ForgotPasswordRequest request
        , [FromServices] IMessagingService messagingService, CancellationToken token)
    {
        // first make sure this email is in the system
        var user = await AccountServices.FindByEmailAsync(request.Email);

        if (user == null)
        {
            var (code, message) = Shared.Constants.Validation.NoUserWithThatEmailFound;
            ModelState.AddModelError(code, string.Format(message, request.Email));
            return BadRequest(ModelState);
        }

        var resetPwToken = await AccountServices.GeneratePasswordResetTokenAsync(user);

        var url = Configuration.GetAppUrl();
        if (string.IsNullOrWhiteSpace(url))
        {
            Logger.LogError("No domain configured to use in links for emails");
        }

        await messagingService.SendResetPasswordEmailAsync(url, resetPwToken, request.Email, token);

        return Ok();
    }

    [HttpPost("forgotPwReset")]
    public async Task<ActionResult> ResetPassword(ResetPwWithTokenRequest request, CancellationToken token)
    {
        // first make sure this email is in the system
        var user = await AccountServices.FindByEmailAsync(request.Email);

        if (user == null)
        {
            var (code, message) = Shared.Constants.Validation.NoUserWithThatEmailFound;
            ModelState.AddModelError(code, string.Format(message, request.Email));
            return BadRequest(ModelState);
        }

        var validation = await AccountServices
            .ResetPasswordAsync(user, request.Token, request.NewPw);

        if (!validation.Succeeded)
        {
            validation.Errors.Each(e => ModelState.AddModelError(e.Code, e.Description), makeConcrete: false);
            return BadRequest(ModelState);
        }

        // set to email confirmed
        // this assumes that the confirmation was done via email
        user.EmailConfirmed = true;
        await AccountServices.DataService.SaveChangesAsync(token);

        return Accepted();
    }

}
