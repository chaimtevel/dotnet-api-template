using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;
using Shared.Options;
using Shared.Validation;
using static Google.Apis.Auth.GoogleJsonWebSignature;
using static Shared.Constants.Validation;

namespace Services.Auth
{
    using AuthProviderMap = Dictionary<AuthProviders, Func<AuthenticationService, ExternalAuthenticationRequest, Task<(bool isAuthenticated, string email)>>>;

    internal class AuthenticationService : IAuthenticationService
    {
        private static readonly AuthProviderMap authProviderMap = new()
        {
            { AuthProviders.Google, (s, a) => s.GoogleAsync(a) }
        };

        public AuthenticationService(IOptions<GoogleOptions> googleOptions
            , IJwtService jwtService
            , SignInManager<User> signInManager
            , ILogger<IAuthenticationService> logger)
        {
            GoogleOptions = googleOptions;
            JwtService = jwtService;
            SignInManager = signInManager;
            Logger = logger;
        }

        public IOptions<GoogleOptions> GoogleOptions { get; }
        public IJwtService JwtService { get; }
        public SignInManager<User> SignInManager { get; }
        public ILogger<IAuthenticationService> Logger { get; }


        public async Task<(bool isAuthenticated, User? user, string? multiFactorCode)>
            LoginAsync(BasicAuthenticationRequest authentication)
        {
            var signInResult = await SignInManager
                .PasswordSignInAsync(authentication.UserName, authentication.Password, true, true);

            if (signInResult.Succeeded)
            {
                var user = await SignInManager.UserManager.FindByNameAsync(authentication.UserName);

                return (signInResult.Succeeded, user, default);
            }
            else if (signInResult.RequiresTwoFactor)
            {
                var user = await SignInManager.UserManager.FindByNameAsync(authentication.UserName);

                if (user?.Email == null)
                {
                    return default;
                }

                var (multiFactorCode, _) = await GenerateMfaCodeAsync(user.Email, user);

                return (true, user, multiFactorCode);
            }
            else
            {
                return default;
            }
        }

        public async Task<(string code, User? user)> GenerateMfaCodeAsync(string email, User? user = null)
        {
            string code = string.Empty;
            user ??= await SignInManager.UserManager.FindByEmailAsync(email);

            if (user == null) { return (code, user); }

            var providers = await SignInManager.UserManager.GetValidTwoFactorProvidersAsync(user);
            if (providers.Count > 0)
            {
                code = await SignInManager.UserManager.GenerateTwoFactorTokenAsync(user, providers[0]);
            }
            else
            {
                Logger.LogError("No provider found to generate mfa token for user {Email}", user.Email);
            }

            return (code, user);
        }

        public async Task<(bool isVerified, ServiceValidation validation, User? user)>
            VerifyMfaAsync(VerifyMfaRequest request, string email)
        {
            ServiceValidation validation = new();
            var user = await SignInManager.UserManager.FindByEmailAsync(email);
            var signInResult = SignInResult.TwoFactorRequired;

            if (user == null)
            {
                var (code, message) = NoUserWithThatEmailFound;
                validation.Add(code, string.Format(message, email));
                return (false, validation, user);
            }

            var providers = await SignInManager.UserManager.GetValidTwoFactorProvidersAsync(user);
            if (providers.Count > 0)
            {
                signInResult = await SignInManager.TwoFactorSignInAsync(providers[0], request.Code, false, request.RememberMe);
            }
            else
            {
                Logger.LogError("No provider found to verify 2fa token for user {Email}", user.Email);
            }

            if (signInResult.Succeeded)
            {
                return (true, validation, user);
            }
            else
            {
                validation.Add("InvalidMfaCode", $"Code {request.Code} is not valid for this user or is expired");
                return (false, validation, user);
            }
        }

        public async Task<(bool isAuthenticated, User? user)>
            LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication)
        {
            authProviderMap.TryGetValue(authProvider, out var val);

            bool isAuthenticated = false;
            string email = string.Empty;

            if(val is not null)
            {
                (isAuthenticated, email) = await val.Invoke(this, authentication);
            }

            if(!isAuthenticated)
            {
                return default;
            }

            // for now we'll work with email. we may be able to add additional options in the future
            var user = await SignInManager.UserManager.FindByEmailAsync(email);

            return (user != null, user);
        }

        private async Task<(bool isAuthenticated, string email)> GoogleAsync(ExternalAuthenticationRequest authentication)
        {
            try
            {
                Payload tokenInfo = await ValidateAsync(authentication.Token);
                if (tokenInfo != null && tokenInfo.AudienceAsList.Contains(GoogleOptions.Value.ClientID, StringComparer.OrdinalIgnoreCase))
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
