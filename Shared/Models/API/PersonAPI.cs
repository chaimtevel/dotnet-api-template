using System.ComponentModel.DataAnnotations;

namespace Shared.Models.API;

public class PersonAPI
{
    public int? ID { get; set; }
    public string? FirstName { get; init; }
    public string LastName { get; init; } = string.Empty;
    /// <summary>
    /// For now we're using just a single email address which is alos used as the login username
    /// </summary>
    [Required(AllowEmptyStrings = false)]
    public string Email { get; init; } = string.Empty;
    public AddressAPI? Address { get; set; }
}