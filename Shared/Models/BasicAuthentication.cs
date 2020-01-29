using System.ComponentModel.DataAnnotations;

namespace Shared.Models
{
    public class BasicAuthentication
    {
        [Required(AllowEmptyStrings = false)]
        public string UserName { get; set; }

        [Required(AllowEmptyStrings = false)]
        public string Password { get; set; }
    }
}
