using System.Text.Json.Serialization;

namespace DCSProject.Models;

// These classes match the parts of Discord's reply that we use. The [JsonPropertyName] names are what Discord sends.
// Fields we do not list are ignored.

/// <summary>
/// Singular message recieved from the Discord channel
/// </summary>
public class DiscordMessage
{
    /// <summary>
    /// Discords unique id for the message
    /// </summary>
    [JsonPropertyName("id")]
    public string Id { get; set; } = "";

    /// <summary>
    /// What was typed
    /// </summary>
    [JsonPropertyName("content")]
    public string? Content { get; set; }

    /// <summary>
    /// When the message was sent
    /// </summary>
    [JsonPropertyName("timestamp")]
    public DateTimeOffset Timestamp { get; set; }

    /// <summary>
    /// 0 is normal, other numbers are things like joins, pins and replies
    /// </summary>
    [JsonPropertyName("type")]
    public int Type { get; set; }

    [JsonPropertyName("author")]
    public DiscordAuthor? Author { get; set; }
}

/// <summary>
/// Diplay name of the person or bot who wrote the Discord message
/// </summary>
public class DiscordAuthor
{
    [JsonPropertyName("username")]
    public string? Username { get; set; }

    /// <summary>
    /// Display name of the poster
    /// </summary>
    [JsonPropertyName("global_name")]
    public string? GlobalName { get; set; }

    [JsonPropertyName("bot")]
    public bool? Bot { get; set; }
}
