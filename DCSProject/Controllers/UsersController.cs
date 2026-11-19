using DCSProject.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace DCSProject.Controllers;

/// <summary>
/// Lets Admins see every user and give or remove the Admin role.
/// The attribute on the class means EVERY action below is Admins only
/// </summary>
[Authorize(Roles = "Admin")]
public class UsersController : Controller
{
    private readonly UserManager<ApplicationUser> _userManager;

    public UsersController(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    // Shows the table of users
    public async Task<IActionResult> Index()
    {
        List<ApplicationUser> users = await _userManager.Users.OrderBy(u => u.Email).ToListAsync();

        // One query to get everyone who is an Admin, then check each user against that list
        IList<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync("Admin");
        HashSet<string> adminIds = admins.Select(a => a.Id).ToHashSet();

        string? currentUserId = _userManager.GetUserId(User);

        List<UserRoleViewModel> rows = users.Select(u => new UserRoleViewModel
        {
            Id = u.Id,
            DisplayName = u.DisplayName,
            Email = u.Email ?? "",
            IsAdmin = adminIds.Contains(u.Id),
            IsCurrentUser = u.Id == currentUserId
        }).ToList();

        return View(rows);
    }

    // Gives a user the Admin role
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MakeAdmin(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        if (!await _userManager.IsInRoleAsync(user, "Admin"))
        {
            await _userManager.AddToRoleAsync(user, "Admin");
        }

        TempData["Message"] = $"{user.Email} is now an Admin.";
        return RedirectToAction(nameof(Index));
    }

    // Takes the Admin role away from a user
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> RemoveAdmin(string id)
    {
        ApplicationUser? user = await _userManager.FindByIdAsync(id);

        if (user == null)
        {
            return NotFound();
        }

        // Rule 1: you cannot remove your own Admin role by accident
        if (user.Id == _userManager.GetUserId(User))
        {
            TempData["Error"] = "You cannot remove your own Admin role.";
            return RedirectToAction(nameof(Index));
        }

        // Rule 2: there must always be at least one Admin left
        IList<ApplicationUser> admins = await _userManager.GetUsersInRoleAsync("Admin");
        if (admins.Count <= 1)
        {
            TempData["Error"] = "You cannot remove the last Admin.";
            return RedirectToAction(nameof(Index));
        }

        await _userManager.RemoveFromRoleAsync(user, "Admin");

        // Makes the removed Admin's login cookie invalid, so they must sign in again and lose access
        await _userManager.UpdateSecurityStampAsync(user);

        TempData["Message"] = $"{user.Email} is no longer an Admin.";
        return RedirectToAction(nameof(Index));
    }
}
