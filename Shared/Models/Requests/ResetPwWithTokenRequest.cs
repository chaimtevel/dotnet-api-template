using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Requests;

public class ResetPwWithTokenRequest
{
    [Required(AllowEmptyStrings = false), EmailAddress]
    public string Email { get; init; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string Token { get; init; } = string.Empty;
    [Required(AllowEmptyStrings = false)]
    public string NewPw { get; init; } = string.Empty;
}
