using DCSProject.Models;
using DCSProject.Services;
using Microsoft.AspNetCore.Mvc;

namespace DCSProject.Controllers;

public class BlogController : Controller
{
    private readonly BlogService _blog;

    public BlogController(BlogService blog)
    {
        _blog = blog;
    }

    public async Task<IActionResult> Index()
    {
        // Posts come from the database (newest first). BlogService first checks Discord for new ones if it has been a while
        List<BlogPost> posts = await _blog.GetPostsAsync();

        // Send the list to the view
        return View(posts);
    }
}