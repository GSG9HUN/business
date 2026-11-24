using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Tracks;
using DSharpPlus.Entities;

namespace DC_bot.Interface.Service.Music.Presentation;

/// <summary>
/// Defines the track notification service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackNotificationService
{

    /// <summary>Sends background skip/empty-channel results with contextual styling and bounded text fallback.</summary>
    Task NotifyMusicControlAsync(
        DiscordChannel channel,
        ulong guildId,
        string text,
        bool success,
        CancellationToken cancellationToken = default);

    /// <summary>Sends a localized failure or user condition with the shared response severity classification.</summary>
    Task SendValidationAsync(IDiscordChannel channel, string errorKey, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raised when a track-start notification is published to subscribers.
    /// </summary>
    public event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted;

    /// <summary>
    /// Publishes started-track details to the selected response channel.
    /// </summary>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="track">The track whose identity and metadata are used by the operation.</param>
    /// <param name="position">The position supplied for this operation.</param>
    /// <param name="duration">The duration supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task NotifyNowPlayingAsync(
        IDiscordChannel textChannel,
        ILavaLinkTrack track,
        TimeSpan position,
        TimeSpan duration,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Sends the localized queue-finished notification to the selected response channel.
    /// </summary>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task NotifyQueueEmptyAsync(IDiscordChannel textChannel, CancellationToken cancellationToken = default);

    /// <summary>
    /// Attempts a Discord notification and logs delivery failures without failing playback.
    /// </summary>
    /// <param name="channel">The Discord channel used by the operation.</param>
    /// <param name="message">The message supplied for this operation.</param>
    /// <param name="operation">The operation supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task SendSafeAsync(
        IDiscordChannel channel,
        string message,
        string operation,
        CancellationToken cancellationToken = default);
}
