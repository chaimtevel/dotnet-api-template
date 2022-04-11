namespace Shared.Models.General;

public class EmailMessageData
{
    public ICollection<(string email, string? name)> ToAddresses { get; } = new HashSet<(string email, string? name)>();
    public ICollection<(string email, string? name)> CcAddresses { get; } = new HashSet<(string email, string? name)>();
    public ICollection<(string email, string? name)> BccAddresses { get; } = new HashSet<(string email, string? name)>();
    public string Subject { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsHtml { get; set; } = true;
}
