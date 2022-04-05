using Shared.Auth;
using Shared.Models.DB.Identity;
using Shared.Models.Requests;
using Shared.Validation;

namespace Services.Auth;

public interface IAuthenticationService
{
    Task<(bool isAuthenticated, User? user, string? multiFactorCode)> LoginAsync(BasicAuthenticationRequest authentication);
    Task<(bool isAuthenticated, User? user)> LoginAsync(AuthProviders authProvider, ExternalAuthenticationRequest authentication);
    Task<(string code, User? user)> GenerateMfaCodeAsync(string email, User? user = null);
    Task<(bool isVerified, ServiceValidation validation, User? user)> VerifyMfaAsync(VerifyMfaRequest request, string email);
}