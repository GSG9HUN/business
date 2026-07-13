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

public class SavePlaylistCommand(
    ILogger<SavePlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "savePlaylist";
    public string Description => localizationService.Get(LocalizationKeys.SavePlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var parsed = await commandHelper.TryParseSavePlaylistArguments(message, responseBuilder, logger, Name);
        if (parsed is null) return;

        var (playlistName, playlistUrl) = parsed.Value;
        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.SavePlaylistAsync(guildId, playlistName, playlistUrl);

        switch (result)
        {
            case SavePlaylistResult.Saved:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.SavePlaylistCommandSaved, playlistName);
                break;
            case SavePlaylistResult.AlreadyExists:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandAlreadyExists,
                    playlistName);
                break;
            case SavePlaylistResult.NoTracksFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandNoTracksFound,
                    playlistName);
                break;
            case SavePlaylistResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.SavePlaylistCommandUnknownError,
                    playlistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
