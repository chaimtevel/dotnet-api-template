using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Models.DB;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Threading;
using Services.HelperServices;

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
            : base(store, optionsAccessor, passwordHasher, new UserValidator<User>[0], passwordValidators, keyNormalizer, errors, services, logger)
        {
            DataService = dataService;
        }

        public IGenericDataService<User> DataService { get; }

        /// <summary>
        /// We override so that we get claims from both the user and from the users roles
        /// </summary>
        /// <param name="user"></param>
        /// <returns>A combined list of claims from the user and from the user roles</returns>
        public async Task<IEnumerable<Claim>> GetAllClaimsAsync(User user, CancellationToken token)
        {
            var userClaims = await base.GetClaimsAsync(user);
            
            var userRoleIds = user.UserRoles.Select(ur => ur.RoleId).ToArray();
            if(userRoleIds.Any())
            {
                var roleClaims = await DataService.AccessData<IdentityRoleClaim<int>>()
                    .Where(rc => userRoleIds.Contains(rc.RoleId)).ToArrayAsync(token);

                userClaims = userClaims.Concat(roleClaims.Select(rc => new Claim(rc.ClaimType, rc.ClaimValue))).ToArray();
            }

            return userClaims;
        }

    }
}
