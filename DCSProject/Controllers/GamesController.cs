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
    private readonly GameService _games;

    public GamesController(GameService games)
    {
        _games = games;
    }

    public async Task<IActionResult> Index()
    {
        // Pull every game from the database 
        List<GameInfo> games = await _games.GetAllGamesAsync();

        // Check to see if there is more than one game present
        if (games.Count == 1)
        {
            return RedirectToAction(nameof(Details), new {id = games[0].GameName });
        }

        // More than one game so display a list to the user
        return View(games);
    }

    public async Task<IActionResult> Details(string id)
    {
        // Find the entry in the database with that id(GameName)
        GameInfo? game = await _games.GetGameAsync(id);

        // If no game match is found, display an error to the user
        if (game == null)
        {
            return NotFound();
        }

        // On the games detail page, a back to list link only appears if there is more than one game in the catalog
        ViewBag.MoreThanOneGame = await _games.GetGameCountAsync() > 1;

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
