using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Presentation;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Repeat;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>
/// Handles the repeat list text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="repeatService">The repeat service dependency used by this boundary.</param>
/// <param name="queueService">The music queue service dependency used by this boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="trackFormatterService">The track formatter service dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class RepeatListCommand(
    IRepeatService repeatService,
    IMusicQueueService queueService,
    IUserValidationService userValidation,
    ILogger<RepeatListCommand> logger,
    IResponseBuilder responseBuilder,
    ITrackFormatterService trackFormatterService,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "repeatList";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.RepeatListCommandDescription);

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

        if (await repeatService.IsRepeatingAsync(guildId, default))
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.RepeatListCommandTrackAlreadyRepeating);
            logger.CommandExecuted(Name);
            return;
        }

        if (await repeatService.IsRepeatingListAsync(guildId, default))
        {
            await repeatService.SetRepeatingListAsync(guildId, false, default);
            await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RepeatListCommandRepeatingOff, await trackFormatterService.FormatCurrentTrackListAsync(guildId, default));
            logger.CommandExecuted(Name);
            return;
        }

        if (!await queueService.TryEnableBatchRepeatAsync(guildId, default))
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewListCommandError);
            return;
        }

        var batch = await queueService.GetRepeatableQueue(guildId, default);
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.RepeatListCommandRepeatingOn, string.Join("\n", batch.Take(10)
            .Select(track => $"{track.Author} - {track.Title}")));
        logger.CommandExecuted(Name);
    }
}
