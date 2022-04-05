namespace Shared.Constants;

public static class Validation
{
    public static readonly (string code, string message) NoUserWithThatEmailFound = (nameof(NoUserWithThatEmailFound), "User with email {0} not found");
}