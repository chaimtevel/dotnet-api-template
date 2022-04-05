namespace Shared.Models.Responses
{
    public class MfaMetaData
    {
        public string MaskedSentToValue { get; init; } = string.Empty;
    }

    public class AuthenticationResponse
    {
        public string Token { get; init; } = string.Empty;
        public DateTime Expiration { get; init; }
        public bool RequiresMfa { get; init; }

        public MfaMetaData MfaInfo { get; init; } = new();
    }
}
