using DCSProject.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DCSProject.Controllers;

/// <summary>
/// Handles creating accounts, signing in and signing out
/// </summary>
public class AccountController : Controller
{
    // UserManager creates and finds users. SignInManager checks passwords and handles the login cookie
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;

    // Displays the empty Register form to the user
    [HttpGet]
    public IActionResult Register()
    {
        // Check to see if they are already signed in
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToAction("Index", "Home");
        }

        return View(new RegisterViewModel());
    }

    // Runs when the register form is submitted
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        // Send the form back with error messages if requirements were not met
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Email utilized as the username
        ApplicationUser user = new ApplicationUser
        {
            UserName = model.Email,
            Email = model.Email,
            DisplayName = model.DisplayName
        };

        // Identity will check the password rules, hashes the password and saves the user
        IdentityResult result = await _userManager.CreateAsync(user, model.Password);

        if (!result.Succeeded)
        {
            // Show error messages
            foreach (IdentityError error in result.Errors)
            {
                ModelState.AddModelError("", error.Description);
            }
            return View(model);
        }

        // Everyone who register gets the User role. No one can pick admin
        await _userManager.AddToRoleAsync(user, "User");

        // Sign the user in right away
        await _signInManager.SignInAsync(user, isPersistent: false);

        return RedirectToAction("Index", "Home");
    }

    public IActionResult Index()
    {
        return View();
    }
}
