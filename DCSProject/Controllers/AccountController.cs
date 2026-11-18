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

    public AccountController(UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
    }

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

    // Shows the empty login form to the user
    [HttpGet]
    public IActionResult Login(string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;
        return View(new LoginViewModel());
    }

    // Runs when login is submitted and checks the provided details
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
    {
        ViewData["ReturnUrl"] = returnUrl;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Users email is the username. LockoutOnFailure counts wrong passwords so the account can be locked
        var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.RememberMe, lockoutOnFailure: true);

        if (result.Succeeded)
        {
            // Only go back to the page they wanted to if it is on our site
            // Will stop links that sends users elsewhere
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        if (result.IsLockedOut)
        {
            ModelState.AddModelError("", "Too many failed attempts. Please try again in 15 minutes");
            return View(model);
        }

        // Same message used for wrong email or password. This stops phishing attempts for what emails have accounts
        ModelState.AddModelError("", "Invalid email or password");
        return View(model);
    }

    // Signs the user out of the site
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await _signInManager.SignOutAsync();
        return RedirectToAction("Index", "Home");
    }

    // Shown if a signed in user reaches a page their role is not allowed
    [HttpGet]
    public IActionResult AccessDenied()
    {
        return View();
    }
}
