using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music.Playback.Models;

namespace DC_bot.Interface.Service.Music.Playback;

/// <summary>
/// Defines the playback control service boundary so callers are independent of its concrete implementation.
/// </summary>
public interface IPlaybackControlService
{

    /// <summary>Rechecks a previously recorded vote without allowing it to target another attempt.</summary>
    Task<PlaybackControlResult> SkipVotedAttemptAsync(
        IDiscordMessage message,
        IDiscordMember member,
        Guid attempt,
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

    /// <summary>Seeks only if the captured playback attempt is still active under the control reservation.</summary>
    /// <param name="message">The guild message used for current permission and voice checks.</param>
    /// <param name="member">The authenticated requesting Discord member.</param>
    /// <param name="position">Absolute invariant playback time.</param>
    /// <param name="expectedPlaybackOperationId">The attempt captured by the mobile client.</param>
    /// <param name="cancellationToken">Cancels supported downstream work.</param>
    /// <returns>Confirmed seek result or a localized rejection/uncertain outcome.</returns>
    Task<PlaybackControlResult> SeekAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        string position,
        Guid expectedPlaybackOperationId,
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
