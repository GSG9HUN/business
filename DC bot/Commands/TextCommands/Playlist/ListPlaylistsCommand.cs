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

public class ListPlaylistsCommand(
    ILogger<ListPlaylistsCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "listPlaylists";
    public string Description => localizationService.Get(LocalizationKeys.ListPlaylistsCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.ListPlaylistsAsync(guildId);

        switch (result.Status)
        {
            case ListPlaylistsStatus.Listed:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.ListPlaylistsCommandResponse,
                    FormatPlaylists(guildId, result.Playlists));
                break;
            case ListPlaylistsStatus.NoPlaylists:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.ListPlaylistsCommandNoPlaylists);
                break;
            case ListPlaylistsStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.ListPlaylistsCommandUnknownError);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }

    private string FormatPlaylists(ulong guildId, IReadOnlyList<PlaylistSummaryDto> playlists)
    {
        return string.Join(Environment.NewLine, playlists.Select((playlist, index) =>
            localizationService.Get(guildId, LocalizationKeys.ListPlaylistsCommandItem, index + 1, playlist.Name,
                playlist.TrackCount)));
    }
}
