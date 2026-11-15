using DCSProject.Models;
using Microsoft.AspNetCore.Identity;
using System.Reflection.Emit;
using System.Xml.Linq;

namespace DCSProject.Data;

/// <summary>
/// Inherits IdentityDbContext to give us the users, Roles, and related login tables 
/// </summary>
public class DcsDb : IdentityDbContext<ApplicationUser>
{
    public DcsDb(DbContextOptions<DcsDb> options) : base(options)
    {
    }

    /// <summary>
    /// Games table (Steam data stored as a backup)
    /// </summary>
    public DbSet<GameInfo> Games { get; set; } = null!;

    /// <summary>
    /// Blog posts table
    /// </summary>
    public DbSet<BlogPost> BlogPosts { get; set; } = null!;

    /// <summary>
    /// Comments table (each row links a blog post to a user)
    /// </summary>
    public DbSet<Comment> Comments { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Must be first: sets up the Identity tables
        base.OnModelCreating(modelBuilder);

        // Create the two roles the site uses. Fixed Ids keep the migration the same on every computer.
        // (The login issue will assign users to these roles.)
        modelBuilder.Entity<IdentityRole>().HasData(
            new IdentityRole { Id = "a1b2c3d4-0001-4000-8000-000000000001", Name = "Admin", NormalizedName = "ADMIN" },
            new IdentityRole { Id = "a1b2c3d4-0002-4000-8000-000000000002", Name = "User", NormalizedName = "USER" }
        );
    }
}