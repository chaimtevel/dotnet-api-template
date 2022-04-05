using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Services.HelperServices;
using Shared.Models.DB.Identity;
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
            , IEnumerable<IPasswordValidator<User>> passwordValidators
            , ILookupNormalizer keyNormalizer
            , IdentityErrorDescriber errors
            , IServiceProvider services
            , ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, Array.Empty<UserValidator<User>>(), passwordValidators, keyNormalizer, errors, services, logger)
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

            userClaims = userClaims.Concat(user.UserRoles.SelectMany(ur => ur.Role.RoleClaims)
                .Select(rc => new SysClaim.Claim(rc.Claim.ClaimType, rc.Claim.ClaimValue))).ToArray();

            return userClaims;
        }

    }
}
