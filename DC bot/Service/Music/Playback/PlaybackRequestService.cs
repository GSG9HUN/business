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
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.Playback;

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
    public Task<PlaybackStartResult> PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url,
        IDiscordMessage message, TrackSearchMode trackSearchMode, string? requestedBy = null)
    {
        return PlayAsync(voiceStateChannel, url.ToString(), message, trackSearchMode, requestedBy);
    }

    public Task<PlaybackStartResult> PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query,
        IDiscordMessage message, TrackSearchMode trackSearchMode, string? requestedBy = null)
    {
        return PlayAsync(voiceStateChannel, query, message, trackSearchMode, requestedBy);
    }

    private async Task<PlaybackStartResult> PlayAsync(IDiscordChannel voiceStateChannel, string query,
        IDiscordMessage message, TrackSearchMode searchMode, string? requestedBy)
    {
        var requestId = Guid.NewGuid();
        var (player, _, guildId, valid) = await playerConnectionService
            .TryJoinAndValidateAsync(message, voiceStateChannel).ConfigureAwait(false);
        if (!valid || player is null)
            return new(PlaybackStartStatus.NotStarted, requestId);

        var session = sessions.Capture(guildId, player);
        if (session is null)
            return new(PlaybackStartStatus.Superseded, requestId);
        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, player, message.Channel);

        TrackLoadResult loaded;
        try
        {
            loaded = await audioService.Tracks.LoadTracksAsync(query, searchMode).ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning("Playback request failed. OperationId: {OperationId}, GuildId: {GuildId}, Phase: {Phase}, Category: {Category}, ExceptionType: {ExceptionType}",
                requestId, guildId, "TrackLoad", "SourceLoad", exception.GetType().Name);
            await SendOutcomeAsync(message, new(PlaybackStartStatus.NotStarted, requestId)).ConfigureAwait(false);
            throw new TrackLoadException(query, "Track source loading failed.", exception);
        }

        if (!sessions.IsCurrent(session))
        {
            var superseded = new PlaybackStartResult(PlaybackStartStatus.Superseded, requestId);
            await SendOutcomeAsync(message, superseded).ConfigureAwait(false);
            return superseded;
        }
        if (loaded.IsFailed || (loaded.IsPlaylist ? loaded.Tracks.Length == 0 : loaded.Track is null))
        {
            await SendOutcomeAsync(message, new(PlaybackStartStatus.NotStarted, requestId)).ConfigureAwait(false);
            throw new TrackLoadException(query, "Track not found or load failed.");
        }

        PlaybackStartResult result;
        try
        {
            result = await trackPlaybackService.PlayTheFoundMusicAsync(loaded, session,
                message.Channel, requestedBy, query, searchMode).ConfigureAwait(false);
        }
        catch (PlaybackSessionExpiredException)
        {
            result = new(PlaybackStartStatus.Superseded, requestId);
        }
        await SendOutcomeAsync(message, result).ConfigureAwait(false);
        return result;
    }

    private async Task SendOutcomeAsync(IDiscordMessage message, PlaybackStartResult result)
    {
        var key = result.Status switch
        {
            PlaybackStartStatus.StartedStatePending => ValidationErrorKeys.PlaybackStatePending,
            PlaybackStartStatus.Unknown => ValidationErrorKeys.PlaybackOutcomeUnknown,
            PlaybackStartStatus.Superseded => ValidationErrorKeys.PlaybackSuperseded,
            PlaybackStartStatus.Busy => ValidationErrorKeys.PlaybackBusy,
            PlaybackStartStatus.NotStarted or PlaybackStartStatus.Empty => ValidationErrorKeys.LavalinkError,
            _ => null
        };
        if (key is null) return;
        try
        {
            await trackNotificationService.SendSafeAsync(message.Channel,
                localizationService.Get(message.Channel.Guild.Id, key), "PlaybackRequest.Outcome")
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            logger.LogWarning("Outcome notification failed. OperationId: {OperationId}, GuildId: {GuildId}, Phase: {Phase}, Category: {Category}, ExceptionType: {ExceptionType}",
                result.OperationId, message.Channel.Guild.Id, "Notification", "Notification", exception.GetType().Name);
        }
    }
}