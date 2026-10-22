namespace DCSProject.Models;

/// <summary>
/// Representative of one game in the studios catalog. Used to create catalog entrys
/// </summary>
public class GameEntry
{
    /// <summary>
    /// The name of the game we are adding into the catalog
    /// </summary>
    public required string GameName { get; set; }

    /// <summary>
    /// The unique Steam app id for the game. Used to make the api call later
    /// </summary>
    public required string SteamAppId { get; set; }

    /// <summary>
    /// Builds the information we type in for each game as a safety net incase Steam is down or unresponsive
    /// </summary>
    public required Func<GameInfo> CreateDefaults { get; set; }
}
