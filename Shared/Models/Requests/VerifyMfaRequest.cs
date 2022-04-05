using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Requests;

public class VerifyMfaRequest
{
    [Required(AllowEmptyStrings = false)]
    public string Code { get; init; } = string.Empty;
    public bool RememberMe { get; set; }
}
