using Microsoft.AspNetCore.Identity;

namespace Karnel_Travel.Data;

// Seeds roles and the default admin account at startup.
public static class DbSeeder
{
    // Role names used across the app.
    public const string AdminRole = "Admin";
    public const string UserRole = "User";

    // Default admin login. Change the password before going live and move it to configuration.
    private const string AdminEmail = "admin@karneltravel.com";
    private const string AdminPassword = "Admin@123";

    // Creates roles, the admin user and default roles. Safe to run repeatedly.
    public static async Task SeedAsync(IServiceProvider services)
    {
        // Resolve the Identity services.
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

        // Create the roles if they do not exist.
        foreach (var role in new[] { AdminRole, UserRole })
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }

        // Create the admin user if it is missing.
        var admin = await userManager.FindByEmailAsync(AdminEmail);
        if (admin == null)
        {
            admin = new ApplicationUser
            {
                UserName = AdminEmail,
                Email = AdminEmail,
                EmailConfirmed = true,
                FullName = "Administrator"
            };

            // Create the account; fail fast if it cannot be created.
            var result = await userManager.CreateAsync(admin, AdminPassword);
            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new InvalidOperationException("Admin user create nahi hua: " + errors);
            }
        }

        // Make sure the admin has the Admin role.
        if (!await userManager.IsInRoleAsync(admin, AdminRole))
        {
            await userManager.AddToRoleAsync(admin, AdminRole);
        }

        // Give the User role to existing accounts that have no role.
        foreach (var user in userManager.Users.ToList())
        {
            if ((await userManager.GetRolesAsync(user)).Count == 0)
            {
                await userManager.AddToRoleAsync(user, UserRole);
            }
        }
    }
}
