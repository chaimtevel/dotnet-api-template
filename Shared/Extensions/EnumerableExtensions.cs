namespace Shared.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<T> Each<T>(this IEnumerable<T> ie, Action<T, int> action, bool makeConcrete = true)
    {
        var collection = makeConcrete ? ie.ToArray() : ie;

        var i = 0;
        foreach (var e in collection)
        {
            action(e, i++);
        }

        return collection;
    }

    public static IEnumerable<T> Each<T>(this IEnumerable<T> ie, Action<T> action, bool makeConcrete = true)
    {
        var collection = makeConcrete ? ie.ToArray() : ie;

        foreach (var e in collection)
        {
            action(e);
        }

        return collection;
    }

    public static bool IsEmpty<T>(this IEnumerable<T> collection)
    {
        if (collection == null)
        {
            return true;
        }
        else
        {
            return collection.Any() == false;
        }
    }

}
