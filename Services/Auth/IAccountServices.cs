using Microsoft.AspNetCore.Identity;
using Services.HelperServices;
using Shared.Models.DB.Identity;
using SysClaims = System.Security.Claims;

namespace Services.Auth;

public interface IAccountServices
{
    IGenericDataService<User> DataService { get; }
    Task<IEnumerable<SysClaims.Claim>> GetAllClaimsAsync(User user, CancellationToken token);
    Task<User> FindByEmailAsync(string email);
    Task<string> GeneratePasswordResetTokenAsync(User user);
    Task<IdentityResult> ResetPasswordAsync(User user, string token, string newPassword);
}