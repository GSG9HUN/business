using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.MusicServices;

public class PlaybackRequestService(
    IAudioService audioService,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ITrackNotificationService trackNotificationService,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    ITrackPlaybackService trackPlaybackService,
    ILogger<PlaybackRequestService> logger) : IPlaybackRequestService
{
    public Task PlayAsyncUrl(IDiscordChannel voiceStateChannel, Uri url, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        return PlayAsync(
            voiceStateChannel,
            url.ToString(),
            message,
            trackSearchMode,
            "LoadTracksAsyncUrl",
            "PlayAsyncUrl.NotFound",
            "Failed to load track from URL",
            logger.FailedToFindMusicWithUrl);
    }

    public Task PlayAsyncQuery(IDiscordChannel voiceStateChannel, string query, IDiscordMessage message,
        TrackSearchMode trackSearchMode)
    {
        return PlayAsync(
            voiceStateChannel,
            query,
            message,
            trackSearchMode,
            "LoadTracksAsyncQuery",
            "PlayAsyncQuery.NotFound",
            "Failed to load track from query",
            logger.FailedToFindMusicWithQuery);
    }

    private async Task PlayAsync(
        IDiscordChannel voiceStateChannel,
        string query,
        IDiscordMessage message,
        TrackSearchMode trackSearchMode,
        string loadOperation,
        string notFoundOperation,
        string loadFailureMessage,
        Action<string> logNotFound)
    {
        logger.LogDebug(
            "Playback request started. Guild: {GuildId}, Channel: {ChannelId}, SearchMode: {SearchMode}, Operation: {Operation}",
            voiceStateChannel.Guild.Id,
            voiceStateChannel.Id,
            trackSearchMode,
            loadOperation);

        var (connection, _, guildId, isValid) =
            await playerConnectionService.TryJoinAndValidateAsync(message, voiceStateChannel);
        if (!isValid || connection == null)
        {
            logger.LogInformation("Playback request aborted after failed connection validation. Guild: {GuildId}",
                voiceStateChannel.Guild.Id);
            return;
        }

        var textChannel = message.Channel;

        playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, textChannel);

        TrackLoadResult loadResult;
        try
        {
            loadResult = await audioService.Tracks.LoadTracksAsync(query, trackSearchMode)
                .ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, loadOperation);
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
            throw new TrackLoadException(query, loadFailureMessage, ex);
        }

        if (loadResult.Track is null || loadResult.IsFailed)
        {
            await trackNotificationService.SendSafeAsync(textChannel,
                $"{localizationService.Get(guildId, LocalizationKeys.PlayCommandFailedToFindMusicUrlError)} {query}",
                notFoundOperation);
            logNotFound(query);
            throw new TrackLoadException(query, "Track not found or load failed");
        }

        logger.LogDebug("Playback request loaded tracks for guild {GuildId}. IsPlaylist: {IsPlaylist}",
            guildId,
            loadResult.IsPlaylist);
        await trackPlaybackService.PlayTheFoundMusicAsync(loadResult, connection, textChannel);
    }
}
