using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>Handles queuefind waiting-queue operations through the existing facade and localized response pipeline.</summary>
/// <param name="lavaLinkService">The existing music facade.</param>
/// <param name="logger">Logs command invocation and completion.</param>
/// <param name="responseBuilder">Sends localized results and validation failures.</param>
/// <param name="localizationService">Resolves the command description.</param>
public sealed class QueueFindCommand(
    ILavaLinkService lavaLinkService,
    ILogger<QueueFindCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{

    /// <inheritdoc />
    public string Name => "queuefind";

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.QueueFindCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var argument = parts.Length > 1 ? parts[1].Trim() : null;
        var result = await lavaLinkService.QueueCommandAsync(message, null, Name, argument ?? string.Empty, default);

        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, result.Message);
        logger.CommandExecuted(Name);
    }
}
