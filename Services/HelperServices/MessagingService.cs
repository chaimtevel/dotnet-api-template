using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using MimeKit.Text;
using Shared.Models.General;
using Shared.Options;
using static Shared.Extensions.EnumerableExtensions;

namespace Services.HelperServices;

internal class MessagingService : IMessagingService
{
    public MessagingService(IOptions<EmailOptions> emailOptions, IHostEnvironment hostEnvironment,  ILogger<IMessagingService> logger)
    {
        EmailOptions = emailOptions.Value;
        HostEnvironment = hostEnvironment;
        Logger = logger;
    }

    public EmailOptions EmailOptions { get; }
    public IHostEnvironment HostEnvironment { get; }
    public ILogger<IMessagingService> Logger { get; }


    public async Task<bool> SendEmailAsync(EmailMessageData data, CancellationToken token)
    {
        MimeMessage message = new();
        message.From.Add(new MailboxAddress(EmailOptions.FromName, EmailOptions.FromAddress));

        data.ToAddresses.Each(a => message.To.Add(new MailboxAddress(a.name, a.email)));
        data.CcAddresses.Each(a => message.Cc.Add(new MailboxAddress(a.name, a.email)));
        data.BccAddresses.Each(a => message.Bcc.Add(new MailboxAddress(a.name, a.email)));

        CheckForDevEmails(message);

        message.Subject = data.Subject;
        message.Body = new TextPart(data.IsHtml ? TextFormat.Html : TextFormat.Plain)
        {
            Text = data.Body
        };

        try
        {
            using var client = new MailKit.Net.Smtp.SmtpClient();

            // TODO: when we wire up for prod we probably should fix this logic
            client.ServerCertificateValidationCallback = (s, c, h, e) => true;

            await client.ConnectAsync(EmailOptions.Domain, EmailOptions.Port, false, token);

            client.Authenticate(EmailOptions.Username, EmailOptions.Password);
            await client.SendAsync(message, token);

            // if we ever start sending emails with better perf then this will need to be taken out
            // so that we can cache the connection
            await client.DisconnectAsync(true, token);

            return true;
        }
        catch (Exception e)
        {
            Logger.LogError(e, "Error sending email with subject of {Subject} and recipients of {Recipients}", data.Subject, string.Join(",", data.ToAddresses));
            
            return false;
        }
    }

    private MimeMessage CheckForDevEmails(MimeMessage message)
    {
        var hasDevEmails = (EmailOptions.DevEmails?.Any()).GetValueOrDefault();

        if(HostEnvironment.IsDevelopment() && !hasDevEmails)
        {
            throw new InvalidOperationException("You need to add destination emails in dev environments using the config key EmailOptions:DevEmails");
        }

        if (hasDevEmails)
        {
            message.To.Clear();
            message.Cc.Clear();
            message.Bcc.Clear();

            foreach (var email in (EmailOptions.DevEmails ?? Enumerable.Empty<string>()))
            {
                if(MailboxAddress.TryParse(email, out MailboxAddress emailAddress))
                {
                    message.To.Add(emailAddress);
                }
            }
        }

        return message;
    }

}
