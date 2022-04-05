namespace Services.HelperServices;

public interface IMessagingService
{
    Task<(bool succeeded, string message)> SendEmailAsync(CancellationToken token);
}
