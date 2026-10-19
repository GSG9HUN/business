using DC_bot.Constants;
using DC_bot.Exceptions.Music;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Events;
using DC_bot.Interface.Service.Music.Lifecycle;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Playback.Models;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Playlist;

public class LoadPlaylistCommand(
    ILogger<LoadPlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    IMusicQueueService musicQueueService,
    ITrackSerializer trackSerializer,
    IPlayerConnectionService playerConnectionService,
    IPlaybackEventHandlerService playbackEventHandlerService,
    ITrackPlaybackService trackPlaybackService,
    GuildPlaybackSessions sessions,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "loadPlaylist";
    public string Description => localizationService.Get(LocalizationKeys.LoadPlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (playlistName is null) return;

        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.LoadPlaylistAsync(guildId, playlistName);
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions((result.Playlist?.Name ?? playlistName).Trim());

        switch (result.Status)
        {
            case LoadPlaylistStatus.Loaded:
                if (result.Playlist is null || result.Playlist.Tracks.Count == 0)
                {
                    await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandEmptyPlaylist,
                        safePlaylistName);
                    break;
                }

                var tracks = TryDeserializeTracks(guildId, result.Playlist);
                if (tracks is null)
                {
                    await responseBuilder.SendErrorAsync(message, LocalizationKeys.LoadPlaylistCommandUnknownError,
                        safePlaylistName);
                    break;
                }

                var (connection, _, _, isValid) =
                    await playerConnectionService.TryJoinAndValidateAsync(message,
                        validationResult.Member?.VoiceState?.Channel);

                if (!isValid || connection is null) return;

                var session = sessions.Capture(guildId, connection);
                if (session is null)
                {
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded);
                    return;
                }

                var queueItems = tracks
                    .Select(track =>
                        new QueueTrackToEnqueue(track, SourceQuery: null, SourceSearchMode: null, RequestedBy: null))
                    .ToList();

                var enqueueSucceeded = await TryEnqueueUnderSessionFenceAsync(session, guildId, queueItems);
                if (!enqueueSucceeded)
                {
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded);
                    return;
                }

                playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, message.Channel);

                await responseBuilder.SendSuccessAsync(
                    message,
                    LocalizationKeys.LoadPlaylistCommandLoaded,
                    safePlaylistName,
                    tracks.Count);

                if (connection.CurrentTrack is null)
                {
                    var startResult = await trackPlaybackService.TryPlayNextTrackAsync(
                        session,
                        message.Channel,
                        PlaybackStartReason.JoinQueue);

                    await HandlePlaybackStartResultAsync(message, startResult);
                }

                break;

            case LoadPlaylistStatus.NotFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandNotFound,
                    safePlaylistName);
                break;

            case LoadPlaylistStatus.EmptyPlaylist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandEmptyPlaylist,
                    safePlaylistName);
                break;

            case LoadPlaylistStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.LoadPlaylistCommandUnknownError,
                    safePlaylistName);
                break;

            case LoadPlaylistStatus.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandInvalidPlaylistName,
                    safePlaylistName);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }
    
     private async Task HandlePlaybackStartResultAsync(IDiscordMessage message, PlaybackStartResult startResult)
        {
            logger.LogDebug(
                "LoadPlaylist auto-start result: {Status}, operationId: {OperationId}, queueItemId: {QueueItemId}",
                startResult.Status,
                startResult.OperationId,
                startResult.QueueItemId);
    
            switch (startResult.Status)
            {
                case PlaybackStartStatus.Started:
                case PlaybackStartStatus.Queued:
                    return;
    
                case PlaybackStartStatus.StartedStatePending:
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackStatePending);
                    return;
    
                case PlaybackStartStatus.Unknown:
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackOutcomeUnknown);
                    return;
    
                case PlaybackStartStatus.Superseded:
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded);
                    return;
    
                case PlaybackStartStatus.Busy:
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackBusy);
                    return;
    
                case PlaybackStartStatus.Empty:
                case PlaybackStartStatus.NotStarted:
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError);
                    return;
    
                default:
                    throw new ArgumentOutOfRangeException(nameof(startResult), startResult.Status, null);
            }
        }

    private async Task<bool> TryEnqueueUnderSessionFenceAsync(
        PlaybackSession session,
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks)
    {
        try
        {
            await sessions.MutateAsync(session, async () =>
            {
                await musicQueueService.EnqueueMany(guildId, tracks);
                return true;
            });

            return true;
        }
        catch (PlaybackSessionExpiredException)
        {
            logger.LogInformation(
                "LoadPlaylist enqueue superseded by newer session. Guild: {GuildId}, Generation: {Generation}",
                session.GuildId,
                session.Generation);
            return false;
        }
    }

    private IReadOnlyList<ILavaLinkTrack>? TryDeserializeTracks(ulong guildId, PlaylistDto playlist)
    {
        try
        {
            return playlist.Tracks
                .OrderBy(track => track.OrderNumber)
                .Select(track => trackSerializer.Deserialize(track.TrackIdentifier))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex,
                "Failed to deserialize stored tracks while loading playlist {PlaylistName} for guild {GuildId}",
                playlist.Name,
                guildId);
            return null;
        }
    }
}