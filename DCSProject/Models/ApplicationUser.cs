using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

public class ApplicationUser : IdentityUser
{
    /// <summary>
    /// Name shown when the user comments
    /// </summary>
    [StringLength(50)]
    public string DisplayName { get; set; } = "";

    /// <summary>
    /// Comments from this user
    /// </summary>
    public List<Comment> Comments { get; set; } = new();
}
