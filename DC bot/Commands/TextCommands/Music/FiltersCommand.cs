using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Presentation;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Inspect, list, reset, or select player filters through the existing facade and localized response pipeline.</summary>
/// <param name="lavaLinkService">The existing music facade.</param>
/// <param name="userValidation">Validates requesting users before changes.</param>
/// <param name="logger">Logs command invocation and completion.</param>
/// <param name="responseBuilder">Sends localized results and validation failures.</param>
/// <param name="localizationService">Resolves the command description.</param>
/// <param name="commandHelper">Runs the existing voice-user validation flow.</param>
/// <param name="pagination">Owns caller-bound navigation for long read-only results.</param>
public sealed class FiltersCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<FiltersCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper,
    DiscordPaginationService pagination) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.Filters;

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.FiltersCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var argument = parts.Length > 1 ? parts[1].Trim() : null;
        var operation = argument?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
            .FirstOrDefault()
            ?.ToLowerInvariant();
        var customParts = argument?.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries) ?? [];
        var changesState = operation is "reset" or "preset"
            || (operation == "custom"
                && customParts.Length >= 2
                && customParts[1].ToLowerInvariant() is "save" or "apply")
            || (operation == "advanced"
                && argument!.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 2);
        IDiscordMember? member = null;

        if (changesState)
        {
            var validation = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message, default);

            if (validation is null)
            {
                return;
            }

            member = validation.Member;
        }

        var result = await lavaLinkService.FiltersAsync(message, member, argument, default);

        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        if (result.Pagination is { } pages)
        {
            await pagination.SendAsync(message, pages);
            logger.CommandExecuted(Name);
            return;
        }

        await responseBuilder.SendPlaybackResultAsync(message, result);
        logger.CommandExecuted(Name);
    }
}
