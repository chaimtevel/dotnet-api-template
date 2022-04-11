using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace Shared.Extensions;

public static class EnumExtensions
{
    public static int ToInt<T>(this T enumValue) where T : Enum, IConvertible => enumValue.ToInt32(default);

    public static string GetDisplay<T>(this T value) where T : Enum
    {
        string strValue = value.ToString();

        FieldInfo? field = typeof(T).GetField(strValue);

        if (field is null) { return strValue; }

        DisplayAttribute? displayAttr = Attribute.GetCustomAttribute(field, typeof(DisplayAttribute)) as DisplayAttribute;

        if (displayAttr?.Name != null)
        {
            return displayAttr.Name;
        }

        DescriptionAttribute? descriptionAttr = Attribute.GetCustomAttribute(field, typeof(DescriptionAttribute)) as DescriptionAttribute;

        return descriptionAttr?.Description ?? strValue;
    }

    public static IEnumerable<T> GetEnums<T>(IEnumerable<T>? excludeList = default) where T : Enum
    {
        return Enum.GetValues(typeof(T))
            .Cast<T>()
            .Except(excludeList ?? Enumerable.Empty<T>())
            .ToArray();
    }

}