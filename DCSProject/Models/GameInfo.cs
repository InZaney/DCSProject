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
        /// Description of the game
        /// </summary>
        public required string Description { get; set; }

        /// <summary>
        /// The studio that created the game
        /// </summary>
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
        /// List of the games features
        /// </summary>
        public List<string> Features { get; set; } = new();
    }
}
