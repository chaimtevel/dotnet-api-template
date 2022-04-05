namespace Shared.Extensions;

public static class StringExtensions
{
    public static bool IsEmpty(this string? str) => string.IsNullOrWhiteSpace(str);

    public static bool HasValue(this string? str) => str.IsEmpty() == false;


}
