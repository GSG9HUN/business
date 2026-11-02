using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord voice state boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordVoiceState
{

    /// <summary>
    /// The Discord channel associated with this object.
    /// </summary>
    IDiscordChannel? Channel { get; }

    /// <summary>
    /// Returns the underlying DSharpPlus voice state represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    DiscordVoiceState ToDiscordVoiceState();
}
