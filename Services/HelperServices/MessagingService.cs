namespace Services.HelperServices;

internal class MessagingService : IMessagingService
{
    public Task<(bool succeeded, string message)> SendEmailAsync(CancellationToken token)
    {
        throw new NotImplementedException();
    }
}
