using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Utility;

/// <summary>
/// Handles the help text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandRegistry">The command registry dependency used by this boundary.</param>
public class HelpCommand(
    IUserValidationService userValidation,
    ILogger<HelpCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandRegistry commandRegistry) : ICommand
{

    /// <inheritdoc />
    public string Name => "help";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.HelpCommandDescription);

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var commands = commandRegistry.Commands;
        var response = commands.Aggregate(string.Empty, (current, command) => current + $"{command.Name} : {command.Description}\n");
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.HelpCommandResponse, response);
        logger.CommandExecuted(Name);
    }
}
