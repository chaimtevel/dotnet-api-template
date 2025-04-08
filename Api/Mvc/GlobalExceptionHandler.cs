using Microsoft.AspNetCore.Diagnostics;

namespace Api.Mvc;

public class GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger) : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception, CancellationToken cancellationToken)
    {
        logger.LogError(exception, "An Error Occurred");

        await httpContext.Response.WriteAsJsonAsync(new
        {
            Message = exception?.Message ?? "Error processing request",
#if DEBUG
            exception?.StackTrace
#endif
        }, cancellationToken: cancellationToken);

        return true;
    }
}
