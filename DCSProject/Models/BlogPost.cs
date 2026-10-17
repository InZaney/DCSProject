using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// Represents a single blog post
/// </summary>
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
}
