using AutoMapper;
using Microsoft.AspNetCore.Identity;
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
    using AuthProviderMap = Dictionary<AuthProviders, Func<AuthenticationService, ExternalAuthentication, Task<(bool isAuthenticated, string email)>>>;

    internal class AuthenticationService : IAuthenticationService
    {
        private static AuthProviderMap authProviderMap = new AuthProviderMap()
        {
            { AuthProviders.Google, (s, a) => s.Google(a) }
        };

        public AuthenticationService(IOptions<JwtOptions> jwtOptions
            , IJwtService jwtService
            , SignInManager<IdentityUser> signInManager
            , UserManager<IdentityUser> userManager
            , ILogger<IAuthenticationService> logger
            , IMapper mapper)
        {
            JwtOptions = jwtOptions;
            JwtService = jwtService;
            SignInManager = signInManager;
            UserManager = userManager;
            Logger = logger;
            Mapper = mapper;
        }

        public IOptions<JwtOptions> JwtOptions { get; }
        public IJwtService JwtService { get; }
        public SignInManager<IdentityUser> SignInManager { get; }
        public UserManager<IdentityUser> UserManager { get; }
        public ILogger<IAuthenticationService> Logger { get; }
        public IMapper Mapper { get; }

        public async Task<(bool isAuthenticated, IApiUser user)> LoginAsync(BasicAuthentication authentication)
        {
            var signinResult = await SignInManager
                .PasswordSignInAsync(authentication.UserName, authentication.Password, true, true);

            if (signinResult.Succeeded)
            {
                var user = await UserManager.FindByNameAsync(authentication.UserName);
                var apiUser = Mapper.Map<ApiUser>(user);
                apiUser.Claims.AddRange(await UserManager.GetClaimsAsync(user));

                return (signinResult.Succeeded, apiUser);
            }
            else
            {
                return default;
            }
        }

        public async Task<(bool isAuthenticated, IApiUser user)> LoginAsync(AuthProviders authProvider, ExternalAuthentication authentication)
        {
            authProviderMap.TryGetValue(authProvider, out Func<AuthenticationService, ExternalAuthentication, Task<(bool isAuthenticated, string email)>> val);

            (bool isAuthenticated, string email) = await val?.Invoke(this, authentication);

            if(!isAuthenticated)
            {
                return default;
            }

            // for now we'll work with email. we may be able to add additional options in the future
            var user = await UserManager.FindByEmailAsync(email);
            var apiUser = Mapper.Map<ApiUser>(user);
            apiUser.Claims.AddRange(await UserManager.GetClaimsAsync(user));

            return (user != null, apiUser);
        }
        
        private async Task<(bool isAuthenticated, string email)> Google(ExternalAuthentication authentication)
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
