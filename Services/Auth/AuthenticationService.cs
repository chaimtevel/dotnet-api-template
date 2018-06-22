using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Models;
using Shared.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Services.Auth
{
    using AuthProviderMap = Dictionary<AuthProviders, Func<AuthenticationService, ExternalAuthentication, Task<bool>>>;

    internal class AuthenticationService : IAuthenticationService
    {
        private static AuthProviderMap authProviderMap = new AuthProviderMap()
        {
            { AuthProviders.Google, (s, a) => s.Google(a) }
        };

        public AuthenticationService(IOptions<JwtOptions> jwtOptions, IJwtService jwtService, ILogger<IAuthenticationService> logger)
        {
            JwtOptions = jwtOptions;
            JwtService = jwtService;
            Logger = logger;
        }

        public IOptions<JwtOptions> JwtOptions { get; }
        public IJwtService JwtService { get; }
        public ILogger<IAuthenticationService> Logger { get; }

        public async Task<IApiUser> Login(BasicAuthentication authentication)
        {
            await Task.CompletedTask;
            return new ApiUser();
        }

        public async Task<IApiUser> Login(AuthProviders authProvider, ExternalAuthentication authentication)
        {
            authProviderMap.TryGetValue(authProvider, out Func<AuthenticationService, ExternalAuthentication, Task<bool>> val);

            bool hasProvidorAuthenticated = await val?.Invoke(this, authentication);

            // for now we'll work with email. we may be able to add additional options in the future

            return new ApiUser();
        }
        
        private async Task<bool> Google(ExternalAuthentication authentication)
        {
            try
            {
                Payload tokenInfo = await ValidateAsync(authentication.Token);
                if (tokenInfo != null || !tokenInfo.AudienceAsList.Contains(JwtOptions.Value.ClientId, StringComparer.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return false;
            }

            return false;
        }

    }
}
