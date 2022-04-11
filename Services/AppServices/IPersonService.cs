using Shared.Models.API;
using Shared.Models.DB;
using Shared.Validation;

namespace Services.AppServices;

public interface IPersonService
{
    Task<(Person?, ServiceValidation)> UpsertPersonAsync(PersonAPI api, CancellationToken token, bool doSave = true);
}
