using DC_bot.Interface.Discord;
using DC_bot.Wrapper.Discord;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Helper.Factory;

/// <summary>
/// Creates message adapters at the DSharpPlus command boundary.
/// </summary>
public class DiscordMessageWrapperFactory : IDiscordMessageFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <param name="author">The author supplied for this operation.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public static IDiscordMessage Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        ILogger<DiscordMessageWrapper>? logger = null,
        DiscordGuild? guild = null)
    {
        var discordAuthor = new DiscordUserWrapper(author);
        var discordChannel = new DiscordChannelWrapper(channel, guild: guild);
        return new DiscordMessageWrapper(message.Id, message.Content, discordChannel, discordAuthor, message.CreationTimestamp, message.Embeds.ToList(), message.RespondAsync, message.RespondAsync, builder => message.ModifyAsync(builder), logger);
    }

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <param name="author">The author supplied for this operation.</param>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    IDiscordMessage IDiscordMessageFactory.Create(
        DiscordMessage message,
        DiscordChannel channel,
        DiscordUser author,
        DiscordGuild? guild)
    {
        return Create(message, channel, author, guild: guild);
    }
}
