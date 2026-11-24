using DCSProject.Data;
using DCSProject.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;

namespace DCSProject.Services;

/// <summary>
/// Gives the Blog page its posts from the database, and keeps the database up to date
/// with the private Discord channel.
/// A message that starts with "Title:" in that channel becomes a blog post
/// </summary>
public class BlogService
{
    private readonly DcsDb _db;
    private readonly DiscordService _discord;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BlogService> _logger;

    // How long to wait between checks of the Discord channel
    private static readonly TimeSpan RefreshAfter = TimeSpan.FromMinutes(1);

    // If Discord fails, wait this long before trying again
    private static readonly TimeSpan RetryAfter = TimeSpan.FromMinutes(1);

    // Name of the "we checked recently" note kept in the memory cache
    private const string SyncKey = "discord-blog-sync";

    // Only one visitor at a time may run a sync. Without this, two visitors arriving together could both
    // try to save the same new post
    private static readonly SemaphoreSlim SyncLock = new SemaphoreSlim(1, 1);

    public BlogService(DcsDb db, DiscordService discord, IMemoryCache cache, ILogger<BlogService> logger)
    {
        _db = db;
        _discord = discord;
        _cache = cache;
        _logger = logger;
    }

    /// <summary>
    /// Every blog post, newest first. Checks Discord first if it has been a while
    /// </summary>
    public async Task<List<BlogPost>> GetPostsAsync()
    {
        await SyncIfOldAsync();

        return await _db.BlogPosts
            .AsNoTracking()
            .OrderByDescending(p => p.DatePosted)
            .ThenByDescending(p => p.BlogPostId)
            .ToListAsync();
    }

    // Runs a sync only if we have not checked Discord recently
    private async Task SyncIfOldAsync()
    {
        // Discord is not set up on this computer, so the Blog page just shows what is in the database
        if (!_discord.IsConfigured)
        {
            return;
        }

        // Checked a moment ago
        if (_cache.TryGetValue(SyncKey, out _))
        {
            return;
        }

        await SyncLock.WaitAsync();
        try
        {
            // Another visitor may have finished a sync while we were waiting
            if (_cache.TryGetValue(SyncKey, out _))
            {
                return;
            }

            bool worked = await SyncFromDiscordAsync();

            // Leave a note so the next visitors skip the check. A failure is retried sooner
            _cache.Set(SyncKey, true, worked ? RefreshAfter : RetryAfter);
        }
        finally
        {
            SyncLock.Release();
        }
    }

    // Copies the Discord channel into the BlogPosts table. Returns false if nothing could be done
    private async Task<bool> SyncFromDiscordAsync()
    {
        List<DiscordMessage>? messages = await _discord.GetRecentMessagesAsync();

        // Discord failed. Change nothing
        if (messages == null)
        {
            return false;
        }

        // Safety check: no messages at all can mean the bot is missing the Read Message History permission.
        // Never delete posts because of this
        if (messages.Count == 0)
        {
            _logger.LogWarning("Discord returned no messages. Posts were not changed. Check the bot can read the channel history");
            return true;
        }

        // Safety check: every message having empty text means the Message Content intent is turned off.
        // Without this check every post would look deleted and be removed
        if (messages.All(m => string.IsNullOrEmpty(m.Content)))
        {
            _logger.LogError("Every Discord message came back empty. Turn on the Message Content intent for the bot (docs/DISCORD.md). Posts were not changed");
            return false;
        }

        // Step 1: work out which messages should be posts, keyed by the Discord message id
        Dictionary<string, ParsedPost> wanted = new Dictionary<string, ParsedPost>();

        foreach (DiscordMessage message in messages)
        {
            // Skip joins/pins/etc and anything written by a bot
            if (message.Type != 0 || message.Author?.Bot == true)
            {
                continue;
            }

            ParsedPost? post = ParsePost(message);

            if (post != null)
            {
                wanted[message.Id] = post;
            }
        }

        // Step 2: load the posts we already saved from Discord
        List<BlogPost> saved = await _db.BlogPosts.Where(p => p.DiscordMessageId != null).ToListAsync();
        Dictionary<string, BlogPost> savedById = saved.ToDictionary(p => p.DiscordMessageId!);

        // Step 3: add new posts and update ones that were edited.
        // EF only writes the columns that actually changed
        foreach (KeyValuePair<string, ParsedPost> item in wanted)
        {
            ParsedPost post = item.Value;

            if (savedById.TryGetValue(item.Key, out BlogPost? existing))
            {
                existing.Title = post.Title;
                existing.BlogContent = post.Body;
                existing.Author = post.Author;
                existing.DatePosted = post.Date;
            }
            else
            {
                _db.BlogPosts.Add(new BlogPost
                {
                    DiscordMessageId = item.Key,
                    Title = post.Title,
                    BlogContent = post.Body,
                    Author = post.Author,
                    DatePosted = post.Date
                });
            }
        }

        // Step 4: remove posts whose message was deleted (or edited so it no longer starts with Title:).
        // We only see the newest 100 messages, so older posts are left alone unless we saw the whole channel
        ulong oldestId = messages.Min(m => ulong.Parse(m.Id));
        bool sawWholeChannel = messages.Count < DiscordService.MessageLimit;
        int removed = 0;

        foreach (BlogPost post in saved)
        {
            if (wanted.ContainsKey(post.DiscordMessageId!))
            {
                continue;
            }

            bool insideWindow = sawWholeChannel || ulong.Parse(post.DiscordMessageId!) >= oldestId;

            if (insideWindow)
            {
                // Removing a post also removes its comments
                _db.BlogPosts.Remove(post);
                removed++;
            }
        }

        await _db.SaveChangesAsync();

        if (removed > 0)
        {
            _logger.LogInformation("Removed {Count} blog post(s) because the Discord message was deleted or changed", removed);
        }

        return true;
    }

    // Turns one Discord message into a blog post. Returns null if the message is not meant to be a post.
    // Format:   Title: My post title
    //           The rest of the message is the body
    private static ParsedPost? ParsePost(DiscordMessage message)
    {
        string content = (message.Content ?? "").Trim();
        string[] lines = content.Split('\n');

        string firstLine = lines[0].Trim();
        const string marker = "Title:";

        // Not a post. Normal chatter in the channel is ignored
        if (!firstLine.StartsWith(marker, StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        string title = firstLine.Substring(marker.Length).Trim();

        if (title.Length == 0)
        {
            return null;
        }

        // Keep within the database column sizes
        if (title.Length > 100)
        {
            title = title.Substring(0, 100);
        }

        // Everything after the first line is the body
        string body = string.Join("\n", lines.Skip(1)).Trim();

        string author = message.Author?.GlobalName ?? message.Author?.Username ?? "Deep Current Studio";
        if (author.Length > 50)
        {
            author = author.Substring(0, 50);
        }

        // The post date is the day the message was sent (UTC)
        DateTime date = message.Timestamp.UtcDateTime;

        return new ParsedPost(title, body, author, date);
    }

    // A post worked out from a message, before it is saved
    private record ParsedPost(string Title, string Body, string Author, DateTime Date);
}
