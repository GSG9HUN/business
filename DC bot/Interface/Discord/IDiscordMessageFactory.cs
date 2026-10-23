using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord message factory boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordMessageFactory
{

    /// <summary>
    /// Constructs the adapter or runtime object from supplied context and dependencies.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <param name="author">The author supplied for this operation.</param>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    IDiscordMessage Create(DiscordMessage message, DiscordChannel channel, DiscordUser author, DiscordGuild? guild = null);
}
