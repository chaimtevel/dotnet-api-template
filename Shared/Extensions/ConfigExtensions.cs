using Microsoft.Extensions.Configuration;

namespace Shared.Extensions;

public static class ConfigExtensions
{
    public static string? GetAppUrl(this IConfiguration configuration)
        => configuration["AppUrl"];
}