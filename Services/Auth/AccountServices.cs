using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Models.DB;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using System.Linq;

namespace Services.Auth
{
    internal class AccountServices : UserManager<User>, IAccountServices
    {

        public AccountServices(IUserStore<User> store
            , IOptions<IdentityOptions> optionsAccessor
            , IPasswordHasher<User> passwordHasher
            , IEnumerable<IPasswordValidator<User>> passwordValidators
            , ILookupNormalizer keyNormalizer
            , IdentityErrorDescriber errors
            , IServiceProvider services
            , ILogger<UserManager<User>> logger)
            : base(store, optionsAccessor, passwordHasher, new UserValidator<User>[0], passwordValidators, keyNormalizer, errors, services, logger)
        {

        }

        public override async Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(User user)
        {
            await Task.CompletedTask;
            return user.Claims.Select(c => new System.Security.Claims.Claim(c.Claim.ClaimType, c.Claim.ClaimValue)).ToList();
        }

        public override async Task<IList<string>> GetRolesAsync(User user)
        {
            await Task.CompletedTask;
            return user.UserRoles.Select(r => r.Role.NormalizedName).ToList();
        }


    }
}
