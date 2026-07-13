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

public class DeletePlaylistCommand(
    ILogger<DeletePlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "deletePlaylist";
    public string Description => localizationService.Get(LocalizationKeys.DeletePlaylistCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name);

        if (playlistName is null) return;

        var guildId = message.Channel.Guild.Id;

        var result = await playlistService.DeletePlaylistAsync(guildId, playlistName);

        switch (result)
        {
            case DeletePlaylistResult.Deleted:
                logger.LogInformation("Playlist '{PlaylistName}' deleted successfully in guild {GuildId}.", playlistName,
                    guildId);
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.DeletePlaylistCommandDeleted,
                    playlistName);
                break;
            case DeletePlaylistResult.DoesNotExist:
                logger.LogWarning("Attempted to delete non-existent playlist '{PlaylistName}' in guild {GuildId}.",
                    playlistName, guildId);
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.DeletePlaylistCommandDoesNotExist,
                    playlistName);
                break;
            case DeletePlaylistResult.UnknownError:
                logger.LogError(
                    "An unknown error occurred while attempting to delete playlist '{PlaylistName}' in guild {GuildId}.",
                    playlistName, guildId);
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.DeletePlaylistCommandUnknownError,
                    playlistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
