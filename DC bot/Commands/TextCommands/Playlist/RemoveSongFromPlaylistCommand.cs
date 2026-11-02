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
/// Handles the remove song from playlist text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="playlistService">The playlist service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class RemoveSongFromPlaylistCommand(
    ILogger<RemoveSongFromPlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "removeSong";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.RemoveSongFromPlaylistCommandDescription);

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

        var parsed = await commandHelper.TryParseSavePlaylistArguments(message, responseBuilder, logger, Name, default);

        if (parsed is null)
        {
            return;
        }

        var(playlistName, trackNumberText) = parsed.Value;
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions(playlistName.Trim());
        var guildId = message.Channel.Guild.Id;

        if (!int.TryParse(trackNumberText, out var trackNumber)
            || trackNumber <= 0)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandInvalidTrackNumber, safePlaylistName, trackNumberText);
            return;
        }

        var result = await playlistService.RemoveSongFromPlaylistAsync(guildId, playlistName, trackNumber, default);
        switch (result)
        {
            case RemoveSongResult.Removed:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandRemoved, safePlaylistName, trackNumber);
                break;
            case RemoveSongResult.PlaylistDoesNotExist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandPlaylistDoesNotExist, safePlaylistName);
                break;
            case RemoveSongResult.SongNotFound:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandSongNotFound, safePlaylistName, trackNumber);
                break;
            case RemoveSongResult.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandInvalidPlaylistName, safePlaylistName);
                break;
            case RemoveSongResult.InvalidTrackNumber:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandInvalidTrackNumber, safePlaylistName, trackNumber);
                break;
            case RemoveSongResult.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.RemoveSongFromPlaylistCommandUnknownError, safePlaylistName, trackNumber);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result, null);
        }

        logger.CommandExecuted(Name);
    }
}
