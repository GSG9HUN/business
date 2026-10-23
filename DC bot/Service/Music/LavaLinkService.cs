using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lavalink;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Sessions;
using DSharpPlus.Entities;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music;

/// <summary>
/// Exposes music commands through connection, request, queue, and playback-control services.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="repeatService">The repeat service dependency used by this boundary.</param>
/// <param name="trackNotificationService">The track notification service dependency used by this boundary.</param>
/// <param name="playerConnectionService">The player connection service dependency used by this boundary.</param>
/// <param name="playbackEventHandlerService">The playback event handler service dependency used by this boundary.</param>
/// <param name="playbackRequestService">The playback request service dependency used by this boundary.</param>
/// <param name="lavalinkNodeConnectionService">The lavalink node connection service dependency used by this boundary.</param>
/// <param name="trackPlaybackService">The track playback service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="playbackControlService">The playback control service dependency used by this boundary.</param>
public class LavaLinkService(
    ILogger<LavaLinkService> logger,
    IRepeatService repeatService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    IPlaybackRequestService playbackRequestService,
    ILavalinkNodeConnectionService lavalinkNodeConnectionService,
    ITrackPlaybackService trackPlaybackService,
    GuildPlaybackSessions sessions,
    IPlaybackControlService playbackControlService) : ILavaLinkService
{

    /// <inheritdoc />
    public event Func<IDiscordChannel, DiscordEmbed, Task> TrackStarted { add => trackNotificationService.TrackStarted += value; remove => trackNotificationService.TrackStarted -= value; }

    /// <summary>
    /// Initializes guild-specific persisted music state for subsequent commands.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <returns>A task representing completion of this operation.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    public async Task Init(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await repeatService.InitAsync(guildId, cancellationToken);
        logger.LogDebug("Music services initialized for guild {GuildId}.", guildId);
    }

    /// <summary>
    /// Connects the configured Discord or Lavalink runtime boundary and propagates connection failures.
    /// </summary>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await lavalinkNodeConnectionService.ConnectAsync(cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Delegates URL playback to the request service using the supplied voice and response context.
    /// </summary>
    /// <param name="voiceStateChannel">The Discord channel used by the operation.</param>
    /// <param name="url">The url supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackStartResult> PlayAsyncUrl(
        IDiscordChannel voiceStateChannel,
        Uri url,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackRequestService.PlayAsyncUrl(voiceStateChannel, url, message, trackSearchMode, requestedBy, cancellationToken);
    }

    /// <summary>
    /// Delegates query playback to the request service using the supplied voice and response context.
    /// </summary>
    /// <param name="voiceStateChannel">The Discord channel used by the operation.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="trackSearchMode">The track search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackStartResult> PlayAsyncQuery(
        IDiscordChannel voiceStateChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string? requestedBy = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackRequestService.PlayAsyncQuery(voiceStateChannel, query, message, trackSearchMode, requestedBy, cancellationToken);
    }

    /// <summary>
    /// Pauses the validated player and persists playback position and pause state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackControlResult> PauseAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackControlService.PauseAsync(message, member, cancellationToken);
    }

    /// <summary>
    /// Resumes the validated player and persists updated playback state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackControlResult> ResumeAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackControlService.ResumeAsync(message, member, cancellationToken);
    }

    /// <summary>
    /// Stops and completes the current attempt, then advances without relying on a later SDK Stopped event.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackControlResult> SkipAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackControlService.SkipAsync(message, member, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Starts the preceding canonical item in the active batch, reporting when no earlier item is available.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public Task<PlaybackControlResult> PreviousAsync(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        return playbackControlService.PreviousAsync(message, member, cancellationToken);
    }

    /// <summary>
    /// Runs shared leave cleanup and closes the active batch after all applicable cleanup phases succeed.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task LeaveVoiceChannel(
        IDiscordMessage message,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        await playbackControlService.LeaveVoiceChannel(message, member, cancellationToken);
    }

    /// <summary>
    /// Joins and validates the player&apos;s channel, registers completion handling, and attempts to start the next waiting item.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="textChannel">The Discord channel used by the operation.</param>
    /// <param name="member">The discord member? dependency used by this boundary.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<PlaybackStartResult> StartPlayingQueue(
        IDiscordMessage message,
        IDiscordChannel textChannel,
        IDiscordMember? member,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var(connection, _, guildId, valid) = await playerConnectionService.TryJoinAndValidateAsync(message, member?.VoiceState?.Channel, cancellationToken: cancellationToken);

        if (!valid || connection is null)
        {
            return new(PlaybackStartStatus.NotStarted, Guid.NewGuid());
        }

        var session = sessions.Capture(guildId, connection);

        if (session is null)
        {
            return new(PlaybackStartStatus.Superseded, Guid.NewGuid());
        }

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        if (connection.CurrentTrack is not null)
        {
            return new(PlaybackStartStatus.Busy, Guid.NewGuid());
        }

        return await trackPlaybackService.TryPlayNextTrackAsync(session, textChannel, PlaybackStartReason.JoinQueue, default, cancellationToken: cancellationToken);
    }
}
