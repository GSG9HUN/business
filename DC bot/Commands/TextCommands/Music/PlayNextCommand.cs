using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Search;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>
/// Loads music to play next through existing validation, search, and localized response boundaries.
/// </summary>
/// <param name="playbackRequestService">The existing source-loading and session-bound playback request service.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="trackSearchResolverService">The track search resolver service dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class PlayNextCommand(
    IPlaybackRequestService playbackRequestService,
    IUserValidationService userValidation,
    IResponseBuilder responseBuilder,
    ILogger<PlayNextCommand> logger,
    ITrackSearchResolverService trackSearchResolverService,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.PlayNext;

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.PlayNextCommandDescription;

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

        var query = await commandHelper.TryGetArgumentAsync(message, responseBuilder, logger, Name, default);

        if (query is null)
        {
            return;
        }

        var voiceChannel = validationResult.Member?.VoiceState?.Channel;

        if (voiceChannel is null)
        {
            await responseBuilder.SendValidationErrorAsync(message, ValidationErrorKeys.UserNotInVoiceChannel, default);
            return;
        }

        var trackSearchMode = trackSearchResolverService.ResolveSearchMode(query);

        await playbackRequestService.PlayNextAsync(voiceChannel, query, message,
            trackSearchMode, message.Author.Username, default);

        logger.CommandExecuted(Name);
    }
}
