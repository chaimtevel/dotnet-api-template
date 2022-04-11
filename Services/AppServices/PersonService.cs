using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Services.HelperServices;
using Shared.Helpers;
using Shared.Models.API;
using Shared.Models.DB;
using Shared.Validation;
using static Services.Extensions.ServiceValidationExtensions;
using static Shared.Extensions.ApiModelExtensions;

namespace Services.AppServices;

internal class PersonService : IPersonService
{
    public PersonService(IGenericDataService<Person> dataService, IGeoService geoService, ILogger<IPersonService> logger)
    {
        DataService = dataService;
        GeoService = geoService;
        Logger = logger;
    }

    public IGenericDataService<Person> DataService { get; }
    public IGeoService GeoService { get; }
    public ILogger<IPersonService> Logger { get; }


    public async Task<(Person?, ServiceValidation)> UpsertPersonAsync(PersonAPI api, CancellationToken token, bool doSave = true)
    {
        ServiceValidation validation = await GeoService.ValidateAsync(api, token);
        Person? db;

        if (api.ID.HasValue)
        {
            db = await DataService.AccessData<Person>()
                .FirstOrDefaultAsync(p => p.ID == api.ID, token);

            if (db == null)
            {
                validation.Add(Shared.Constants.Validation.PersonNotFound, api.ID);
                return (db, validation);
            }
        }
        else
        {
            DataService.Add(db = new Person { });
        }

        db.FirstName = api.FirstName;
        db.LastName = api.LastName;

        // for now we're just sticking to one email
        var email = db.ContactMethods.FirstOrDefault(cm => cm.Type == ContactMethodType.Email);
        if (email == null)
        {
            db.ContactMethods.Add(email = new() { Type = ContactMethodType.Email });
        }
        email.Value = api.Email.Trim();

        if (api.Address != null)
        {
            api.Address.UpdateAddress(db.Address ??= new Address());
        }

        if (!validation.IsValid) { return (db, validation); }

        if (doSave)
        {
            await DataService.SaveChangesAsync(token);
        }

        return (db, validation);
    }


}
