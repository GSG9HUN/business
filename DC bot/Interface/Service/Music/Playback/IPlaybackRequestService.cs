using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

/// <summary>
/// Defines the playback request service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IPlaybackRequestService
{

    /// <summary>
    /// Requests source loading and session-bound playback for a URL.
    /// </summary>
    /// <param name="voiceStateChannel">The Discord channel used by the operation.</param>
    /// <param name="url">The url supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> PlayAsyncUrl(
        IDiscordChannel voiceStateChannel,
        Uri url,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests source loading and session-bound playback for a search query.
    /// </summary>
    /// <param name="voiceStateChannel">The Discord channel used by the operation.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> PlayAsyncQuery(
        IDiscordChannel voiceStateChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default);
}
