using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<RepeatCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "repeat";
    public string Description => localizationService.Get(LocalizationKeys.RepeatCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (!validationResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatCommandListAlreadyRepeating));
            logger.CommandExecuted(Name);
            return;
        }

        if (lavaLinkService.IsRepeating[guildId])
        {
            lavaLinkService.IsRepeating[guildId] = false;
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOff));
            logger.CommandExecuted(Name);
            return;
        }

        lavaLinkService.IsRepeating[guildId] = true;
        await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOn)} {lavaLinkService.GetCurrentTrack(guildId)}");

        logger.CommandExecuted(Name);
    }
}