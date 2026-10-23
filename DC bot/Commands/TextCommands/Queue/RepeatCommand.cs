using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>
/// Handles the repeat text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="repeatService">The repeat service dependency used by this boundary.</param>
/// <param name="currentTrackService">The current track service dependency used by this boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class RepeatCommand(
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    IUserValidationService userValidation,
    ILogger<RepeatCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "repeat";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.RepeatCommandDescription);

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

        var guildId = message.Channel.Guild.Id;

        if (await repeatService.IsRepeatingListAsync(guildId, default))
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.RepeatCommandListAlreadyRepeating);
            logger.CommandExecuted(Name);
            return;
        }

        if (await repeatService.IsRepeatingAsync(guildId, default))
        {
            await repeatService.SetRepeatingAsync(guildId, false, default);
            await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RepeatCommandRepeatingOff);
            logger.CommandExecuted(Name);
            return;
        }

        await repeatService.SetRepeatingAsync(guildId, true, default);
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RepeatCommandRepeatingOn, await currentTrackService.GetCurrentTrackFormattedAsync(guildId));
        logger.CommandExecuted(Name);
    }
}
