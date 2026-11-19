using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// Represents a single blog post
/// </summary>
[Index(nameof(DiscordMessageId), IsUnique = true)] // The same message can only be one post
public class BlogPost
{
    /// <summary>
    /// Unique Identifier for the post
    /// </summary>
    [Key]
    public int BlogPostId { get; set; }

    /// <summary>
    /// Title of the post
    /// </summary>
    [StringLength(100)]
    public required string Title { get; set; }

    /// <summary>
    /// The body of the post
    /// </summary>
    [DataType(DataType.MultilineText)]
    public required string BlogContent { get; set; }

    /// <summary>
    /// Author of the post
    /// </summary>
    [StringLength(50)]
    public required string Author { get; set; }

    /// <summary>
    /// Date of the Post
    /// </summary>
    [DataType(DataType.Date)]
    public DateOnly DatePosted { get; set; }

    /// <summary>
    /// Comments left on the post
    /// </summary>
    public List<Comment> Comments { get; set; } = new();

    /// <summary>
    /// Id of the discord message that the post originated from. Utilized for updating or removing the post
    /// </summary>
    [StringLength(30)]
    public string? DiscordMessageId { get; set; }
}
