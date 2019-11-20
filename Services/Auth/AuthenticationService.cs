using AutoMapper;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Models.DB;
using Shared.Models.Requests;
using Shared.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using static Google.Apis.Auth.GoogleJsonWebSignature;

namespace Services.Auth
{
    using AuthProviderMap = Dictionary<AuthProviders, Func<AuthenticationService, ExternalAuthenticationRequest, Task<(bool isAuthenticated, string email)>>>;

    internal class AuthenticationService : IAuthenticationService
    {
        private static AuthProviderMap authProviderMap = new AuthProviderMap()
        {
            { AuthProviders.Google, (s, a) => s.Google(a) }
        };

        public AuthenticationService(IOptions<JwtOptions> jwtOptions
            , IJwtService jwtService
            , SignInManager<User> signInManager
            , ILogger<IAuthenticationService> logger)
        {
            JwtOptions = jwtOptions;
            JwtService = jwtService;
            SignInManager = signInManager;
            Logger = logger;
        }

        public IOptions<JwtOptions> JwtOptions { get; }
        public IJwtService JwtService { get; }
        public SignInManager<User> SignInManager { get; }
        public ILogger<IAuthenticationService> Logger { get; }

        public async Task<(bool isAuthenticated, User user)>
            LoginAsync(BasicAuthenticationRequest authentication)
        {
            var signinResult = await SignInManager
                .PasswordSignInAsync(authentication.UserName, authentication.Password, true, true);

            if (signinResult.Succeeded)
            {
                var user = await SignInManager.UserManager.FindByNameAsync(authentication.UserName);

                return (signinResult.Succeeded, user);
            }
            else
            {
                return default;
            }
        }

        public async Task<(bool isAuthenticated, User user)>
            LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication)
        {
            authProviderMap.TryGetValue(authProvider, out Func<AuthenticationService, ExternalAuthenticationRequest, Task<(bool isAuthenticated, string email)>> val);

            (bool isAuthenticated, string email) = await val?.Invoke(this, authentication);

            if(!isAuthenticated)
            {
                return default;
            }

            // for now we'll work with email. we may be able to add additional options in the future
            var user = await SignInManager.UserManager.FindByEmailAsync(email);

            return (user != null, user);
        }
        
        private async Task<(bool isAuthenticated, string email)> Google(ExternalAuthenticationRequest authentication)
        {
            try
            {
                Payload tokenInfo = await ValidateAsync(authentication.Token);
                if (tokenInfo != null || !tokenInfo.AudienceAsList.Contains(JwtOptions.Value.ClientId, StringComparer.OrdinalIgnoreCase))
                {
                    return (true, tokenInfo.Email);
                }
            }
            catch (Exception e)
            {
                Logger.LogError(e.ToString());
                return default;
            }

            return default;
        }

    }
}
