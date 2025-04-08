using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.AppServices;
using Services.HelperServices;
using Shared.Models.API;
using Shared.Models.DB.Identity;
using Shared.Validation;
using static Services.Extensions.ServiceValidationExtensions;
using static Shared.Extensions.EnumerableExtensions;
using SysClaim = System.Security.Claims;

namespace Services.Auth
{
    /// <summary>
    /// Use this class to customize aspnet identity user manager behavior
    /// </summary>
    internal class AccountServices : UserManager<User>, IAccountServices
    {

        public AccountServices(IGenericDataService<User> dataService
            , IUserStore<User> store
            , IOptions<IdentityOptions> optionsAccessor
            , IPasswordHasher<User> passwordHasher
            , IEnumerable<IUserValidator<User>> userValidators
            , IEnumerable<IPasswordValidator<User>> passwordValidators
            , ILookupNormalizer keyNormalizer
            , IdentityErrorDescriber errors
            , IServiceProvider services
            , ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, userValidators, passwordValidators, keyNormalizer, errors, services, logger)
        {
            DataService = dataService;
        }

        public IGenericDataService<User> DataService { get; }

        /// <summary>
        /// We override so that we get claims from both the user and from the users roles
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A combined list of claims from the user and from the user roles</returns>
        public async Task<IEnumerable<SysClaim.Claim>> GetAllClaimsAsync(User user, CancellationToken token)
        {
            var userClaims = await base.GetClaimsAsync(user);

            var roleClaims = user.UserRoles
                .SelectMany(ur => ur.Role.RoleClaims)
                .Select(rc => new SysClaim.Claim(rc.Claim.ClaimType, rc.Claim.ClaimValue));

            userClaims = userClaims.Concat(roleClaims).ToArray();

            return userClaims;
        }

        public async Task<(User?, ServiceValidation)> CreateUserAsync(UserAPI api
            , IPersonService personService, CancellationToken token)
        {
            ServiceValidation validation = await DataService.ValidateIDsAsync<Role>(api.RoleIDs, r => r.Id, token);

            var (personDb, personValidation) = await personService.UpsertPersonAsync(api, token, false);
            personValidation.CopyTo(validation);

            if (!validation.IsValid) { return (null, validation); }

            var user = new User
            {
                Email = api.Email,
                EmailConfirmed = false,
                UserName = api.Email,
                Person = personDb
            };

            var foundRoles = await DataService.AccessData<Role>()
                .Where(r => api.RoleIDs.Contains(r.Id))
                .ToDictionaryAsync(r => r.Id, token);

            // rely that these role ids are valid based on the above validation
            foreach (var userRoleId in api.RoleIDs)
            {
                var role = foundRoles[userRoleId];

                user.UserRoles.Add(new UserRole { Role = role, User = user });
            }

            IdentityResult userCreateResult = await CreateAsync(user, api.Password);

            userCreateResult.Errors.Each(e => validation.Add(e.Code, e.Description));

            return (user, validation);
        }

    }
}
