using Flurl;
using Microsoft.Extensions.Logging;
using Services.HelperServices;
using Shared.Helpers;
using Shared.Models.DB.Identity;
using Shared.Models.General;
using Shared.Validation;
using System.Text;
using static Shared.Extensions.ModelExtensions;
using static Shared.Extensions.StringExtensions;

namespace Services.Extensions;

public static class MessagingExtensions
{
    public static async Task<IMessagingService> SendResetPasswordEmailAsync(this IMessagingService messagingService
        , string appUrl, string resetPwToken, User user, CancellationToken token, bool isNewUser = false)
    {
        var url = appUrl
                .AppendPathSegment("reset-password")
                .SetQueryParams(new
                {
                    token = resetPwToken,
                    email = user.Email
                });

        // TODO: add proper html content - this will probably be with razor views
        var html = new StringBuilder();

        if(isNewUser && user.Person is not null)
        {
            html.AppendLine($"Welcome {user.Person.FirstName}!").AddHtmlNewLine(2);
        }

        html.AppendLine("Click the link below to confirm your email and set your password").AddHtmlNewLine(2);
        html.AppendLine(url.ToString());

        var messageData = new EmailMessageData
        {
            Subject = "Welcome to the App!",
            IsHtml = true,
            Body = html.ToString()
        };

        messageData.ToAddresses.Add((user.Email, user.Person?.GetFullName()));

        await messagingService.SendEmailAsync(messageData, token);

        return messagingService;
    }

    public static async Task<(ServiceValidation, ContactMethodType, string maskedSentToValue)>
        SendMfaMessageAsync(this IMessagingService messagingService, User user, string code, ILogger logger, CancellationToken token)
    {
        ServiceValidation validation = new();
        string message = $"Your App verification code is: {code}";

        var html = $"<div style=\"font-size:14px;font-family:roboto,sans-serif;color:#212121;\"><b>{message}</b></div>";
        var messageData = new EmailMessageData
        {
            Subject = "Your App Verification Code",
            IsHtml = true,
            Body = html
        };

        messageData.ToAddresses.Add((user.Email, null));

        await messagingService.SendEmailAsync(messageData, token);

        string firstTwo = user.Email.Left(2), lastTwo = user.Email.Right(2);
        return (validation, ContactMethodType.Email, $"{firstTwo}******{lastTwo}");
    }
}
