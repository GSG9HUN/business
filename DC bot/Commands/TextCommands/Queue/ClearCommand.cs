using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>
/// Handles the clear text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class ClearCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ClearCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "clear";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.ClearCommandDescription);

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
        await musicQueueService.ClearQueue(guildId, default);
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.ClearCommandResponse);
        logger.CommandExecuted(Name);
    }
}
