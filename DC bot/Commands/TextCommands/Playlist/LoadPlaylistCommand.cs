using DC_bot.Constants;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
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
                    await playerConnectionService.TryJoinAndValidateAsync(
                        message,
                        validationResult.Member?.VoiceState?.Channel);
                if (!isValid || connection is null) return;

                playbackEventHandlerService.RegisterPlaybackFinishedHandler(guildId, connection, message.Channel);
                await musicQueueService.EnqueueMany(guildId, tracks);
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.LoadPlaylistCommandLoaded,
                    safePlaylistName, tracks.Count);

                if (connection.CurrentTrack is null)
                {
                    await trackPlaybackService.TryPlayNextTrackAsync(connection, message.Channel, guildId);
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
