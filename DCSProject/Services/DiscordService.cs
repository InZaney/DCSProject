using System.Net.Http.Headers;
using DCSProject.Models;

namespace DCSProject.Services;

public class DiscordService
{
    // Discord returns at most 100 messages per reques
    public const int MessageLimit = 100;

    private readonly HttpClient _http;
    private readonly ILogger<DiscordService> _logger;
    private readonly string? _channelId;
    private readonly bool _hasToken;

    public DiscordService(HttpClient http, IConfiguration config, ILogger<DiscordService> logger)
    {
        _http = http;
        _logger = logger;

        // Both values come from User Secrets to keep from ever being commited
        string? token = config["Discord:BotToken"];
        _channelId = config["Discord:ChannelId"];

        if (!string.IsNullOrWhiteSpace(token))
        {
            // How the bot authorizes itself to Discord
            _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bot", token);
            _hasToken = true;
        }
    }

    /// <summary>
    /// True when the bot has a token and channel id set. If false the discord feed is switched off
    /// </summary>
    public bool IsConfigured => _hasToken && ulong.TryParse(_channelId, out _);

    public async Task<List<DiscordMessage>?> GetRecentMessagesAsync()
    {
        if (!IsConfigured)
        {
            return null;
        }

        try
        {
            HttpResponseMessage response = await _http.GetAsync($"channels/{_channelId}/messages?limit={MessageLimit}");

            if (!response.IsSuccessStatusCode)
            {
                // Number will tell us what is wrong
                _logger.LogWarning("Discord returned {StatusCode} when reading the blog channel. 401 = wrong bot token," +
                    "403 = bot cannot see the channel, 404 = wrong channel id or bot not in the server, 429 = asked too often",
                    (int)response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<List<DiscordMessage>>();
        } catch (Exception ex) when (ex is HttpRequestException || ex is System.Text.Json.JsonException || ex is TaskCanceledException)
        {
            // Discord was down, slow or sent something odd. The saved post will keep working
            _logger.LogWarning(ex, "Could not read messages from Discord");
            return null;
        }
    }
}
