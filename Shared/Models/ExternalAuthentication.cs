using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class ExternalAuthentication
    {
        [Required(AllowEmptyStrings = false)]
        public string Token { get; set; }
    }
}
