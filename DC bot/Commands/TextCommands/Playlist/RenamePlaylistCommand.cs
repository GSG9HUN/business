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

public class RenamePlaylistCommand(
    ILogger<RenamePlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "renamePlaylist";
    public string Description => localizationService.Get(LocalizationKeys.RenamePlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var parsed = await commandHelper.TryParseSavePlaylistArguments(message, responseBuilder, logger, Name);
        if (parsed is null) return;

        var (currentName, newName) = parsed.Value;
        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.RenamePlaylistAsync(guildId, currentName, newName);

        switch (result)
        {
            case RenamePlaylistResult.Renamed:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RenamePlaylistCommandRenamed,
                    currentName, newName);
                break;
            case RenamePlaylistResult.PlaylistDoesNotExist:
                await responseBuilder.SendWarningAsync(message,
                    LocalizationKeys.RenamePlaylistCommandPlaylistDoesNotExist, currentName);
                break;
            case RenamePlaylistResult.PlaylistAlreadyExists:
                await responseBuilder.SendWarningAsync(message,
                    LocalizationKeys.RenamePlaylistCommandPlaylistAlreadyExists, newName);
                break;
            case RenamePlaylistResult.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message,
                    LocalizationKeys.RenamePlaylistCommandInvalidPlaylistName);
                break;
            case RenamePlaylistResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.RenamePlaylistCommandUnknownError,
                    currentName, newName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
