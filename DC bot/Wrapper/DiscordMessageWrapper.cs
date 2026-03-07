using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Wrapper;

public class DiscordMessageWrapper(
    ulong id,
    string content,
    IDiscordChannel channel,
    IDiscordUser author,
    DateTimeOffset createdAt,
    List<DiscordEmbed> embeds,
    Func<string, Task<DiscordMessage>> responseAsync,
    Func<DiscordEmbed, Task<DiscordMessage>> responseEmbedAsync,
    ILogger<DiscordMessageWrapper>? logger = null)
    : IDiscordMessage
{
    private readonly ILogger<DiscordMessageWrapper> _logger = logger ?? NullLogger<DiscordMessageWrapper>.Instance;

    public ulong Id { get; set; } = id;
    public string Content { get; set; } = content;
    public IDiscordChannel Channel { get; set; } = channel;
    public IDiscordUser Author { get; set; } = author;
    public DateTimeOffset CreatedAt { get; set; } = createdAt;
    public IReadOnlyList<DiscordEmbed> Embeds { get; set; } = embeds;

    public async Task RespondAsync(string message)
    {
        try
        {
            await responseAsync(message);
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, "DiscordMessageWrapper.RespondAsync(string)");
        }
    }

    public async Task RespondAsync(DiscordEmbed embed)
    {
        try
        {
            await responseEmbedAsync(embed);
        }
        catch (Exception ex)
        {
            _logger.ResponseSendFailed(ex, "DiscordMessageWrapper.RespondAsync(embed)");
        }
    }
}