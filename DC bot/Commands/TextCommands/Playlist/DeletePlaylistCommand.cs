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

/// <summary>
/// Handles the delete playlist text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="playlistService">The playlist service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class DeletePlaylistCommand(
    ILogger<DeletePlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "deletePlaylist";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.DeletePlaylistCommandDescription);

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

        var playlistName = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name, default);

        if (playlistName is null)
        {
            return;
        }

        var guildId = message.Channel.Guild.Id;
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions(playlistName.Trim());
        var result = await playlistService.DeletePlaylistAsync(guildId, playlistName, default);
        switch (result)
        {
            case DeletePlaylistResult.Deleted:
                logger.LogInformation("Playlist '{PlaylistName}' deleted successfully in guild {GuildId}.", playlistName, guildId);
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.DeletePlaylistCommandDeleted, safePlaylistName);
                break;
            case DeletePlaylistResult.DoesNotExist:
                logger.LogWarning("Attempted to delete non-existent playlist '{PlaylistName}' in guild {GuildId}.", playlistName, guildId);
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.DeletePlaylistCommandDoesNotExist, safePlaylistName);
                break;
            case DeletePlaylistResult.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.DeletePlaylistCommandInvalidPlaylistName, safePlaylistName);
                break;
            case DeletePlaylistResult.UnknownError:
                logger.LogError("An unknown error occurred while attempting to delete playlist '{PlaylistName}' in guild {GuildId}.", playlistName, guildId);
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.DeletePlaylistCommandUnknownError, safePlaylistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
