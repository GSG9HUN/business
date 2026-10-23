using DSharpPlus.Entities;

namespace DC_bot.Interface.Discord;

/// <summary>
/// Defines the discord member boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IDiscordMember
{

    /// <summary>
    /// The Discord identifier of this object.
    /// </summary>
    ulong Id { get; }

    /// <summary>
    /// Whether the Discord account belongs to a bot.
    /// </summary>
    bool IsBot { get; }

    /// <summary>
    /// The Discord username.
    /// </summary>
    string Username { get; }

    /// <summary>
    /// The Discord mention text for this user.
    /// </summary>
    string Mention { get; }

    /// <summary>
    /// The member's current voice state, or null when unavailable.
    /// </summary>
    IDiscordVoiceState? VoiceState { get; }

    /// <summary>
    /// Returns the underlying DSharpPlus member represented by the adapter.
    /// </summary>
    /// <returns>The value produced for the supplied operation context.</returns>
    DiscordMember ToDiscordMember();
}
