using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.Controls;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Routes music-config through the guild music-control service.</summary>
/// <param name="service">Executes guild controls and validation.</param>
/// <param name="localization">Resolves the command description.</param>
/// <param name="responses">Presents localized outcomes.</param>
/// <param name="logger">Records command invocation.</param>
public sealed class MusicConfigCommand(
    MusicControlSettingsService service,
    ILocalizationService localization,
    IResponseBuilder responses,
    ILogger<MusicConfigCommand> logger) : ICommand
{

    /// <inheritdoc />
    public string Name => "music-config";

    /// <inheritdoc />
    public string Description => localization.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.MusicConfigDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var result = await service.ConfigureAsync(message, parts.Length == 2 ? parts[1] : string.Empty);

        if (!result.Success)
        {
            if (result.ErrorCode == LocalizationKeys.MusicConfigUsage)
            {
                await responses.SendUsageAsync(message, Name);
                return;
            }

            await responses.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responses.SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, result.Message);
        logger.CommandExecuted(Name);
    }
}
