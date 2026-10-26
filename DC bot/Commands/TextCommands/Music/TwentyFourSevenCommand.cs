using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Persistence.Models.Guilds;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DC_bot.Service.Music.Lifecycle;
using DC_bot.Service.Music.Lifecycle.PersistentVoice;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Music;

/// <summary>
/// Changes or displays the guild's persistent voice connection mode.
/// </summary>
/// <param name="twentyFourSevenService">Coordinates preferences and initial voice setup.</param>
/// <param name="userValidation">Rejects bot users and validates voice membership when enabling.</param>
/// <param name="commandHelper">Runs shared user validation and validation responses.</param>
/// <param name="responseBuilder">Sends localized command responses.</param>
/// <param name="localizationService">Resolves command descriptions and guild-only responses.</param>
/// <param name="logger">Records command execution and failures.</param>
public sealed class TwentyFourSevenCommand(
    TwentyFourSevenService twentyFourSevenService,
    IUserValidationService userValidation,
    ICommandHelper commandHelper,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ILogger<TwentyFourSevenCommand> logger) : ICommand
{

    /// <inheritdoc />
    public string Name => "24-7";

    /// <inheritdoc />
    public string Description =>
        localizationService.Get(LocalizationKeys.TwentyFourSevenCommandDescription);

    /// <summary>
    /// Displays the current mode or changes it using an explicit boolean argument.
    /// </summary>
    /// <param name="message">The command message or adapted slash interaction.</param>
    /// <returns>A task that completes after the command response is sent.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);

        if (userValidation.IsBotUser(message))
        {
            return;
        }

        if (message.Channel?.Guild is not { Id: > 0 } guild)
        {
            await message.RespondAsync(
                localizationService.Get(LocalizationKeys.SlashCommandGuildOnly));

            return;
        }

        var arguments = message.Content.Split(
            (char[]?)null,
            StringSplitOptions.RemoveEmptyEntries);

        bool? enabled = null;

        if (arguments.Length > 1)
        {

            if (arguments.Length != 2
                || !bool.TryParse(arguments[1], out var parsed))
            {
                await responseBuilder.SendWarningAsync(
                    message,
                    LocalizationKeys.TwentyFourSevenCommandInvalidArgument,
                    CancellationToken.None);

                return;
            }

            enabled = parsed;
        }

        IDiscordChannel? voiceChannel = null;

        if (enabled == true)
        {
            var validation = await commandHelper.TryValidateUserAsync(
                userValidation,
                responseBuilder,
                message,
                CancellationToken.None);

            if (validation is null)
            {
                return;
            }

            voiceChannel = validation.Member?.VoiceState?.Channel;
        }

        TwentyFourSevenSettingsRecord? settings;

        try
        {
            settings = enabled.HasValue
                ? await twentyFourSevenService.SetAsync(
                    message,
                    enabled.Value,
                    voiceChannel,
                    CancellationToken.None)
                : await twentyFourSevenService.GetAsync(
                    guild.Id,
                    CancellationToken.None);
        }
        catch (Exception exception)
        {
            logger.CommandExecutionFailed(exception, Name);

            await responseBuilder.SendErrorAsync(
                message,
                LocalizationKeys.TwentyFourSevenCommandFailed,
                CancellationToken.None);

            return;
        }

        if (settings is null)
        {
            // Player connection validation already sent the failure response.
            return;
        }

        var responseKey = settings.IsEnabled
            ? LocalizationKeys.TwentyFourSevenCommandEnabled
            : LocalizationKeys.TwentyFourSevenCommandDisabled;

        await responseBuilder.SendSuccessAsync(
            message,
            responseKey,
            CancellationToken.None);

        logger.CommandExecuted(Name);
    }
}