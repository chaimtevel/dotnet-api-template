using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Requests
{
    public class BasicAuthenticationRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
