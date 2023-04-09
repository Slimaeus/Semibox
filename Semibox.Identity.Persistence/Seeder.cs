using Microsoft.AspNetCore.Identity;
using Semibox.Identity.Domain.Entities;

namespace Semibox.Identity.Persistence
{
    public static class Seeder
    {
        public static async Task SeedAsync(UserManager<AppUser> userManager)
        {
            if (userManager.Users.Any()) return;

            var users = new List<AppUser>
                {
                    new AppUser
                    {
                        DisplayName = "thai",
                        UserName = "thai",
                        Email = "thai@test.com"
                    },
                    new AppUser
                    {
                        DisplayName = "mei",
                        UserName = "mei",
                        Email = "mei@test.com"
                    },
                    new AppUser
                    {
                        DisplayName = "a",
                        UserName = "a",
                        Email = "a@test.com"
                    },
                    new AppUser
                    {
                        DisplayName = "string",
                        UserName = "string",
                        Email = "string@test.com"
                    }
                };

            foreach (var user in users)
            {
                await userManager.CreateAsync(user, "P@ssw0rd");
            }
        }
    }
}
