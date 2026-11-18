using Microsoft.AspNetCore.Mvc;

namespace DCSProject.Controllers;

/// <summary>
/// Handles creating accounts, signing in and signing out
/// </summary>
public class AccountController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
