namespace Shared.Extensions;

public static class ReflectionExtensions
{
    public static bool Implements<TInterface>(this Type type)
        => type.Implements(typeof(TInterface));

    public static bool Implements(this Type type, Type typeToCheck)
        => typeToCheck.IsAssignableFrom(type);

    public static IReadOnlyDictionary<string, object?> ToDictionary(this object obj, bool ignoreNulls = true, bool lowercaseFirst = false)
    {
        var props = obj.GetType().GetProperties();
        var dictionary = new Dictionary<string, object?>(props.Length);

        foreach (var prop in props)
        {
            var val = prop.GetValue(obj);

            if (ignoreNulls && val == null) { continue; }

            dictionary.Add(lowercaseFirst ? prop.Name.LowerCaseFirst() : prop.Name, prop.GetValue(obj));
        }

        return dictionary;
    }

}