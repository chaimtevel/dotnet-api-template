using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Services.HelperServices;
using Shared.Auth;
using Shared.Models.DB.Identity;

namespace Services.Auth
{
    internal class UserStore : IUserLoginStore<User>,
        IUserClaimStore<User>,
        IUserPasswordStore<User>,
        IUserSecurityStampStore<User>,
        IUserEmailStore<User>,
        IUserLockoutStore<User>,
        IUserPhoneNumberStore<User>,
        IQueryableUserStore<User>,
        IUserTwoFactorStore<User>,
        IUserAuthenticationTokenStore<User>,
        IUserAuthenticatorKeyStore<User>,
        IUserTwoFactorRecoveryCodeStore<User>
    {

        private const string AuthenticatorKeyTokenName = "AuthenticatorKey";
        private const string RecoveryCodeTokenName = "RecoveryCodes";

        public UserStore(IGenericDataService<User> dataService)
        {
            DataService = dataService;
        }

        public IGenericDataService<User> DataService { get; }

        public IQueryable<User> Users => DataService.AccessData<User>();

        public async Task AddClaimsAsync(User user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            var matchedClaims = await DataService.AccessData<Claim>()
                .Join(claims, oc => new { oc.ClaimType, oc.ClaimValue }, tc => new { ClaimType = tc.Type, ClaimValue = tc.Value }, (oc, tc) => new { OurClaim = oc, TheirClaim = tc })
                .ToArrayAsync(cancellationToken);

            foreach (var matchedClaim in matchedClaims)
            {
                DataService.Add(matchedClaim.OurClaim);
            }
            
        }

        public Task AddLoginAsync(User user, UserLoginInfo login, CancellationToken cancellationToken)
        {
            if(!Enum.TryParse(login.LoginProvider, out AuthProviders authProvider))
            {
                throw new ArgumentException($"Invalid value {login.LoginProvider}", nameof(login.LoginProvider));
            }

            DataService.Add(new UserLogin
            {
                UserID = user.Id,
                LoginProvider = authProvider,
                ProviderKey = login.ProviderKey,
                ProviderDisplayName = login.ProviderDisplayName
            });

            return Task.FromResult(false);
        }

        public async Task<int> CountCodesAsync(User user, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, AuthProviders.Internal.ToString(), RecoveryCodeTokenName, cancellationToken) ?? "";
            if (mergedCodes.Length > 0)
            {
                return mergedCodes.Split(';').Length;
            }

            return 0;
        }

        public async Task<IdentityResult> CreateAsync(User user, CancellationToken cancellationToken)
        {
            DataService.Add(user);
            await DataService.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public async Task<IdentityResult> DeleteAsync(User user, CancellationToken cancellationToken)
        {
            DataService.Remove(user);
            await DataService.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }

        public void Dispose()
        {
            
        }

        public Task<User> FindByEmailAsync(string normalizedEmail, CancellationToken cancellationToken)
            => DataService.AccessData<User>().FirstOrDefaultAsync(u => u.NormalizedEmail == normalizedEmail, cancellationToken)!;

        public Task<User> FindByIdAsync(string userId, CancellationToken cancellationToken)
        {
            if(!int.TryParse(userId, out int intUserId))
            {
                return Task.FromResult(null as User)!;
            }

            return DataService.AccessData<User>().FirstOrDefaultAsync(u => u.Id == intUserId, cancellationToken)!;
        }

        public async Task<User> FindByLoginAsync(string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(loginProvider, out AuthProviders authProvider))
            {
                return null!;
            }

            var userLogin = await DataService.AccessData<UserLogin>()
                .Include(ul => ul.User)
                .FirstOrDefaultAsync(ul => ul.LoginProvider == authProvider && ul.ProviderKey == providerKey, cancellationToken);

            return userLogin?.User!;
        }

        public Task<User> FindByNameAsync(string normalizedUserName, CancellationToken token)
            => DataService.AccessData<User>().FirstOrDefaultAsync(u => u.NormalizedUserName == normalizedUserName, token)!;

        public Task<int> GetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
            => Task.FromResult(user.AccessFailedCount);

        public Task<string> GetAuthenticatorKeyAsync(User user, CancellationToken cancellationToken)
        {
            return GetTokenAsync(user, AuthProviders.Internal.ToString(), AuthenticatorKeyTokenName, cancellationToken);
        }

        public async Task<IList<System.Security.Claims.Claim>> GetClaimsAsync(User user, CancellationToken cancellationToken)
        {
            var userClaims = await DataService.AccessData<UserClaim>()
                .Where(uc => uc.UserID == user.Id)
                .Select(uc => uc.Claim)
                .ToArrayAsync(cancellationToken);

            return userClaims.Select(uc => new System.Security.Claims.Claim(uc.ClaimType, uc.ClaimValue))
                .ToList();
        }

        public Task<string> GetEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Email);
        }

        public Task<bool> GetEmailConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.EmailConfirmed);
        }

        public Task<bool> GetLockoutEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnabled);
        }

        public Task<DateTimeOffset?> GetLockoutEndDateAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.LockoutEnd);
        }

        public async Task<IList<UserLoginInfo>> GetLoginsAsync(User user, CancellationToken cancellationToken)
        {
            var userLogins = await DataService.AccessData<UserLogin>()
                .Where(ul => ul.UserID == user.Id)
                .Select(ul => new { ul.LoginProvider, ul.ProviderKey, ul.ProviderDisplayName })
                .ToArrayAsync(cancellationToken);

            return userLogins.Select(a => new UserLoginInfo(a.LoginProvider.ToString(), a.ProviderKey, a.ProviderDisplayName))
                .ToList();
        }

        public Task<string> GetNormalizedEmailAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedEmail);
        }

        public Task<string> GetNormalizedUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.NormalizedUserName);
        }

        public Task<string> GetPasswordHashAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash);
        }

        public Task<string> GetPhoneNumberAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumber);
        }

        public Task<bool> GetPhoneNumberConfirmedAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PhoneNumberConfirmed);
        }

        public Task<string> GetSecurityStampAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.SecurityStamp);
        }

        public Task<string> GetTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(loginProvider, out AuthProviders authProvider))
            {
                return null!;
            }

            return DataService.AccessData<UserToken>()
                .Where(ut => ut.UserID == user.Id && ut.LoginProvider == authProvider && ut.Name == name)
                .Select(ut => ut.Value)
                .FirstOrDefaultAsync(cancellationToken)!;
        }

        public Task<bool> GetTwoFactorEnabledAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.TwoFactorEnabled);
        }

        public Task<string> GetUserIdAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.Id.ToString());
        }

        public Task<string> GetUserNameAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.UserName);
        }

        public async Task<IList<User>> GetUsersForClaimAsync(System.Security.Claims.Claim claim, CancellationToken cancellationToken)
        {
            var foundClaimId = await DataService.AccessData<Claim>()
                .Where(c => c.ClaimType == claim.Type && c.ClaimValue == claim.Value)
                .Select(c => c.ID)
                .FirstOrDefaultAsync(cancellationToken);

            if(foundClaimId == default)
            {
                return new List<User>();
            }

            return await DataService.AccessData<UserClaim>()
                .Where(uc => uc.ClaimID == foundClaimId)
                .Select(uc => uc.User)
                .ToListAsync(cancellationToken);
        }

        public Task<bool> HasPasswordAsync(User user, CancellationToken cancellationToken)
        {
            return Task.FromResult(user.PasswordHash != null);
        }

        public Task<int> IncrementAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount++;
            return Task.FromResult(user.AccessFailedCount);
        }

        public async Task<bool> RedeemCodeAsync(User user, string code, CancellationToken cancellationToken)
        {
            var mergedCodes = await GetTokenAsync(user, AuthProviders.Internal.ToString(), RecoveryCodeTokenName, cancellationToken) ?? "";
            var splitCodes = mergedCodes.Split(';');
            if (splitCodes.Contains(code))
            {
                var updatedCodes = new List<string>(splitCodes.Where(s => s != code));
                await ReplaceCodesAsync(user, updatedCodes, cancellationToken);
                return true;
            }
            return false;
        }

        public async Task RemoveClaimsAsync(User user, IEnumerable<System.Security.Claims.Claim> claims, CancellationToken cancellationToken)
        {
            var foundClaimIds = await DataService.AccessData<Claim>()
                .Join(claims, oc => new { oc.ClaimType, oc.ClaimValue }, tc => new { ClaimType = tc.Type, ClaimValue = tc.Value }, (oc, tc) => oc.ID)
                .ToArrayAsync(cancellationToken);

            var claimsToRemove = await DataService.AccessData<UserClaim>()
                .Where(uc => uc.UserID == user.Id && foundClaimIds.Contains(uc.ClaimID))
                .ToArrayAsync(cancellationToken);

            DataService.RemoveRange(claimsToRemove);
        }

        public async Task RemoveLoginAsync(User user, string loginProvider, string providerKey, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(loginProvider, out AuthProviders authProvider))
            {
                return;
            }

            var loginsToRemove = await DataService.AccessData<UserLogin>()
                .Where(ul => ul.UserID == user.Id && ul.LoginProvider == authProvider && ul.ProviderKey == providerKey)
                .ToArrayAsync(cancellationToken);

            DataService.RemoveRange(loginsToRemove);
        }

        public async Task RemoveTokenAsync(User user, string loginProvider, string name, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(loginProvider, out AuthProviders authProvider))
            {
                return;
            }

            var foundToken = await DataService.AccessData<UserToken>()
                .FirstOrDefaultAsync(ut => ut.UserID == user.Id && ut.LoginProvider == authProvider && ut.Name == name, cancellationToken);

            if(foundToken != null)
            {
                DataService.RemoveRange(new[] { foundToken });
            }
        }

        public Task ReplaceClaimAsync(User user, System.Security.Claims.Claim claim, System.Security.Claims.Claim newClaim, CancellationToken cancellationToken)
        {
            // for our structure of claims this doesn't make sense so I don't think we should implement
            return Task.CompletedTask;
        }

        public Task ReplaceCodesAsync(User user, IEnumerable<string> recoveryCodes, CancellationToken cancellationToken)
        {
            var mergedCodes = string.Join(";", recoveryCodes);
            return SetTokenAsync(user, AuthProviders.Internal.ToString(), RecoveryCodeTokenName, mergedCodes, cancellationToken);
        }

        public Task ResetAccessFailedCountAsync(User user, CancellationToken cancellationToken)
        {
            user.AccessFailedCount = 0;
            return Task.CompletedTask;
        }

        public Task SetAuthenticatorKeyAsync(User user, string key, CancellationToken cancellationToken)
        {
            return SetTokenAsync(user, AuthProviders.Internal.ToString(), AuthenticatorKeyTokenName, key, cancellationToken);
        }

        public Task SetEmailAsync(User user, string email, CancellationToken cancellationToken)
        {
            user.Email = email;
            return Task.CompletedTask;
        }

        public Task SetEmailConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.EmailConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetLockoutEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.LockoutEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetLockoutEndDateAsync(User user, DateTimeOffset? lockoutEnd, CancellationToken cancellationToken)
        {
            user.LockoutEnd = lockoutEnd;
            return Task.CompletedTask;
        }

        public Task SetNormalizedEmailAsync(User user, string normalizedEmail, CancellationToken cancellationToken)
        {
            user.NormalizedEmail = normalizedEmail;
            return Task.CompletedTask;
        }

        public Task SetNormalizedUserNameAsync(User user, string normalizedName, CancellationToken cancellationToken)
        {
            user.NormalizedUserName = normalizedName;
            return Task.CompletedTask;
        }

        public Task SetPasswordHashAsync(User user, string passwordHash, CancellationToken cancellationToken)
        {
            user.PasswordHash = passwordHash;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberAsync(User user, string phoneNumber, CancellationToken cancellationToken)
        {
            user.PhoneNumber = phoneNumber;
            return Task.CompletedTask;
        }

        public Task SetPhoneNumberConfirmedAsync(User user, bool confirmed, CancellationToken cancellationToken)
        {
            user.PhoneNumberConfirmed = confirmed;
            return Task.CompletedTask;
        }

        public Task SetSecurityStampAsync(User user, string stamp, CancellationToken cancellationToken)
        {
            user.SecurityStamp = stamp;
            return Task.CompletedTask;
        }

        public async Task SetTokenAsync(User user, string loginProvider, string name, string value, CancellationToken cancellationToken)
        {
            if (!Enum.TryParse(loginProvider, out AuthProviders authProvider))
            {
                return;
            }

            var foundToken = await DataService.AccessData<UserToken>()
                .FirstOrDefaultAsync(ut => ut.UserID == user.Id && ut.LoginProvider == authProvider && ut.Name == name, cancellationToken);

            if(foundToken == null)
            {
                foundToken = new UserToken
                {
                    LoginProvider = authProvider,
                    Name = name,
                    UserID = user.Id
                };

                DataService.Add(foundToken);
            }

            foundToken.Value = value;
        }

        public Task SetTwoFactorEnabledAsync(User user, bool enabled, CancellationToken cancellationToken)
        {
            user.TwoFactorEnabled = enabled;
            return Task.CompletedTask;
        }

        public Task SetUserNameAsync(User user, string userName, CancellationToken cancellationToken)
        {
            user.UserName = userName;
            return Task.CompletedTask;
        }

        public async Task<IdentityResult> UpdateAsync(User user, CancellationToken cancellationToken)
        {
            user.ConcurrencyStamp = Guid.NewGuid().ToString();
            DataService.Update(user);
            await DataService.SaveChangesAsync(cancellationToken);
            return IdentityResult.Success;
        }
    }
}
