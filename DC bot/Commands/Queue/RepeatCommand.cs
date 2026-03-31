using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.Queue;

public class RepeatCommand(
    IRepeatService repeatService,
    ICurrentTrackService currentTrackService,
    IUserValidationService userValidation,
    ILogger<RepeatCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "repeat";
    public string Description => localizationService.Get(LocalizationKeys.RepeatCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;

        if (repeatService.IsRepeatingList(guildId))
        {
            await responseBuilder.SendSuccessAsync(message,
                localizationService.Get(LocalizationKeys.RepeatCommandListAlreadyRepeating));
            logger.CommandExecuted(Name);
            return;
        }

        if (repeatService.IsRepeating(guildId))
        {
            repeatService.SetRepeating(guildId, false);
            await responseBuilder.SendSuccessAsync(message,
                localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOff));
            logger.CommandExecuted(Name);
            return;
        }

        repeatService.SetRepeating(guildId, true);
        await responseBuilder.SendSuccessAsync(message,
            $"{localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOn)} {currentTrackService.GetCurrentTrackFormatted(guildId)}");

        logger.CommandExecuted(Name);
    }
}