using Shared.Models.DB;

namespace Shared.Extensions;

public static class ModelExtensions
{
    public static string GetFullName(this Person person) => $"{person.FirstName} {person.LastName}";
}