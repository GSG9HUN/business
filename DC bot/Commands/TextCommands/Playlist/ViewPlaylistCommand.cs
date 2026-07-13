using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Playlist;

public class ViewPlaylistCommand(
    ILogger<ViewPlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    private const int MaxDisplayedTracks = 10;

    public string Name => "viewPlaylist";
    public string Description => localizationService.Get(LocalizationKeys.ViewPlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);
        if (playlistName is null) return;

        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.ViewPlaylistAsync(guildId, playlistName);

        switch (result.Status)
        {
            case ViewPlaylistStatus.Viewed:
                await responseBuilder.SendSuccessAsync(message,
                    LocalizationKeys.ViewPlaylistCommandResponse,
                    result.PlaylistName,
                    result.Tracks.Count,
                    FormatTracks(guildId, result.Tracks));
                break;
            case ViewPlaylistStatus.PlaylistDoesNotExist:
                await responseBuilder.SendWarningAsync(message,
                    LocalizationKeys.ViewPlaylistCommandPlaylistDoesNotExist, playlistName);
                break;
            case ViewPlaylistStatus.EmptyPlaylist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewPlaylistCommandEmptyPlaylist,
                    result.PlaylistName);
                break;
            case ViewPlaylistStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.ViewPlaylistCommandUnknownError,
                    playlistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }

    private string FormatTracks(ulong guildId, IReadOnlyList<PlaylistViewTrackDto> tracks)
    {
        var lines = tracks
            .Take(MaxDisplayedTracks)
            .Select(track => localizationService.Get(guildId,
                LocalizationKeys.ViewPlaylistCommandTrack,
                track.OrderNumber,
                track.Author,
                track.Title,
                FormatDuration(track.Duration)))
            .ToList();

        if (tracks.Count > MaxDisplayedTracks)
        {
            lines.Add(localizationService.Get(guildId, LocalizationKeys.ViewPlaylistCommandMoreTracks,
                tracks.Count - MaxDisplayedTracks));
        }

        return string.Join(Environment.NewLine, lines);
    }

    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1
            ? duration.ToString(@"h\:mm\:ss")
            : duration.ToString(@"m\:ss");
    }
}
