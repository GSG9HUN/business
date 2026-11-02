using DC_bot.Interface.Discord;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Models;
using Lavalink4NET.Events.Players;

namespace DC_bot.Interface.Service.Music.Events;

/// <summary>
/// Defines the track ended handler service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackEndedHandlerService
{

    /// <summary>
    /// Completes the owned queue attempt, then reloads, repeats, advances, or closes the batch for its end reason.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="operationId">The correlation identifier of the playback or command operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task HandleTrackEndedAsync(
        PlaybackSession session,
        Guid operationId,
        long queueItemId,
        TrackEndedEventArgs args,
        IDiscordChannel textChannel);
}
