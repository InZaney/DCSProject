using System.Runtime.CompilerServices;
using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models
{
    /// <summary>
    /// Represents a single game made by the studio
    /// </summary>
    public class GameInfo
    {
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
        public decimal Price {  get; set; }

        /// <summary>
        /// Platform that the game runs on
        /// </summary>
        public required string Platform { get; set; }

        /// <summary>
        /// Link to the games steam page
        /// </summary>
        [DataType(DataType.Url)]
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
        [StringLength(75)]
        public required string PlayerCount { get; set; }
    }
}
