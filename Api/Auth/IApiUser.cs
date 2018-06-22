namespace Api.Auth
{
    public interface IApiUser
    {
        int ID { get; }
        string Email { get; }
        string UserName { get; }
    }
}
