using DCSProject.Data;
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
}
