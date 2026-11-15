using System.ComponentModel.DataAnnotations;

namespace DCSProject.Models;

/// <summary>
/// Representative of a singular comment from a signed in user
/// </summary>
public class Comment
{
    /// <summary>
    /// Unique ID for comment
    /// </summary>
    [Key]
    public int CommentId { get; set; }

    /// <summary>
    /// The content for the comment
    /// </summary>
    [StringLength(1000)]
    public required string Content { get; set; }

    /// <summary>
    /// When the comment was posted
    /// </summary>
    [DataType(DataType.DateTime)]
    public DateTime DatePosted { get; set; }

    /// <summary>
    /// Id for the post the comment was left on
    /// </summary>
    public int BlogPostId { get; set; }

    /// <summary>
    /// The post the comment belongs to 
    /// </summary>
    public BlogPost? BlogPost { get; set; }

    /// <summary>
    /// Id of the user who wrote the comment (foreign key)
    /// It is a string since IdentityUser utilizes a string Id
    /// </summary>
    public required string UserId { get; set; }

    /// <summary>
    /// User who wrote the commment
    /// </summary>
    public ApplicationUser? User { get; set; }
}
