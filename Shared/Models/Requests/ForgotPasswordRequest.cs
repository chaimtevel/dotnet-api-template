using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Requests;

public class ForgotPasswordRequest
{
    [Required(AllowEmptyStrings = false), EmailAddress]
    public string Email { get; init; } = string.Empty;
}
