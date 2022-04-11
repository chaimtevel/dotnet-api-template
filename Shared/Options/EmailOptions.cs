namespace Shared.Options;

public class EmailOptions
{
    public string FromName { get; init; } = string.Empty;
    public string FromAddress { get; init; } = string.Empty;
    public string Domain { get; init; } = string.Empty;
    public int Port { get; set; }
    public string Username { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public ICollection<string> DevEmails { get; init; } = new HashSet<string>();
}
