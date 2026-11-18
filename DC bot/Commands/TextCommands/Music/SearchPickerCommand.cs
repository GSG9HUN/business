using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Discovery;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Routes searchpicker through the shared discovery service and localized result presentation.</summary>
/// <param name="service">Executes the discovery behavior with its ownership and validation rules.</param>
/// <param name="localization">Resolves the command description.</param>
/// <param name="responses">Presents localized success and validation results.</param>
/// <param name="logger">Records command invocation and completion.</param>
public sealed class SearchPickerCommand(
    ISearchPickerHandler service,
    ILocalizationService localization,
    IResponseBuilder responses,
    ILogger<SearchPickerCommand> logger) : ICommand
{

    /// <inheritdoc />
    public string Name => "searchpicker";

    /// <inheritdoc />
    public string Description => localization.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.SearchPickerCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var result = await service.ConfigureAsync(message, parts.Length == 2 ? parts[1] : string.Empty);

        if (!result.Success)
        {
            await responses.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responses.SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, result.Message);
        logger.CommandExecuted(Name);
    }
}
