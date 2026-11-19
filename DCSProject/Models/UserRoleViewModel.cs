namespace DCSProject.Models;

/// <summary>
/// One row of the Manage Users table: a user and whether they are an Admin
/// </summary>
public class UserRoleViewModel
{
    public required string Id { get; set; }

    public string DisplayName { get; set; } = "";

    public string Email { get; set; } = "";

    /// <summary>
    /// True if the user has the Admin role
    /// </summary>
    public bool IsAdmin { get; set; }

    /// <summary>
    /// True if this row is the person looking at the page (they cannot change their own role)
    /// </summary>
    public bool IsCurrentUser { get; set; }
}