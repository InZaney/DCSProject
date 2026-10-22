using DCSProject.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.Globalization;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DCSProject.Services;

/// <summary>
/// Builds game info for any games contained within the catalog
/// </summary>
public class SteamService
{
    private readonly HttpClient _http;
    private readonly IMemoryCache _cache;

    public SteamService(HttpClient http, IMemoryCache cache)
    {
        _http = http;
        _cache = cache;
    }

    public async Task<GameInfo> GetGameAsync(GameEntry entry)
    {
        // Hard-coded information and backup if steam is down
        GameInfo game = entry.CreateDefaults();

        // Make request to steam
        SteamAppData? steam = await GetSteamDataAsync(entry.SteamAppId);

        // If reply received from steam, overwrite hard-coded data
        if (steam != null)
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
        return game;
    }

    // Pulls the games details from Steam or cache if recently asked. Returns null if no answer
    private async Task<SteamAppData?> GetSteamDataAsync(string appId)
    {
        string cachekey = "steam-" + appId;

        // If already saved from recent request
        if (_cache.TryGetValue(cachekey, out SteamAppData? cached))
        {
            return cached;
        }

        try
        {
            string url = $"https://store.steampowered.com/api/appdetails?appids={appId}&cc=us&l=english";

            var response = await _http.GetFromJsonAsync<Dictionary<string, SteamAppResponse>>(url);

            if (response != null && response.TryGetValue(appId, out SteamAppResponse? entry) &&
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
        foreach (Match match in Regex.Matches(html, "<li>(.*?)</li>", RegexOptions.Singleline))
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
}
