using System.ComponentModel.DataAnnotations;

namespace Semibox.Identity.Application.Account
{
    public record LoginInput
    {
        [Required(ErrorMessage = "Please enter a username.")]
        public string UserName { get; set; }
        [Required(ErrorMessage = "Please enter a password.")]
        public string Password { get; set; }
    }
}
