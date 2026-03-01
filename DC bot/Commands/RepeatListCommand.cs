using DC_bot.Constants;
using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatListCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<RepeatListCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "repeatList";
    public string Description => localizationService.Get(LocalizationKeys.RepeatListCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;
        logger.LogInformation("Repeat list command invoked");

        if (lavaLinkService.IsRepeating[guildId])
        {
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatListCommandTrackAlreadyRepeating));
            logger.LogInformation("Repeat list command executed");
            return;
        }

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            lavaLinkService.IsRepeatingList[guildId] = false;
            await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOff)}\n {lavaLinkService.GetCurrentTrackList(guildId)}");
            logger.LogInformation("Repeat list command executed");
            return;
        }

        lavaLinkService.IsRepeatingList[guildId] = true;
        await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOn)}\n {lavaLinkService.GetCurrentTrackList(guildId)}");

        lavaLinkService.CloneQueue(guildId);

        logger.LogInformation("Repeat list command executed");
    }
}