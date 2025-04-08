using Shared.Models.General;

namespace Shared.Messaging;

public class EmailRecipient : IPersonName
{
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public required string Email { get; init; }
}
