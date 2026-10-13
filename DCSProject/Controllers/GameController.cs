using DCSProject.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace DCSProject.Controllers;

public class GameController : Controller
{
    public IActionResult Index()
    {
        return View();
    }

    public IActionResult DisasterResponseForce()
    {
        GameInfo game = new GameInfo()
        {
            Title = "Disaster Response Force",
            Description = "Disaster Response Force is a twin-stick 2D top-down hack-and-slash arcade game. " +
                    "You play as one of three characters, a responder sent to hold back pink, " +
                    "industrial-runoff-corrupted enemies on the dying factory planet POL-XIIV while " +
                    "civilians are evacuated. Survive as long as possible and score as high as you can.",
            Developer = "Deep Current Studio",
            ReleaseDate = new DateOnly(2026, 9, 4),
            // the "m" tells C# this number is a decimal
            Price = 4.99m, 
            Platform = "Windows",
            SteamUrl = "https://store.steampowered.com/app/4735850/Disaster_Response_Force/",
            // Image is hosted by Steam for now. Later we can download it and use a local file.
            HeaderImageUrl = "https://shared.fastly.steamstatic.com/store_item_assets/steam/apps/4735850/2b4c823182c37f0117302f40904db2873407c40c/header.jpg",
            Features = new List<string>()
                {
                    "Three playable characters, each with different weapons",
                    "In-field upgrades that improve your character",
                    "A vending machine that offers upgrades between areas",
                    "Enemies get stronger the deeper you go into the city",
                    "Score-based gameplay focused on kills and points",
                    "Steam Achievements"
                },
            // Minimum requirements to run the game
            SystemRequirements = new List<string>()
                {
                    "OS: Windows 10",
                    "Processor: Intel Core i3-3220",
                    "Memory: 4 MB RAM",
                    "Graphics: Integrated graphics",
                    "DirectX: Version 10"
                },
            PlayerCount = "Single-player",
        };

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
