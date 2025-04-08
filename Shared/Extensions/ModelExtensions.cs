using Shared.Models.General;

namespace Shared.Extensions;

public static class ModelExtensions
{
    public static string GetFullName<T>(this T person) where T : IPersonName
        => $"{person.FirstName} {person.LastName}";
}