using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// Information requested from a user when creating an account
/// </summary>
public class RegisterViewModel
{
    /// <summary>
    /// Name that will be shown when the user post a comment
    /// </summary>
    [Required(ErrorMessage = "Enter a display name")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "Display name must be 2 to 50 characters")]
    [Display(Name = "Display Name")]
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Used to sign in
    /// </summary>
    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    [StringLength(250)]
    public string Email { get; set; } = "";

    /// <summary>
    /// Identity saves only a hashed version
    /// </summary>
    [Required(ErrorMessage = " Enter a password")]
    [StringLength(100, MinimumLength = 8, ErrorMessage = "Password must be atleast 8 characters long")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    /// <summary>
    /// Typed a second time to confirm and catch any potential mistakes
    /// </summary>
    [Required(ErrorMessage = " Confirm your password")]
    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "The passwords do not match")]
    [Display(Name = "Confirm Password")]
    public string ConfirmPassword { get; set; } = "";
}
