using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>Show or change pitch (0.5-2.0) through the existing facade and localized response pipeline.</summary>
/// <param name="lavaLinkService">The existing music facade.</param>
/// <param name="userValidation">Validates requesting users before changes.</param>
/// <param name="logger">Logs command invocation and completion.</param>
/// <param name="responseBuilder">Sends localized results and validation failures.</param>
/// <param name="localizationService">Resolves the command description.</param>
/// <param name="commandHelper">Runs the existing voice-user validation flow.</param>
public sealed class PitchCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<PitchCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => CommandNames.Pitch;

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.PitchCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var argument = parts.Length > 1 ? parts[1].Trim() : null;
        var changesState = argument is not null;
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

        var result = await lavaLinkService.PitchAsync(message, member, argument, default);

        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responseBuilder.SendPlaybackResultAsync(message, result);
        logger.CommandExecuted(Name);
    }
}
