using Microsoft.AspNetCore.Mvc;

namespace DCSProject.Controllers;

public class BlogController : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
