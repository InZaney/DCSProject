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
    }

    /// <summary>
    /// Pulls one game by its GameName and returns null if there is no match
    /// </summary>
    public async Task<GameInfo?> GetGameAsync(string gameName)
    {
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
}
