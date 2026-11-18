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

/// <summary>Show or set player output volume (0-200%) through the existing music facade and localized response boundaries.</summary>
/// <param name="lavaLinkService">The existing music facade executing the operation.</param>
/// <param name="userValidation">Validates requesting members before audio mutations.</param>
/// <param name="logger">Logs command invocation and completion.</param>
/// <param name="responseBuilder">Sends localized usage, rejection, and success responses.</param>
/// <param name="localizationService">Resolves command descriptions.</param>
/// <param name="commandHelper">Runs the existing user validation flow.</param>
public sealed class VolumeCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<VolumeCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "volume";

    /// <inheritdoc />
    public string Description => localizationService.Get(DescriptionKey);

    /// <inheritdoc />
    public string DescriptionKey => LocalizationKeys.VolumeCommandDescription;

    /// <inheritdoc />
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var parts = message.Content.Split((char[]?)null, 2, StringSplitOptions.RemoveEmptyEntries);
        var argument = parts.Length > 1 ? parts[1].Trim() : null;

        IDiscordMember? member = null;

        if (argument is not null)
        {
            var validation = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message, default);

            if (validation is null)
            {
                return;
            }

            member = validation.Member;
        }

        var result = await lavaLinkService.VolumeAsync(message, member, argument, default);

        if (!result.Success)
        {
            await responseBuilder.SendValidationErrorAsync(message, result.ErrorCode ?? ValidationErrorKeys.LavalinkError, default);

            return;
        }

        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.PlaybackCommandResponse, result.Message);
        logger.CommandExecuted(Name);
    }
}
