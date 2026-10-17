using DCSProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace DCSProject.Controllers;

public class BlogController : Controller
{
    public IActionResult Index()
    {
        // Retrieve posts and return them by newest first
        List<BlogPost> posts = GetPost()
            .OrderByDescending(posts => posts.DatePosted)
            .ToList();

        // Send the list to the view
        return View(posts);
    }

    private List<BlogPost> GetPost()
    {
        return new List<BlogPost>()
        {
            // Sample post for presentation purposes
            new BlogPost()
            {
                // The production product will auto assign the key
                // This field will not me touched by the user
                BlogPostId = 1,
                Title = "Welcome to Deep Current studio",
                Author = "Deep Current Studio",
                DatePosted = new DateOnly(2026, 10, 1),
                BlogContent = "This is our first blog post to test the new website!"
            },

            new BlogPost()
            {
                BlogPostId = 2,
                Title = "Disaster Response Force available NOW!",
                Author = "Deep Current Studios",
                DatePosted= new DateOnly(2026, 10, 17),
                BlogContent = "Disaster Response Force is available to play right now on Steam! Go check" +
                "it out via the link on our games page!"
            },

            new BlogPost()
            {
                BlogPostId= 3,
                Title = "Whats next?!",
                Author = "Deep Current Studio",
                DatePosted = new DateOnly(2026, 10, 17),
                BlogContent = "We have some exciting news coming very soon! Be sure to keep an eye out here" +
                "and on our social media's so you do not miss it!"
            },

        };
    }
}
