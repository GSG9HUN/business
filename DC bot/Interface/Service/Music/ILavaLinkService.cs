using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Interface.Service.Music;

/// <summary>
/// Defines the lava link service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface ILavaLinkService
{

    /// <summary>
    /// Pauses the validated player and persists playback position and pause state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackControlResult> PauseAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests URL playback using the supplied Discord voice and response context.
    /// </summary>
    /// <param name="toDiscordChannel">The Discord channel used by the operation.</param>
    /// <param name="result">The result supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> PlayAsyncUrl(
        IDiscordChannel toDiscordChannel,
        Uri result,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Requests query playback using the supplied Discord voice and response context.
    /// </summary>
    /// <param name="toDiscordChannel">The Discord channel used by the operation.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> PlayAsyncQuery(
        IDiscordChannel toDiscordChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task ConnectAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Stops and completes the current attempt, then advances without relying on a later SDK Stopped event.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackControlResult> SkipAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Starts the preceding canonical item in the active batch, reporting when no earlier item is available.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackControlResult> PreviousAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Resumes the validated player and persists updated playback state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackControlResult> ResumeAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Initializes guild-specific persisted music state for subsequent commands.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task Init(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Raised when a track-start notification is published to subscribers.
    /// </summary>
    event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted;

    /// <summary>
    /// Requests a validated player connection and starts the next waiting queue item.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    Task<PlaybackStartResult> StartPlayingQueue(
        IDiscordMessage message,
        IDiscordChannel textChannel,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);

    /// <summary>
    /// Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    Task LeaveVoiceChannel(IDiscordMessage message, IDiscordMember? member, CancellationToken cancellationToken = default);
}
