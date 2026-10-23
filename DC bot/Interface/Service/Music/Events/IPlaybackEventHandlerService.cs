using DC_bot.Interface.Discord;
using Lavalink4NET.Players;

namespace DC_bot.Interface.Service.Music.Events;

/// <summary>
/// Defines the playback event handler service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IPlaybackEventHandlerService
{

    /// <summary>
    /// Subscribes the current player to correlated end events while retiring previous subscriptions.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="connection">The lavalink player dependency used by this boundary.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    void RegisterPlaybackFinishedHandler(ulong guildId, ILavalinkPlayer connection, IDiscordChannel textChannel);

    /// <summary>
    /// Disables and removes current and retired track-end subscriptions for the guild.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task CleanupGuildAsync(ulong guildId);
}
