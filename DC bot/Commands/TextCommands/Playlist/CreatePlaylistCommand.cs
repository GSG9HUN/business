using DC_bot.Constants;
using DC_bot.Helper;
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

public class CreatePlaylistCommand(
    ILogger<CreatePlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "createPlaylist";
    public string Description => localizationService.Get(LocalizationKeys.CreatePlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);

        if (playlistName is null) return;

        var guildId = message.Channel.Guild.Id;
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions(playlistName.Trim());

        var result = await playlistService.CreatePlaylistAsync(guildId, playlistName);
        switch (result)
        {
            case CreatePlaylistResult.Created:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.CreatePlaylistCommandCreated,
                    safePlaylistName);
                break;
            case CreatePlaylistResult.PlaylistAlreadyExists:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.CreatePlaylistCommandAlreadyExists,
                    safePlaylistName);
                break;
            case CreatePlaylistResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.CreatePlaylistCommandUnknownError,
                    safePlaylistName);
                break;
            case CreatePlaylistResult.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.CreatePlaylistCommandInvalidPlaylistName,
                    safePlaylistName);
                break;
            case CreatePlaylistResult.PlaylistLimitReached:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.CreatePlaylistCommandPlaylistLimitReached,
                    safePlaylistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
