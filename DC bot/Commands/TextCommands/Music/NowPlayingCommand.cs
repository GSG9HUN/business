using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Show the current track and playback details through the existing music facade and localized response boundaries.</summary>
/// <param name="lavaLinkService">The existing music facade executing the operation.</param>
/// <param name="logger">Logs command invocation and completion.</param>
/// <param name="responseBuilder">Sends localized usage, rejection, and success responses.</param>
/// <param name="localizationService">Resolves command descriptions.</param>
public sealed class NowPlayingCommand(
    ILavaLinkService lavaLinkService,
    ILogger<NowPlayingCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.NowPlaying;

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.NowPlayingCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var argument = parts.Length > 1 ? parts[1].Trim() : null;

        if (argument is not null)
        {
            await responseBuilder.SendUsageAsync(message, Name);
            return;
        }

        var result = await lavaLinkService.NowPlayingAsync(message, default);

        if (!result.Success && result.ErrorCode == LocalizationKeys.NowPlayingCommandNoTrack)
        {
            await responseBuilder.SendInfoAsync(message, LocalizationKeys.NowPlayingCommandNoTrack);
            return;
        }

        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responseBuilder.SendPlaybackResultAsync(message, result);
        logger.CommandExecuted(Name);
    }
}
