using System.ComponentModel.DataAnnotations;

namespace Shared.Models.Requests
{
    public class ExternalAuthenticationRequest
    {
        [Required(AllowEmptyStrings = false)]
        public string Token { get; set; }
    }
}
