using Services.HelperServices;
using Shared.Models.DB;

namespace Services.AppServices;

public interface IGeoService
{
    IGenericDataService<State> DataService { get; }

    Task<State[]> GetStatesAsync(CancellationToken token, bool detachEntities = false);
    Task<State[]> GetStatesCacheAsync(CancellationToken token);
    Task<State?> GetStateByIdAsync(int stateId, CancellationToken token, bool useCache = true);
    Task<bool> DoesStateExistAsync(int stateId, CancellationToken token);
    Task<IEnumerable<(int stateId, bool exists)>> DoStateIdsExistAsync(IEnumerable<int> stateIds, CancellationToken token);
}
