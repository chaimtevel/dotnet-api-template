using Microsoft.Extensions.Logging;
using Services.HelperServices;
using Shared.Models.DB.Identity;
using Shared.Validation;

namespace Services.Extensions;

public static class MessagingExtensions
{
    public static Task<(ServiceValidation, string maskedSentToValue)> SendMfaMessageAsync(this IMessagingService messagingService
        , User user, string code, ILogger logger, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public static Task SendResetPasswordEmailAsync(this IMessagingService messagingService
        , string domain, string resetPwToken, string email, CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
