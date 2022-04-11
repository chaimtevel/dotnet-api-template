using Shared.Models.General;

namespace Services.HelperServices;

public interface IMessagingService
{
    Task<bool> SendEmailAsync(EmailMessageData data, CancellationToken token);
}
