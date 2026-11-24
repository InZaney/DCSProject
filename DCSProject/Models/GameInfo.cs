using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace DCSProject.Models;

/// <summary>
/// Represents a single game made by the studio
/// </summary>
[Index(nameof(GameName), IsUnique = true)] // No two games can share the same name
[Index(nameof(SteamAppId), IsUnique = true)] // No two games can have the same SteamId
public class GameInfo
{
    /// <summary>
    /// Unique Id for the game
    /// </summary>
    [Key]
    public int GameId { get; set; }

    /// <summary>
    /// Steam app Id used for the api call
    /// </summary>
    [StringLength(30)]
    public string SteamAppId { get; set; } = "";

    /// <summary>
    /// Used to keep track when the data was last updated
    /// </summary>
    public DateTime LastUpdated { get; set; }

    /// <summary>
    /// Name of the game
    /// </summary>
    [StringLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// Short name without spaces used for the page URL
    /// </summary>
    [StringLength(100)]
    public string GameName { get; set; } = "";

    /// <summary>
    /// Description of the game
    /// </summary>
    public required string Description { get; set; }

    /// <summary>
    /// The studio that created the game
    /// </summary>
    [StringLength(75)]
    public required string Developer { get; set; }

    /// <summary>
    /// When the game was or will be released
    /// </summary>
    [DataType(DataType.Date)]
    public DateOnly ReleaseDate { get; set; }

    /// <summary>
    /// Price of the game
    /// </summary>
    [DataType(DataType.Currency)]
    [Column(TypeName = "decimal(8, 2)")] // Up to 999,999.99
    public decimal Price {  get; set; }

    /// <summary>
    /// Platform that the game runs on
    /// </summary>
    public required string Platform { get; set; }

    /// <summary>
    /// Link to the games steam page
    /// </summary>
    [DataType(DataType.Url)]
    [StringLength(300)]
    public required string SteamUrl { get; set; }

    /// <summary>
    /// URL for the header image
    /// </summary>
    [DataType(DataType.ImageUrl)]
    public required string HeaderImageUrl { get; set; }

    /// <summary>
    /// List of the games features
    /// </summary>
    ///public List<string> Features { get; set; } = new();

    /// <summary>
    /// List of the required system specifications to play the game
    /// </summary>
    public List<string> SystemRequirements { get; set; } = new();

    /// <summary>
    /// The number of players the game supports
    /// </summary>
    [StringLength(200)]
    public required string PlayerCount { get; set; }
}
