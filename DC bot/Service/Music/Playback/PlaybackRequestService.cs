using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Playback;

/// <summary>
/// Loads requested tracks and routes them to a validated, session-bound queue.
/// </summary>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="trackNotificationService">The track notification service dependency used by this boundary.</param>
/// <param name="playerConnectionService">The player connection service dependency used by this boundary.</param>
/// <param name="playbackEventHandlerService">The playback event handler service dependency used by this boundary.</param>
/// <param name="trackPlaybackService">The track playback service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public sealed class PlaybackRequestService(
    IAudioService audioService,
    ILocalizationService localizationService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    ITrackPlaybackService trackPlaybackService,
    GuildPlaybackSessions sessions,
    ILogger<PlaybackRequestService> logger) : IPlaybackRequestService
{

    /// <summary>
    /// Loads a source URL through the shared session-bound playback request pipeline.
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
        return PlayAsync(voiceStateChannel, url.ToString(), message, trackSearchMode, requestedBy, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Loads a search query through the shared session-bound playback request pipeline.
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
        return PlayAsync(voiceStateChannel, query, message, trackSearchMode, requestedBy, cancellationToken: cancellationToken);
    }

    /// <summary>
    /// Joins and validates a player, loads the requested source, and dispatches loaded tracks only while the captured session remains current.
    /// </summary>
    /// <param name="voiceStateChannel">The Discord channel used by the operation.</param>
    /// <param name="query">The query supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="searchMode">The search mode supplied for this operation.</param>
    /// <param name="requestedBy">The requested by supplied for this operation.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    private async Task<PlaybackStartResult> PlayAsync(
        IDiscordChannel voiceStateChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode searchMode,
        string? requestedBy,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var requestId = Guid.NewGuid();
        var(player, _, guildId, valid) = await playerConnectionService.TryJoinAndValidateAsync(message, voiceStateChannel, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (!valid || player is null)
        {
            return new(PlaybackStartStatus.NotStarted, requestId);
        }

        var session = sessions.Capture(guildId, player);

        if (session is null)
        {
            return new(PlaybackStartStatus.Superseded, requestId);
        }

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, player, message.Channel);
        TrackLoadResult loaded;
        try
        {
            loaded = await audioService.Tracks.LoadTracksAsync(query, searchMode, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Playback request failed. OperationId: {OperationId}, GuildId: {GuildId}, Phase: {Phase}, Category: {Category}, ExceptionType: {ExceptionType}", requestId, guildId, "TrackLoad", "SourceLoad", exception.GetType().Name);
            await SendOutcomeAsync(message, new(PlaybackStartStatus.NotStarted, requestId), cancellationToken)
                .ConfigureAwait(false);
            throw new TrackLoadException(query, "Track source loading failed.", exception);
        }

        if (!sessions.IsCurrent(session))
        {
            var superseded = new PlaybackStartResult(PlaybackStartStatus.Superseded, requestId);
            await SendOutcomeAsync(message, superseded, cancellationToken)
                .ConfigureAwait(false);
            return superseded;
        }

        if (loaded.IsFailed
            || (loaded.IsPlaylist ? loaded.Tracks.Length == 0 : loaded.Track is null))
        {
            await SendOutcomeAsync(message, new(PlaybackStartStatus.NotStarted, requestId), cancellationToken)
                .ConfigureAwait(false);
            throw new TrackLoadException(query, "Track not found or load failed.");
        }

        PlaybackStartResult result;
        try
        {
            result = await trackPlaybackService.PlayTheFoundMusicAsync(loaded, session, message.Channel, requestedBy, query, searchMode, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (PlaybackSessionExpiredException)
        {
            result = new(PlaybackStartStatus.Superseded, requestId);
        }

        await SendOutcomeAsync(message, result, cancellationToken)
            .ConfigureAwait(false);
        return result;
    }

    /// <summary>
    /// Presents a playback-start result using the appropriate localized response.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="result">The result supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task SendOutcomeAsync(
        IDiscordMessage message,
        PlaybackStartResult result,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var key = result.Status switch
        {
            PlaybackStartStatus.StartedStatePending => ValidationErrorKeys.PlaybackStatePending,
            PlaybackStartStatus.Unknown => ValidationErrorKeys.PlaybackOutcomeUnknown,
            PlaybackStartStatus.Superseded => ValidationErrorKeys.PlaybackSuperseded,
            PlaybackStartStatus.Busy => ValidationErrorKeys.PlaybackBusy,
            PlaybackStartStatus.NotStarted or PlaybackStartStatus.Empty => ValidationErrorKeys.LavalinkError,
            _ => null
        };

        if (key is null)
        {
            return;
        }

        try
        {
            await trackNotificationService.SendSafeAsync(message.Channel, localizationService.Get(message.Channel.Guild.Id, key), "PlaybackRequest.Outcome", cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning("Outcome notification failed. OperationId: {OperationId}, GuildId: {GuildId}, Phase: {Phase}, Category: {Category}, ExceptionType: {ExceptionType}", result.OperationId, message.Channel.Guild.Id, "Notification", "Notification", exception.GetType().Name);
        }
    }
}
