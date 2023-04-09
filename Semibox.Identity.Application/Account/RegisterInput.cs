using System.ComponentModel.DataAnnotations;

namespace Semibox.Identity.Application.Account
{
    public record RegisterInput
    {
        [Required(ErrorMessage = "Please enter a username.")]
        public string UserName { get; set; }

        [Required(ErrorMessage = "Please enter an email address.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Please enter a display name.")]
        public string DisplayName { get; set; }

        [Required(ErrorMessage = "Please enter a password.")]
        public string Password { get; set; }
    }
}
