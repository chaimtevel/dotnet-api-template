namespace Api.Auth
{
    public class ApiUser : IApiUser
    {
        public int ID { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }
    }
}
