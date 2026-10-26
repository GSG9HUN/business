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
/// Handles the view playlist text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="playlistService">The playlist service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class ViewPlaylistCommand(
    ILogger<ViewPlaylistCommand> logger,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    IPlaylistService playlistService,
    ICommandHelper commandHelper) : ICommand
{
    private const int MaxDisplayedTracks = 10;

    /// <inheritdoc />
    public string Name => "viewPlaylist";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.ViewPlaylistCommandDescription);

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
        var result = await playlistService.ViewPlaylistAsync(guildId, playlistName, default);
        var safePlaylistName = DiscordTextSanitizer.EscapeMentions(result.PlaylistName.Trim());
        switch (result.Status)
        {
            case ViewPlaylistStatus.Viewed:
                await responseBuilder.SendSuccessAsync(message, LocalizationKeys.ViewPlaylistCommandResponse, safePlaylistName, result.Tracks.Count, FormatTracks(guildId, result.Tracks));
                break;
            case ViewPlaylistStatus.PlaylistDoesNotExist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewPlaylistCommandPlaylistDoesNotExist, safePlaylistName);
                break;
            case ViewPlaylistStatus.EmptyPlaylist:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewPlaylistCommandEmptyPlaylist, safePlaylistName);
                break;
            case ViewPlaylistStatus.InvalidPlaylistName:
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewPlaylistCommandInvalidPlaylistName, safePlaylistName);
                break;
            case ViewPlaylistStatus.UnknownError:
                await responseBuilder.SendErrorAsync(message, LocalizationKeys.ViewPlaylistCommandUnknownError, safePlaylistName);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(result), result.Status, null);
        }

        logger.CommandExecuted(Name);
    }

    /// <summary>
    /// Formats playlist track positions and metadata for a Discord response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="tracks">The tracks supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private string FormatTracks(ulong guildId, IReadOnlyList<PlaylistViewTrackDto> tracks)
    {
        var lines = tracks.Take(MaxDisplayedTracks)
            .Select(track => localizationService.Get(guildId, LocalizationKeys.ViewPlaylistCommandTrack, track.OrderNumber, DiscordTextSanitizer.EscapeMentions(track.Author), DiscordTextSanitizer.EscapeMentions(track.Title), FormatDuration(track.Duration)))
            .ToList();

        if (tracks.Count > MaxDisplayedTracks)
        {
            lines.Add(localizationService.Get(guildId, LocalizationKeys.ViewPlaylistCommandMoreTracks, tracks.Count - MaxDisplayedTracks));
        }

        return string.Join(Environment.NewLine, lines);
    }

    /// <summary>
    /// Formats track duration for display in a Discord response.
    /// </summary>
    /// <param name="duration">The duration supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string FormatDuration(TimeSpan duration)
    {
        return duration.TotalHours >= 1 ? duration.ToString(@"h\:mm\:ss") : duration.ToString(@"m\:ss");
    }
}
