using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Presentation;
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
/// <param name="pagination">Owns caller-bound navigation for long read-only results.</param>
public class HelpCommand(
    IUserValidationService userValidation,
    ILogger<HelpCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandRegistry commandRegistry,
    DiscordPaginationService pagination) : ICommand
{

    /// <inheritdoc />
    public string Name => "help";

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.HelpCommandDescription;

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

        var arguments = message.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);
        var guildId = message.Channel.Guild?.Id ?? 0;

        if (arguments.Length > 2
            || (arguments.Length == 2 && arguments[1].Length > 64))
        {
            await responseBuilder.SendUsageAsync(message, Name);
            return;
        }

        if (arguments.Length == 2)
        {
            var command = commandRegistry.Commands.FirstOrDefault(candidate =>
                candidate.Name.Equals(arguments[1], StringComparison.OrdinalIgnoreCase));

            if (command is null)
            {
                await responseBuilder.SendWarningAsync(message, LocalizationKeys.HelpCommandUnknown, arguments[1]);
                await responseBuilder.SendUsageAsync(message, Name);
                return;
            }

            await responseBuilder.SendSuccessAsync(message, LocalizationKeys.HelpCommandDetails,
                command.Name, localizationService.Get(guildId, command.DescriptionKey));
            await responseBuilder.SendUsageAsync(message, command.Name);
            logger.CommandExecuted(Name);
            return;
        }

        var items = commandRegistry.Commands.OrderBy(command => command.Name, StringComparer.OrdinalIgnoreCase)
            .Select(command => $"**{command.Name}**: {localizationService.Get(guildId, command.DescriptionKey)}")
            .ToArray();
        await pagination.SendAsync(message, DiscordPaginationService.FromItems(
            localizationService.Get(guildId, LocalizationKeys.PaginationHelpTitle), items));
        logger.CommandExecuted(Name);
    }
}
