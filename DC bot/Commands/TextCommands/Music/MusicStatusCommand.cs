using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.Controls;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Routes music-status through the guild music-control service.</summary>
/// <param name="service">Executes guild controls and validation.</param>
/// <param name="localization">Resolves the command description.</param>
/// <param name="responses">Presents localized outcomes.</param>
/// <param name="logger">Records command invocation.</param>
public sealed class MusicStatusCommand(
    MusicControlSettingsService service,
    ILocalizationService localization,
    IResponseBuilder responses,
    ILogger<MusicStatusCommand> logger) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.MusicStatus;

    /// <inheritdoc />
    public string Description => localization.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.MusicStatusDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        if ((message.Content ?? string.Empty).Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries).Length > 1)
        {
            await responses.SendUsageAsync(message, Name);
            return;
        }

        logger.CommandInvoked(Name);

        var result = await service.StatusAsync(message);

        if (!result.Success)
        {
            await responses.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responses.SendPlaybackResultAsync(message, result);
        logger.CommandExecuted(Name);
    }
}
