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

/// <summary>Validates help entry and delegates presentation and message lifetime to their existing service boundaries.</summary>
/// <param name="userValidation">Rejects bot-authored command messages.</param>
/// <param name="logger">Records command invocation and completion.</param>
/// <param name="responses">Presents argument and guild validation failures.</param>
/// <param name="localization">Resolves this command's description.</param>
/// <param name="helpResponses">Builds localized help views and navigation metadata.</param>
/// <param name="pagination">Owns response delivery, interactions and expiry.</param>
public sealed class HelpCommand(
    IUserValidationService userValidation,
    ILogger<HelpCommand> logger,
    IResponseBuilder responses,
    ILocalizationService localization,
    IHelpResponseBuilder helpResponses,
    DiscordPaginationService pagination) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.Help;

    /// <inheritdoc />
    public string Description => localization.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.HelpCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        var guildId = message.Channel.Guild?.Id ?? 0;

        if (guildId == 0)
        {
            await responses.SendWarningAsync(message, LocalizationKeys.SlashCommandGuildOnly);
            return;
        }

        var arguments = message.Content.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries);

        if (arguments.Length > 2
            || (arguments.Length == 2 && arguments[1].Length > 64))
        {
            await responses.SendUsageAsync(message, Name);
            return;
        }

        var commandName = arguments.Length == 2 ? arguments[1] : null;
        var response = helpResponses.Create(guildId, commandName);

        if (response is null)
        {
            await responses.SendWarningAsync(message, LocalizationKeys.HelpCommandUnknown, commandName!);
            await responses.SendUsageAsync(message, Name);
            return;
        }

        await pagination.SendHelpAsync(message, response);
        logger.CommandExecuted(Name);
    }
}