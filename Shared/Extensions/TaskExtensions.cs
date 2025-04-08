namespace Shared.Extensions;

public static class TaskExtensions
{
    public static Task<T> WrapInTask<T>(this T value)
    {
        return Task.FromResult(value);
    }
}
