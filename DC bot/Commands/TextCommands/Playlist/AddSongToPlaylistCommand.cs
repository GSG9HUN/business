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

public class AddSongToPlaylistCommand(
    ILogger<AddSongToPlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper): ICommand
{
    public string Name => "addSong";
    public string Description => localizationService.Get(LocalizationKeys.AddSongToPlaylistCommandDescription);
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var parsed = await commandHelper.TryParseSavePlaylistArguments(message, responseBuilder, logger, Name);
        if (parsed is null) return;

        var (playlistName, songUrl) = parsed.Value;
        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.AddSongToPlaylistAsync(guildId, playlistName, songUrl);

        switch (result)
        {
            case AddSongResult.Added:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.AddSongToPlaylistCommandAdded,
                    playlistName);
                break;
            case AddSongResult.InvalidSongUrl:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.AddSongToPlaylistCommandInvalidSongUrl,
                    playlistName);
                break;
            case AddSongResult.PlaylistDoesNotExist:
                await responseBuilder.SendWarningAsync(message,
                    LocalizationKeys.AddSongToPlaylistCommandPlaylistDoesNotExist, playlistName);
                break;
            case AddSongResult.NoTracksFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.AddSongToPlaylistCommandNoTracksFound,
                    playlistName);
                break;
            case AddSongResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.AddSongToPlaylistCommandUnknownError,
                    playlistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
