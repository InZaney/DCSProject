using DCSProject.Models;
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
    // The steam id of Disaster Response Force
    private const string SteamAppId = "4735850";

    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public GamesController(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public IActionResult Index()
    {
        return View();
    }

    public async Task<IActionResult> DisasterResponseForce()
    {
        // Hard-coded info and backup if steam is down
        GameInfo game = BuildDefaultGame();

        // Make request to steam
        SteamAppData? steam = await GetSteamDataAsync();

        // If reply recieved from steam, overwrite hard-coded data
        if (steam == null)
        {
            game.Title = steam.Name ?? game.Title;
            game.Description = steam.ShortDescription ?? game.Description;
            game.Developer = steam.Developers?.FirstOrDefault() ?? game.Developer;
            game.HeaderImageUrl = steam.HeaderImage ?? game.HeaderImageUrl;

            // Transition steam date from text to dateonly
            if (steam.ReleaseDate?.Date != null && DateOnly.TryParse(steam.ReleaseDate.Date, new CultureInfo("en-US"), DateTimeStyles.None, out DateOnly releaseDate))
            {
                game.ReleaseDate = releaseDate;
            }

            // Convert the cents price steam sends (divide by 100 and add "m" to make it a decimal)
            if (steam.PriceOverview != null)
            {
                game.Price = steam.PriceOverview.Final / 100m;
            }

            // Steam send 3 true/false values for platforms. Building text instead
            if (steam.Platforms != null)
            {
                List<string> platforms = new List<string>();
                if (steam.Platforms.Windows) platforms.Add("Windows");
                if (steam.Platforms.Mac) platforms.Add("Mac");
                if (steam.Platforms.Linux) platforms.Add("Linux");

                if (platforms.Count > 0)
                {
                    game.Platform = string.Join(", ", platforms);
                }
            }

            // Player count: Keep the Steam categories that talk about players and join them into one text
            if (steam.Categories != null)
            {
                List<string> playerTags = steam.Categories.Select(c => c.Description ?? "")
                                                          .Where(d => d.Contains("Player", StringComparison.OrdinalIgnoreCase)
                                                          || d.Contains("co-op", StringComparison.OrdinalIgnoreCase))
                                                          .ToList();
                if (playerTags.Count > 0)
                {
                    game.PlayerCount = string.Join(", ", playerTags);
                }
            }

            // System requirements. Converted from html to plain text
            List<string> requirements = ParseRequirements(steam.PcRequirements);
            if (requirements.Count > 0)
            {
                game.SystemRequirements = requirements;
            }
        }
        return View(game);
    }

    // Backup information used to build the page if Steam does not return anything or is down
    private GameInfo BuildDefaultGame()
    {
        return new GameInfo()
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
    }

    // Pulls the games details from Steam or cache if recently asked. Returns null if no answer
    private async Task<SteamAppData?> GetSteamDataAsync()
    {
        string cachekey = "steam-" + SteamAppId;

        // If already saved from recent request
        if (_cache.TryGetValue(cachekey, out SteamAppData? cached))
        {
            return cached;
        }

        try
        {
            string url = $"https://store.steampowered.com/api/appdetails?appids={SteamAppId}&cc=us&l=english";

            var response = await _http.GetFromJsonAsync<Dictionary<string, SteamAppResponse>>(url);

            if (response != null && response.TryGetValue(SteamAppId, out SteamAppResponse? entry) &&
                entry.Success && entry.Data != null)
            {
                // Cache it for 1 hour and only save real answers
                _cache.Set(cachekey, entry.Data, TimeSpan.FromHours(1));
                return entry.Data;
            }
        }
        catch (Exception ex) when (ex is HttpRequestException || ex is JsonException || ex is TaskCanceledException)
        {
            // Steam was down, slow, or sent something odd. Fall through and return null
            // so the page still works with the hard-coded info.
        }

        return null;
    }

    // This cleans the way Steam sends the requirements and returns them to neat lines
    private List<string> ParseRequirements(JsonElement requirements)
    {
        List<string> lines = new List<string>();

        //Steam will send [] when there are none, this is a check to continue if real values present
        if (requirements.ValueKind != JsonValueKind.Object || !requirements.TryGetProperty("minimum", out JsonElement minimum) ||
            minimum.ValueKind != JsonValueKind.String)
        {
            return lines;
        }

        string html = minimum.GetString() ?? "";

        // Go through and find every <li>...<li>
        foreach (Match match in Regex.Matches(html, "<li>(.*?)<li>", RegexOptions.Singleline))
        {
            // Remove the tags (<strong>,<br>)
            string text = Regex.Replace(match.Groups[1].Value, "<[^>]+>", "");
            text = WebUtility.HtmlDecode(text).Trim();

            if (text.Length > 0)
            {
                lines.Add(text);
            }
        }
        return lines;
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
