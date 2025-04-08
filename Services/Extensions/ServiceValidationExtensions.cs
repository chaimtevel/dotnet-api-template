using Microsoft.EntityFrameworkCore;
using Services.AppServices;
using Services.HelperServices;
using Shared.Models.API;
using Shared.Validation;
using System.Linq.Expressions;
using static Shared.Extensions.EnumerableExtensions;

namespace Services.Extensions;

public static class ServiceValidationExtensions
{
    public static async Task<ServiceValidation> ValidateIDsAsync<T>(this IDataService dataService, IEnumerable<int> ids
        , Expression<Func<T, int>> selector, CancellationToken token)
        where T : class
    {
        var validation = new ServiceValidation();

        var set = ids.ToHashSet();
        var foundCount = await dataService.AccessData<T>()
            .Select(selector)
            .Distinct()
            .CountAsync(i => set.Contains(i), token);

        // if the set count is less than what came back from the db that means that not all id's were found in the db
        if (foundCount != set.Count)
        {
            validation.Add("InvalidIdSent", $"One of these id's aren't valid. {string.Join(", ", ids)}");
        }

        return validation;
    }

    public static async Task<ServiceValidation> ValidateStateIdsAsync(this IGeoService geoService, IEnumerable<int> stateIds, CancellationToken token)
    {
        ServiceValidation validation = [];
        var validatedStateIds = await geoService.DoStateIdsExistAsync(stateIds, token);

        var invalidStateIds = validatedStateIds.Where(v => !v.exists).Select(s => s.stateId);

        if (invalidStateIds.Any())
        {
            validation.Add("InvalidStateIds", $"Invalid state id's found ({string.Join(", ", invalidStateIds)}).");
        }

        return validation;
    }

    #region api payload validations

    public static async Task<ServiceValidation> ValidateAsync(this IGeoService geoService, PersonAPI personAPI, CancellationToken token)
    {
        var validation = new ServiceValidation();

        if (personAPI.Email.IsEmpty())
        {
            validation.Add("EmailRequired", "Email is required for updating profile");
        }

        if (personAPI.Address is not null)
        {
            (await geoService.ValidateAsync(personAPI.Address, token)).CopyTo(validation);
        }

        return validation;
    }

    public static async Task<ServiceValidation> ValidateAsync(this IGeoService geoService, AddressAPI address, CancellationToken token)
    {
        ServiceValidation validation = [];

        if (address?.StateID.HasValue == true)
        {
            (await geoService.ValidateStateIdsAsync([address.StateID.Value], token))
                .CopyTo(validation);
        }

        return validation;
    }

    #endregion

}