using Microsoft.AspNetCore.Identity;

namespace Semibox.Identity.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        [PersonalData]
        public string DisplayName { get; set; }
    }
}
