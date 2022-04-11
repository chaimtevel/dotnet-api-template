using Shared.Models.API;
using Shared.Models.DB;

namespace Shared.Extensions;

public static class ApiModelExtensions
{

    public static AddressAPI UpdateAddress(this AddressAPI api, Address db)
    {
        db.Address1 = api.Address1.NormalizeNonNullable();
        db.Address2 = api.Address2.NormalizeNullable();
        db.City = api.City.NormalizeNullable();
        db.StateID = api.StateID;
        db.Zip = api.Zip.NormalizeNonNullable();

        return api;
    }

}
