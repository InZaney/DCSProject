using DCSProject.Data;
using DCSProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DCSProject.Services;

/// <summary>
/// This service will read games from the database and keep them up to date with Steam
/// The copy in the database is the backup that is utilized if Steam is down or unresponsive to the api call
/// </summary>
public class GameService
{
    private readonly DcsDb _db; // The project database
    private readonly SteamService _steam; // The steam API caller
    private readonly IMemoryCache _cache; // short-term memory kept in the servers ram

    // How old our database data can be before we ask Steam for it again
    // Set to 7 days to avoid hitting steam with multiple API calls. Could set to FromHours and pick a value
    // to help catch any flash sales
    private static readonly TimeSpan RefreshAfter = TimeSpan.FromDays(7);

    // If Steam fails or does not respond, defined time to wait before trying again
    private static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(5);

    public GameService(DcsDb db, SteamService steam, IMemoryCache cache)
    {
        _db = db;
        _steam = steam;
        _cache = cache;
    }

    /// <summary>
    /// Pulls every game from the catalog and refreshes any that are out of date
    /// </summary>
    public async Task<List<GameInfo>> GetAllGamesAsync()
    {
        await AddStarterGamesAsync();

        List<GameInfo> games = await _db.Games.OrderBy(g => g.Title).ToListAsync();

        // One at a time since dbcontext cannot do two things at once
        foreach (GameInfo game in games)
        {
            await RefreshIfOldAsync(game);
        }

        return games;
    }

    /// <summary>
    /// Pulls one game by its GameName and returns null if there is no match
    /// </summary>
    public async Task<GameInfo?> GetGameAsync(string gameName)
    {
        await AddStarterGamesAsync();

        // Sql will ignore casing when comparing by default
        GameInfo? game = await _db.Games.FirstOrDefaultAsync(g => g.GameName == gameName);

        if (game != null)
        {
            await RefreshIfOldAsync(game);
        }

        return game;
    }

    /// <summary>
    /// Counts how many games are in the catalog. Decider on if the games page shows "All Games" or not
    /// </summary>
    public async Task<int> GetGameCountAsync()
    {
        return await _db.Games.CountAsync();
    }

    /// <summary>
    /// Copies the games from the catalog into the database only if the table is empty
    /// </summary>
    private async Task AddStarterGamesAsync()
    {
        if (await _db.Games.AnyAsync())
        {
            return;
        }

        foreach (GameEntry entry in GameCatalog.All)
        {
            GameInfo game = entry.CreateDefaults();
            game.GameName = entry.GameName;
            game.SteamAppId = entry.SteamAppId;

            // Never checked yet, so the first visit will ask Steam
            game.LastUpdated = DateTime.MinValue;

            _db.Games.Add(game);
        }

        await _db.SaveChangesAsync();

    }

    /// <summary>
    /// Asks Steam for new data if the saved copy is old. If steam is down it will continue to use the saved copy
    /// </summary>
    private async Task RefreshIfOldAsync(GameInfo game)
    {
        // Saved copy is still good, do nothing
        // Determined by the set variable RefreshAfter declared above
        if (DateTime.UtcNow - game.LastUpdated < RefreshAfter)
        {
            return;
        }

        // Steam failed so we do not want to keep visitors waiting 
        string retryKey = "steam-retry-" + game.SteamAppId;
        if (_cache.TryGetValue(retryKey, out _))
        {
            // Steam failed so skip this game
            return;
        }

        // Get an answer from SteamService if Steam answered or not 
        bool steamAnswered = await _steam.ApplySteamDataAsync(game);

        if (steamAnswered)
        {
            game.LastUpdated = DateTime.UtcNow;
            await _db.SaveChangesAsync();
        } else
        {
            _cache.Set(retryKey, true, RetryAfter);
        }
    }

    // Check to see if game is already exist in the project. Returns true if it is
    public async Task<bool> GameExistsAsync(string gameName, string steamAppId)
    {
        return await _db.Games.AnyAsync(g => g.GameName == gameName || g.SteamAppId == steamAppId);
    }

    // Adds a new game to the catalog and Steam will fill in the details with the API call
    public async Task<GameInfo?> AddGameAsync(string gameName, string steampAppId)
    {
        // Placeholder text for the required fields. Steam overwrites what it knows
        GameInfo game = new GameInfo()
        {
            GameName = gameName,
            SteamAppId = steampAppId,
            Title = gameName,
            Description = "",
            Developer = "",
            Platform = "Not listed",
            PlayerCount = "Not listed",
            SteamUrl = "https://store.steampowered.com/app/" + steampAppId,
            HeaderImageUrl = ""
        };

        bool steamAnswered = await _steam.ApplySteamDataAsync(game);

        // No answer means the Steam id is probably wrong so do not save an empty game
        if (!steamAnswered)
        {
            return null;
        }

        game.LastUpdated = DateTime.UtcNow;
        _db.Games.Add(game);
        await _db.SaveChangesAsync();

        return game;
    }

    /// <summary>
    /// Gets one game from the database by its database id (for edit and delete)
    /// </summary>
    public async Task<GameInfo?> GetGameByIdAsync(int gameId)
    {
        return await _db.Games.FirstOrDefaultAsync(g => g.GameId == gameId);
    }

    /// <summary>
    /// Will return true if a game with that name or id already exist
    /// </summary>
    public async Task<bool> GameExistsAsync(string gameName, string steamAppId, int? excludeGameId = null)
    {
        return await _db.Games.AnyAsync(g =>
            (excludeGameId == null || g.GameId != excludeGameId) &&
            (g.GameName == gameName || g.SteamAppId == steamAppId));
    }

    /// <summary>
    /// Changes a games name or SteamId
    /// </summary>
    public async Task<GameInfo?> EditGameAsync(int gameId, string gameName, string steamAppId)
    {
        GameInfo? game = await _db.Games.FirstOrDefaultAsync(g => g.GameId == gameId);

        if (game == null)
        {
            return null;
        }

        bool steamIdChanged = game.SteamAppId != steamAppId;

        game.GameName = gameName;
        game.SteamAppId = steamAppId;

        if (steamIdChanged)
        {
            // It is a different game now, so clear the old game's details first.
            // Otherwise anything Steam leaves out (like the price of a free game) would keep the OLD game's value
            game.Title = gameName;
            game.Description = "";
            game.Developer = "";
            game.ReleaseDate = default;
            game.Price = 0;
            game.Platform = "Not listed";
            game.PlayerCount = "Not listed";
            game.SteamUrl = "https://store.steampowered.com/app/" + steamAppId;
            game.HeaderImageUrl = "";
            game.SystemRequirements = new List<string>();

            bool steamAnswered = await _steam.ApplySteamDataAsync(game);

            // No answer means the new Steam id is probably wrong. We return without saving, so the database keeps the old game
            if (!steamAnswered)
            {
                return null;
            }

            game.LastUpdated = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        // Takes the user to the newly updated game
        return game;
    }

}
