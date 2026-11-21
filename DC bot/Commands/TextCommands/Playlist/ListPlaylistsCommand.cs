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
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Playlist;

/// <summary>
/// Handles the list playlists text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="playlistService">The playlist service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
/// <param name="pagination">Owns caller-bound navigation for long read-only results.</param>
public class ListPlaylistsCommand(
    ILogger<ListPlaylistsCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper,
    DiscordPaginationService pagination) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.ListPlaylists;

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.ListPlaylistsCommandDescription;

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if ((message.Content ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 1)
        {
            await responseBuilder.SendUsageAsync(message, Name);
            return;
        }

        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message, default);

        if (validationResult is null)
        {
            return;
        }

        var guildId = message.Channel.Guild.Id;
        var result = await playlistService.ListPlaylistsAsync(guildId, default);
        switch (result.Status)
        {
            case ListPlaylistsStatus.Listed:
                var items = result.Playlists.Select((playlist, index) => localizationService.Get(guildId,
                        LocalizationKeys.ListPlaylistsCommandItem, index + 1,
                        DiscordTextSanitizer.EscapeMentions(playlist.Name), playlist.TrackCount))
                    .ToArray();
                await pagination.SendAsync(message, DiscordPaginationService.FromItems(
                    localizationService.Get(guildId, LocalizationKeys.PaginationPlaylistsTitle), items));
                break;
            case ListPlaylistsStatus.NoPlaylists:
                await responseBuilder.SendInfoAsync(message, LocalizationKeys.ListPlaylistsCommandNoPlaylists);
                break;
            case ListPlaylistsStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.ListPlaylistsCommandUnknownError);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }
}
