using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Service.Music.Lifecycle.Models;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

/// <summary>
/// Defines the track playback service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ITrackPlaybackService
{

    /// <summary>Enqueues loaded tracks while retaining separate Discord and mobile requester labels.</summary>
    /// <param name="searchQuery">Loaded tracks to enqueue.</param>
    /// <param name="session">Current player generation.</param>
    /// <param name="textChannel">Response channel.</param>
    /// <param name="requestedBy">Discord requester label or mention.</param>
    /// <param name="sourceQuery">Reloadable source query.</param>
    /// <param name="sourceSearchMode">Source provider search mode.</param>
    /// <param name="cancellationToken">Cancels supported queue and playback work.</param>
    /// <param name="requestedByDisplayName">Plain requester name captured for mobile read models.</param>
    /// <returns>The outcome of the captured enqueue or playback request.</returns>
    Task<PlaybackStartResult> PlayTheFoundMusicAsync(
        TrackLoadResult searchQuery,
        PlaybackSession session,
        IDiscordChannel textChannel,
        string? requestedBy,
        string? sourceQuery,
        TrackSearchMode? sourceSearchMode,
        CancellationToken cancellationToken = default,
        string? requestedByDisplayName = null);

    /// <summary>
    /// Claims and starts the next waiting track without blindly retrying uncertain remote dispatch.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> TryPlayNextTrackAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        PlaybackStartReason reason = PlaybackStartReason.QueueNext,
        ControlReservation? reservation = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the specified durable queue item under the supplied or newly acquired reservation.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="queueItemId">The durable queue-item identity used for playback and persistence correlation.</param>
    /// <param name="reason">The reason supplied for this operation.</param>
    /// <param name="replacement">The lava link track? dependency used by this boundary.</param>
    /// <param name="resume">The optional confirmed position and pause state used when restoring the same queue item.</param>
    /// <param name="reservation">The caller&apos;s exclusive control reservation, when one is supplied.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> StartExistingAsync(
        PlaybackSession session,
        IDiscordChannel textChannel,
        long queueItemId,
        PlaybackStartReason reason,
        ILavaLinkTrack? replacement = null,
        ControlReservation? reservation = null,
        PlaybackResumeOptions? resume = null,
        CancellationToken cancellationToken = default);

    /// <summary>Starts a captured waiting occurrence only if its complete waiting order remains unchanged.</summary>
    Task<PlaybackStartResult> StartQueueSelectionAsync(
        PlaybackSession session,
        IDiscordChannel channel,
        long targetId,
        IReadOnlyList<long> expectedIds,
        ControlReservation reservation,
        CancellationToken cancellationToken = default);

    /// <summary>Enqueues loaded tracks while retaining separate Discord and mobile requester labels.</summary>
    /// <param name="loaded">Loaded tracks to insert next.</param>
    /// <param name="session">Current player generation.</param>
    /// <param name="channel">Response channel.</param>
    /// <param name="query">Reloadable source query.</param>
    /// <param name="searchMode">Source provider search mode.</param>
    /// <param name="requestedBy">Discord requester label or mention.</param>
    /// <param name="cancellationToken">Cancels supported queue and playback work.</param>
    /// <param name="requestedByDisplayName">Plain requester name captured for mobile read models.</param>
    /// <returns>The outcome of the captured enqueue or playback request.</returns>
    Task<PlaybackStartResult> PlayNextLoadedAsync(
        TrackLoadResult loaded,
        PlaybackSession session,
        IDiscordChannel channel,
        string query,
        TrackSearchMode searchMode,
        string requestedBy,
        CancellationToken cancellationToken = default,
        string? requestedByDisplayName = null);

}
