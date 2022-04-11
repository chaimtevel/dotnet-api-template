using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.HelperServices;
using Shared.Models.DB;
using static Shared.Extensions.EnumerableExtensions;

namespace Services.AppServices;

internal class GeoService : IGeoService
{
    public GeoService(IGenericDataService<State> dataService
        , ICachingService cachingService
        , ILogger<IGeoService> logger)
    {
        DataService = dataService;
        CachingService = cachingService;
        Logger = logger;
    }

    public IGenericDataService<State> DataService { get; }
    public ICachingService CachingService { get; }
    public ILogger<IGeoService> Logger { get; }


    #region State

    public Task<State[]> GetStatesAsync(CancellationToken token, bool detachEntities = false)
    {
        var query = DataService.Entities;

        if (detachEntities)
        {
            query = query.AsNoTracking();
        }

        return query.OrderBy(s => s.Name).ToArrayAsync(token);
    }

    public Task<State[]> GetStatesCacheAsync(CancellationToken token)
        => CachingService.GetCacheAsync(Shared.Constants.CacheKeys.StatesCache, t => GetStatesAsync(t, true), token, TimeSpan.FromDays(1));

    public async Task<State?> GetStateByIdAsync(int stateId, CancellationToken token, bool useCache = true)
    {
        if (useCache)
        {
            return (await GetStatesCacheAsync(token)).FirstOrDefault(s => s.ID == stateId);
        }
        else
        {
            return await DataService.Entities.FirstOrDefaultAsync(s => s.ID == stateId, token);
        }
    }

    public Task<bool> DoesStateExistAsync(int stateId, CancellationToken token)
        => DataService.Entities.AnyAsync(s => s.ID == stateId, token);

    public async Task<IEnumerable<(int stateId, bool exists)>> DoStateIdsExistAsync(IEnumerable<int> stateIds, CancellationToken token)
    {
        var results = new List<(int stateId, bool exists)>();

        if (stateIds.IsEmpty()) { return results; }

        stateIds = stateIds.ToHashSet();

        var foundStateIds = (await GetStatesCacheAsync(token)).Select(s => s.ID);

        foreach (var stateId in stateIds)
        {
            results.Add((stateId, foundStateIds.Any(si => si == stateId)));
        }

        return results;
    }

    #endregion

}