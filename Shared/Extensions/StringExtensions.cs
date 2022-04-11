using System.Text;

namespace Shared.Extensions;

public static class StringExtensions
{
    public static bool IsEmpty(this string? str) => string.IsNullOrWhiteSpace(str);

    public static bool HasValue(this string? str) => str.IsEmpty() == false;

    public static string? NullIfEmpty(this string str)
        => str.IsEmpty() ? null : str;

    public static string NormalizeNonNullable(this string str)
    {
        return str?.Trim() ?? string.Empty;
    }

    public static string? NormalizeNullable(this string? str)
    {
        return str?.NullIfEmpty()?.Trim();
    }

    public static string LowerCaseFirst(this string str)
    {
        if (str?.Length > 0)
        {
            var letters = str.ToCharArray();
            letters[0] = char.ToLower(letters[0]);
            return new string(letters);
        }

        return str ?? string.Empty;
    }

    public static string Left(this string str, int length)
    {
        // its probably fine to pass back the value that the caller passed even if its null in non-nullable contexts
        if(str is null) { return str!; }

        var maxLength = Math.Min(length, str.Length);
        return str[..maxLength];
    }

    public static string Right(this string str, int length)
    {
        if (str is null) { return str!; }

        var maxLength = Math.Min(length, str.Length);
        return str[^maxLength..];
    }

    public static StringBuilder AddHtmlNewLine(this StringBuilder stringBuilder, int numberOfLines = 1)
    {
        Enumerable.Range(1, numberOfLines).Each(i => stringBuilder.Append("<br />"));

        return stringBuilder;
    }
}
