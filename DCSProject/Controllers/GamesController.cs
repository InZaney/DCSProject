using DCSProject.Models;
using DCSProject.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DCSProject.Controllers;

public class GamesController : Controller
{
    private readonly SteamService _steam;

    public GamesController(SteamService steam)
    {
        _steam = steam;
    }

    public async Task<IActionResult> Index()
    {
        // Check to see if there is more than one game present
        if (GameCatalog.All.Count == 1)
        {
            return RedirectToAction(nameof(Details), new {id = GameCatalog.All[0].GameName });
        }

        // More than one game so display a list to the user
        IEnumerable<Task<GameInfo>> tasks = GameCatalog.All.Select(entry => _steam.GetGameAsync(entry));
        GameInfo[] games = await Task.WhenAll(tasks);

        // Send the list
        return View(games.ToList());
    }

    public async Task<IActionResult> Details(string id)
    {
        // Find the entry in the catalog with that id(GameName)
        GameEntry? entry = GameCatalog.All.FirstOrDefault(g => string.Equals(g.GameName, id, StringComparison.OrdinalIgnoreCase));

        // If no game match is found, display an error to the user
        if (entry == null)
        {
            return NotFound();
        }

        // Pass the games SteamId into the API call and build the games info for the page
        GameInfo game = await _steam.GetGameAsync(entry);

        // On the games detail page, a back to list link only appears if there is more than one game in the catalog
        ViewBag.MoreThanOneGame = GameCatalog.All.Count > 1;

        return View(game);
    }



    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
