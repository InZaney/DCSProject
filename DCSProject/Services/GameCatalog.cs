using DCSProject.Models;

namespace DCSProject.Services;

/// <summary>
/// List of every game made by the studio
/// To Add a new game: Copy the template from the commented out section bellow
/// </summary>
public static class GameCatalog
{
    // Backup information used to build the page if Steam does not return anything or is down
    public static readonly List<GameEntry> All = new List<GameEntry>()
    {
        new GameEntry()
        {
            GameName = "DisasterResponseForce",
            SteamAppId = "4735850",
            CreateDefaults = () => new GameInfo()
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
                //Features = new List<string>()
                //    {
                //        ""
                //    },
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
            }
        }

        // Bellow is the template for adding a new game to the catalog

        /// <summary>
        /// new GameEntry() {
        /// {
        ///     GameName = "",
        ///     SteamAppId = "",
        ///     CreateDefaults = () => GameInfo()
        ///     {
        ///         Title = "",
        ///         Description = "",
        ///         Developer = "",
        ///         ReleaseDate = "",
        ///         Price = 0.00m,
        ///         Platform = "",
        ///         SteamUrl = "",
        ///         HeaderImageUrl = "",
        ///         Features = new List<string>()
        ///         {
        ///             "",
        ///             "",
        ///             ""
        ///         },
        ///         SystemRequirements = new List<string>()
        ///         {
        ///             "OS:",
        ///             "Processor:",
        ///             "Memory:",
        ///             "Graphics:",
        ///             "DirectX:"
        ///         },
        ///         PlayerCount = ""
        ///     }
        /// }
        /// </summary>
    };
}
