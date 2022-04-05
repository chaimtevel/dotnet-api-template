namespace Shared.Extensions
{
    public static class ParsingExtensions
    {
        public static int ToInt(this string str) => int.TryParse(str, out int result) ? result : default;
        public static bool? ToBool(this string str) => bool.TryParse(str, out bool result) ? result : default(bool?);
    }
}
