using DC_bot.Constants;
using DC_bot.Interface;
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
        logger.LogInformation("Repeat command invoked");
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatCommandListAlreadyRepeating));
            logger.LogInformation("Repeat command executed");
            return;
        }

        if (lavaLinkService.IsRepeating[guildId])
        {
            lavaLinkService.IsRepeating[guildId] = false;
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOff));
            logger.LogInformation("Repeat command executed");
            return;
        }

        lavaLinkService.IsRepeating[guildId] = true;
        await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatCommandRepeatingOn)} {lavaLinkService.GetCurrentTrack(guildId)}");

        logger.LogInformation("Repeat command executed");
    }
}