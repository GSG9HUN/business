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
    /// <summary>Shows, lists, resets, or selects complete filter configurations.</summary>
    /// <param name="message">The guild message receiving the localized result.</param>
    /// <param name="member">The requesting member; required for changes.</param>
    /// <param name="arguments">The invariant command argument, or null for inspection.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed result or rejection.</returns>
    Task<PlaybackControlResult> FiltersAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? arguments,
        CancellationToken cancellationToken = default);

    /// <summary>Shows filters or changes only the low-frequency equalizer curve.</summary>
    /// <param name="message">The guild message receiving the localized result.</param>
    /// <param name="member">The requesting member; required for changes.</param>
    /// <param name="level">The invariant command argument, or null for inspection.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed result or rejection.</returns>
    Task<PlaybackControlResult> BassBoostAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? level,
        CancellationToken cancellationToken = default);

    /// <summary>Shows or changes speed while preserving pitch, rate, and other filters.</summary>
    /// <param name="message">The guild message receiving the localized result.</param>
    /// <param name="member">The requesting member; required for changes.</param>
    /// <param name="multiplier">The invariant command argument, or null for inspection.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed result or rejection.</returns>
    Task<PlaybackControlResult> SpeedAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default);

    /// <summary>Shows or changes pitch while preserving speed, rate, and other filters.</summary>
    /// <param name="message">The guild message receiving the localized result.</param>
    /// <param name="member">The requesting member; required for changes.</param>
    /// <param name="multiplier">The invariant command argument, or null for inspection.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The localized confirmed result or rejection.</returns>
    Task<PlaybackControlResult> PitchAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? multiplier,
        CancellationToken cancellationToken = default);


    /// <summary>Reads the current track, requester, source, position, repeat state, output volume, and configured effects.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> NowPlayingAsync(
        IDiscordMessage message,
        CancellationToken cancellationToken = default);

    /// <summary>Reads or sets the connected player output level between zero and 200 percent.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="member">The requesting member, required for audio mutations.</param>
    /// <param name="value">The invariant command argument, validated before transport.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> VolumeAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string? value,
        CancellationToken cancellationToken = default);

    /// <summary>Seeks the active track to an absolute seconds, mm:ss, or hh:mm:ss position.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="member">The requesting member, required for audio mutations.</param>
    /// <param name="position">The invariant command argument, validated before transport.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> SeekAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string position,
        CancellationToken cancellationToken = default);

    /// <summary>Moves forward by nonnegative integer seconds, clamping before the end of the active track.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="member">The requesting member, required for audio mutations.</param>
    /// <param name="seconds">The invariant command argument, validated before transport.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> ForwardAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string seconds,
        CancellationToken cancellationToken = default);

    /// <summary>Moves backward by nonnegative integer seconds, clamping at the beginning of the active track.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="member">The requesting member, required for audio mutations.</param>
    /// <param name="seconds">The invariant command argument, validated before transport.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> RewindAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string seconds,
        CancellationToken cancellationToken = default);

    /// <summary>Seeks the active track to zero without replacing it or changing its pause state.</summary>
    /// <param name="message">The guild message receiving the localized command result.</param>
    /// <param name="member">The requesting member, required for audio mutations.</param>
    /// <param name="cancellationToken">Cancels supported downstream operations.</param>
    /// <returns>The confirmed result or a localized rejection or uncertainty outcome.</returns>
    Task<PlaybackControlResult> RestartAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default);


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

    /// <summary>Inspects or edits the waiting queue using one-based Discord arguments and existing ownership.</summary>
    Task<PlaybackControlResult> QueueCommandAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string operation,
        string arguments,
        CancellationToken cancellationToken = default);

}
