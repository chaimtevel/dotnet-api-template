namespace Shared.Messaging;

public class EmailMessageData
{
    public ICollection<EmailRecipient> ToAddresses { get; } = new HashSet<EmailRecipient>();
    public ICollection<EmailRecipient> CcAddresses { get; } = new HashSet<EmailRecipient>();
    public ICollection<EmailRecipient> BccAddresses { get; } = new HashSet<EmailRecipient>();
    public required string Subject { get; set; }
    public required string Body { get; set; }
    public bool IsHtml { get; set; } = true;
}
