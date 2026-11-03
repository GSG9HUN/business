using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord channel boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordChannel
{

    /// <summary>
    /// The Discord identifier of this object.
    /// </summary>
    ulong Id { get; }

    /// <summary>
    /// The display name.
    /// </summary>
    string Name { get; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    IDiscordGuild Guild { get; }

    /// <summary>
    /// Sends a message through the wrapped Discord channel.
    /// </summary>
    /// <param name="message">The message supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendMessageAsync(string message);

    /// <summary>
    /// Sends a message through the wrapped Discord channel.
    /// </summary>
    /// <param name="embed">The embed supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task SendMessageAsync(DiscordEmbed embed);

    /// <summary>
    /// Returns the underlying DSharpPlus channel represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    DiscordChannel ToDiscordChannel();
}
