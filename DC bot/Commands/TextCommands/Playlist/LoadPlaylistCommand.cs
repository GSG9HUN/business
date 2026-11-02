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
using DC_bot.Service.Music.Lifecycle.Models;
using DC_bot.Service.Music.Lifecycle.Sessions;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Playlist;

/// <summary>
/// Handles the load playlist text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="playlistService">The playlist service dependency used by this boundary.</param>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
/// <param name="playerConnectionService">The player connection service dependency used by this boundary.</param>
/// <param name="playbackEventHandlerService">The playback event handler service dependency used by this boundary.</param>
/// <param name="trackPlaybackService">The track playback service dependency used by this boundary.</param>
/// <param name="sessions">The sessions supplied for this operation.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
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

    /// <inheritdoc />
    public string Name => "loadPlaylist";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.LoadPlaylistCommandDescription);

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message, default);

        if (validationResult is null)
        {
            return;
        }

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);

        if (playlistName is null)
        {
            return;
        }

        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.LoadPlaylistAsync(guildId, playlistName, default);
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions((result.Playlist?.Name ?? playlistName).Trim());
        switch (result.Status)
        {
            case LoadPlaylistStatus.Loaded:

                if (result.Playlist is null || result.Playlist.Tracks.Count == 0)
                {
                    await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandEmptyPlaylist, safePlaylistName);
                    break;
                }

                var tracks = TryDeserializeTracks(guildId, result.Playlist);

                if (tracks is null)
                {
                    await responseBuilder.SendErrorAsync(message, LocalizationKeys.LoadPlaylistCommandUnknownError, safePlaylistName);
                    break;
                }

                var(connection, _, _, isValid) = await playerConnectionService.TryJoinAndValidateAsync(message, validationResult.Member?.VoiceState?.Channel);

                if (!isValid || connection is null)
                {
                    return;
                }

                var session = sessions.Capture(guildId, connection);

                if (session is null)
                {
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded, default);
                    return;
                }

                var queueItems = tracks.Select(track => new QueueTrackToEnqueue(track, SourceQuery: null, SourceSearchMode: null, RequestedBy: null))
                    .ToList();
                var enqueueSucceeded = await TryEnqueueUnderSessionFenceAsync(session, guildId, queueItems);

                if (!enqueueSucceeded)
                {
                    await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded, default);
                    return;
                }

                playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, message.Channel);
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.LoadPlaylistCommandLoaded, safePlaylistName, tracks.Count);

                if (connection.CurrentTrack is null)
                {
                    var startResult = await trackPlaybackService.TryPlayNextTrackAsync(session, message.Channel, PlaybackStartReason.JoinQueue, default);
                    await HandlePlaybackStartResultAsync(message, startResult);
                }

                break;
            case LoadPlaylistStatus.NotFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandNotFound, safePlaylistName);
                break;
            case LoadPlaylistStatus.EmptyPlaylist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandEmptyPlaylist, safePlaylistName);
                break;
            case LoadPlaylistStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.LoadPlaylistCommandUnknownError, safePlaylistName);
                break;
            case LoadPlaylistStatus.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.LoadPlaylistCommandInvalidPlaylistName, safePlaylistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }

    /// <summary>
    /// Translates playback-start status into a structured mobile-command result.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="startResult">The start result supplied for this operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task HandlePlaybackStartResultAsync(IDiscordMessage message, PlaybackStartResult startResult)
    {
        logger.LogDebug("LoadPlaylist auto-start result: {Status}, operationId: {OperationId}, queueItemId: {QueueItemId}", startResult.Status, startResult.OperationId, startResult.QueueItemId);
        switch (startResult.Status)
        {
            case PlaybackStartStatus.Started:
            case PlaybackStartStatus.Queued:
                return;
            case PlaybackStartStatus.StartedStatePending:
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackStatePending, default);
                return;
            case PlaybackStartStatus.Unknown:
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackOutcomeUnknown, default);
                return;
            case PlaybackStartStatus.Superseded:
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackSuperseded, default);
                return;
            case PlaybackStartStatus.Busy:
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.PlaybackBusy, default);
                return;
            case PlaybackStartStatus.Empty:
            case PlaybackStartStatus.NotStarted:
                await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.LavalinkError, default);
                return;
            default:
                throw new ArgumentOutOfRangeException(nameof(startResult), startResult.Status, null);
        }
    }

    /// <summary>
    /// Appends requested tracks only while the captured session still owns the guild.
    /// </summary>
    /// <param name="session">The captured player generation whose ownership must remain current.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="tracks">The tracks supplied for this operation.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    private async Task<bool> TryEnqueueUnderSessionFenceAsync(
        PlaybackSession session,
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks)
    {
        try
        {
            await sessions.MutateAsync(session, async () =>
            {
                await musicQueueService.EnqueueMany(guildId, tracks, default);
                return true;
            });
            return true;
        }
        catch (PlaybackSessionExpiredException)
        {
            logger.LogInformation("LoadPlaylist enqueue superseded by newer session. Guild: {GuildId}, Generation: {Generation}", session.GuildId, session.Generation);
            return false;
        }
    }

    /// <summary>
    /// Restores stored track identifiers while reporting unparseable entries.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="playlist">The playlist supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    private IReadOnlyList<ILavaLinkTrack>? TryDeserializeTracks(ulong guildId, PlaylistDto playlist)
    {
        try
        {
            return playlist.Tracks.OrderBy(track => track.OrderNumber)
                .Select(track => trackSerializer.Deserialize(track.TrackIdentifier))
                .ToList();
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to deserialize stored tracks while loading playlist {PlaylistName} for guild {GuildId}", playlist.Name, guildId);
            return null;
        }
    }
}
