using DC_bot.Interface.Discord;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Adapts discord channel data and operations to the bot&apos;s Discord boundary.
/// </summary>
/// <param name="discordChannel">The discord channel supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="guild">The guild supplied for this operation.</param>
public class DiscordChannelWrapper(
    DiscordChannel discordChannel,
    ILogger<DiscordChannelWrapper>? logger = null,
    DiscordGuild? guild = null) : IDiscordChannel
{
    private readonly ILogger<DiscordChannelWrapper> _logger = logger ?? NullLogger<DiscordChannelWrapper>.Instance;

    /// <inheritdoc />
    public ulong Id => discordChannel.Id;

    /// <inheritdoc />
    public string Name => discordChannel.Name;

    /// <summary>
    /// Sends a message through the wrapped Discord channel.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendMessageAsync(string message)
    {
        try
        {
            await discordChannel.SendMessageAsync(message);
        }
        catch (Exception ex)
        {
            _logger.MessageSendFailed(ex, "DiscordChannelWrapper.SendMessageAsync");
        }
    }

    /// <summary>
    /// Sends a message through the wrapped Discord channel.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SendMessageAsync(DiscordEmbed embed)
    {
        try
        {
            await discordChannel.SendMessageAsync(embed);
        }
        catch (Exception ex)
        {
            _logger.MessageSendFailed(ex, "DiscordChannelWrapper.SendMessageAsync");
        }
    }

    /// <inheritdoc />
    public IDiscordGuild Guild => new DiscordGuildWrapper(guild ?? discordChannel.Guild ?? throw new InvalidOperationException("Discord channel has no guild context."));

    /// <summary>
    /// Returns the underlying DSharpPlus channel represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    public DiscordChannel ToDiscordChannel()
    {
        return discordChannel;
    }
}
