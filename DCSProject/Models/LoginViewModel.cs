using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// Information requested from the user to sign in
/// </summary>
public class LoginViewModel
{
    [Required(ErrorMessage = "Enter your email")]
    [EmailAddress(ErrorMessage = "Please enter a valid email")]
    public string Email { get; set; } = "";

    [Required(ErrorMessage = "Enter your password")]
    [DataType(DataType.Password)]
    public string Password { get; set; } = "";

    /// <summary>
    /// If true, the login cookie lasts after the browser is closed
    /// </summary>
    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
