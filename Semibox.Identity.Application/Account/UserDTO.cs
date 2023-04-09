using Semibox.Identity.Domain.Entities;

namespace Semibox.Identity.Application.Account
{
    public record UserDTO
    {
        public string DisplayName { get; set; }
        public string Email { get; set; }
        public string Token { get; set; }
        public string AvatarUrl { get; set; }

        public static UserDTO Create(AppUser user, string token = default, string avatarUrl = default)
            => new UserDTO { DisplayName = user.DisplayName, Email = user.Email, Token = token, AvatarUrl = avatarUrl };
    }
}
