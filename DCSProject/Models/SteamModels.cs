using System.Text.Json;
using System.Text.Json.Serialization;

namespace DCSProject.Models;

// These clases are built to match the JSON that steam returns from the request.
// Only listing the fields utilized on the site and ignoring all others

public class SteamAppResponse
{
    /// <summary>
    /// Gets or sets a value indicating whether the operation succeeded.
    /// </summary>
    [JsonPropertyName("success")]
    public bool Success { get; set; }

    /// <summary>
    /// Details for the game
    /// </summary>
    [JsonPropertyName("data")]
    public SteamAppData? Data { get; set; }
}

/// <summary>
/// Game Details recieved from steam
/// </summary>
public class SteamAppData
{
    /// <summary>
    /// Name of the game
    /// </summary>
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    /// <summary>
    /// Short description for the game
    /// </summary>
    [JsonPropertyName("short_description")]
    public string? ShortDescription { get; set; }

    /// <summary>
    /// The games developers
    /// </summary>
    [JsonPropertyName("developers")]
    public List<string>? Developers { get; set; }

    /// <summary>
    /// Url for the header image
    /// </summary>
    [JsonPropertyName("header_image")]
    public string? HeaderImage { get; set; }

    /// <summary>
    /// The date in which the game was released to play
    /// </summary>
    [JsonPropertyName("release_date")]
    public SteamReleaseDate? ReleaseDate { get; set; }

    [JsonPropertyName("price_overview")]
    public SteamPrice? PriceOverview { get; set; }

    [JsonPropertyName("platforms")]
    public SteamPlatforms? Platforms { get; set; }

    [JsonPropertyName("categories")]
    public List<SteamCategory>? Categories { get; set; }

    [JsonPropertyName("pc_requirements")]
    public JsonElement PcRequirements { get; set; }
}

public class SteamReleaseDate
{
    [JsonPropertyName("date")]
    public string? Date { get; set; }
}

public class SteamPrice
{
    [JsonPropertyName("final")]
    public int Final {  get; set; }
}

public class SteamPlatforms
{
    [JsonPropertyName("windows")]
    public bool Windows { get; set; }

    [JsonPropertyName("mac")]
    public bool Mac {  get; set; }

    [JsonPropertyName("linux")]
    public bool Linux { get; set; }
}

public class SteamCategory
{
    [JsonPropertyName("description")]
    public string? Description { get; set; }
}