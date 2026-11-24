using DCSProject.Models;
using DCSProject.Services;
using Microsoft.AspNetCore.Authorization;
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

    // Shows the empty add game form only to Admin users
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public IActionResult Add()
    {
        return View(new AddGame());
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Add(AddGame model)
    {
        // Send the form back with error messages if rule is broken
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Check to ensure we are not adding a game that already exist
        if (await _games.GameExistsAsync(model.GameName, model.SteamAppId))
        {
            ModelState.AddModelError("", "A game with that name or Steam app id already exist");
            return View(model);
        }

        GameInfo? game = await _games.AddGameAsync(model.GameName, model.SteamAppId);

        if (game == null)
        {
            ModelState.AddModelError("SteamAppId", "Steam did not return a game for that app id. Please check it and try again");
            return View(model);
        }

        // Go to the new games page
        return RedirectToAction(nameof(Details), new { id = game.GameName });
    }

    // Displays the edit form to an admin with the games current name and SteamId
    [Authorize(Roles = "Admin")]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        GameInfo? game = await _games.GetGameByIdAsync(id);

        if (game == null)
        {
            return NotFound();
        }

        return View(new EditGame
        {
            GameId = game.GameId,
            GameName = game.GameName,
            SteamAppId = game.SteamAppId
        });
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(EditGame model)
    {
        // Send the form back with error messages if a rule on the view model was broken
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        GameInfo? existing = await _games.GetGameByIdAsync(model.GameId);

        if (existing == null)
        {
            return NotFound();
        }

        // Do not allow the same name or Steam id as ANOTHER game (the game itself is allowed to keep its own)
        if (await _games.GameExistsAsync(model.GameName, model.SteamAppId, model.GameId))
        {
            ModelState.AddModelError("", "Another game already uses that name or Steam app id.");
            return View(model);
        }

        // Save the change. Null means Steam had nothing for the new Steam id
        GameInfo? game = await _games.EditGameAsync(model.GameId, model.GameName, model.SteamAppId);

        if (game == null)
        {
            ModelState.AddModelError("SteamAppId", "Steam did not return a game for that app id. Check the number and try again.");
            return View(model);
        }

        TempData["Message"] = $"{game.Title} was updated.";
        return RedirectToAction(nameof(Details), new { id = game.GameName });
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
