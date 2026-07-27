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
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions(playlistName.Trim());
        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.SavePlaylistAsync(guildId, playlistName, playlistUrl);

        switch (result)
        {
            case SavePlaylistResult.Saved:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.SavePlaylistCommandSaved, safePlaylistName);
                break;
            case SavePlaylistResult.AlreadyExists:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandAlreadyExists,
                    safePlaylistName);
                break;
            case SavePlaylistResult.NoTracksFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandNoTracksFound,
                    safePlaylistName);
                break;
            case SavePlaylistResult.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandInvalidPlaylistName,
                    safePlaylistName);
                break;
            case SavePlaylistResult.PlaylistLimitReached:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandPlaylistLimitReached,
                    safePlaylistName);
                break;
            case SavePlaylistResult.TrackLimitExceeded:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.SavePlaylistCommandTrackLimitExceeded,
                    safePlaylistName);
                break;
            case SavePlaylistResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.SavePlaylistCommandUnknownError,
                    safePlaylistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
