namespace Shared.Models.API;

public class AddressAPI
{
    public string Address1 { get; set; } = string.Empty;
    public string? Address2 { get; set; }
    public string? City { get; set; }
    public string Zip { get; set; } = string.Empty;
    public int? StateID { get; set; }
}
