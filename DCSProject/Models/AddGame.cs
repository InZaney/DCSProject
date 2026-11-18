using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// All information the admin types in to add a game into the catalog. Everything else is
/// populated from the Steam API call
/// </summary>
public class AddGame
{
    // Short name used to create the url
    [Required(ErrorMessage = "Enter a game name")]
    [StringLength(100)]
    [RegularExpression("^[A-Za-z0-9-]+$", ErrorMessage = "Use only letters, numbers, and dashes. No spaces please")]
    [Display(Name = "Game Name (used in the web address)")]
    public string GameName { get; set; } = "";

    [Required(ErrorMessage = "Enter the Steam app id")]
    [StringLength(20)]
    [RegularExpression("^[0-9]+$", ErrorMessage = "The Steam app id is numbers only")]
    [Display(Name = "Steam App Id")]
    public string SteamAppId { get; set; } = "";
}
