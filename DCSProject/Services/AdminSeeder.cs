using DCSProject.Models;
using Microsoft.AspNetCore.Identity;

namespace DCSProject.Services;

/// <summary>
/// Builds the first admin account when the site starts.
/// The email and password comes from the user secrets so the are never committed
/// and keeps passwords out of the repo
/// </summary>
public static class AdminSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        IConfiguration config = services.GetRequiredService<IConfiguration>();
        ILogger logger = services.GetRequiredService<ILoggerFactory>().CreateLogger("AdminSeeder");

        string? email = config["SeedAdmin:Email"];
        string? password = config["SeedAdmin:Password"];

        // No admin setting on this computer, so nothing to do
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
        {
            logger.LogInformation("SeedAdmin settings not found, no admin account was created");
            return;
        }

        try
        {
            UserManager<ApplicationUser> userManager = services.GetRequiredService<UserManager<ApplicationUser>>();

            // Only create the account if it does not exist yet
            ApplicationUser? admin = await userManager.FindByEmailAsync(email);

            if (admin == null)
            {
                admin = new ApplicationUser
                {
                    UserName = email,
                    Email = email,
                    DisplayName = "Admin"
                };

                IdentityResult created = await userManager.CreateAsync(admin, password);

                if (!created.Succeeded)
                {
                    string errors = string.Join(", ", created.Errors.Select(e => e.Description));
                    logger.LogError("Could not create the Admin account: {Errors}", errors);
                    return;
                }

                // Make sure the account has the Admin role
                if (!await userManager.IsInRoleAsync(admin, "Admin"))
                {
                    await userManager.AddToRoleAsync(admin, "Admin");
                }
            }
        }
        catch (Exception ex)
        {
            // Most likely the database has not been created yet (Update-Database)
            logger.LogError(ex, "Could not create the Admin account. Has Update-Database been run?");
        }
    }
}
