using Microsoft.AspNetCore.Identity;
using SchoolSystem.Models;

namespace SchoolSystem.Data
{
    public static class IdentitySeeder
    {
        public static async Task SeedAdminAsync(IServiceProvider services)
        {
            var roleManager =
                services.GetRequiredService<RoleManager<IdentityRole>>();

            var userManager =
                services.GetRequiredService<UserManager<ApplicationUser>>();

            // Create Roles
            string[] roles =
            {
                "Admin",
                "Lecturer",
                "Student"
            };

            foreach (var role in roles)
            {
                if (!await roleManager.RoleExistsAsync(role))
                {
                    await roleManager.CreateAsync(new IdentityRole(role));
                }
            }

            // Check if admin already exists
            string adminEmail = "admin@school.com";

            var admin = await userManager.FindByEmailAsync(adminEmail);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = adminEmail,
                    Email = adminEmail,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(
                    admin,
                    "Admin@123"
                );

                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
    }
}