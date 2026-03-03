using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Logging;
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
        logger.CommandInvoked(Name);
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (!validationResult.IsValid)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;

        if (lavaLinkService.IsRepeating[guildId])
        {
            await responseBuilder.SendSuccessAsync(message, localizationService.Get(LocalizationKeys.RepeatListCommandTrackAlreadyRepeating));
            logger.CommandExecuted(Name);
            return;
        }

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            lavaLinkService.IsRepeatingList[guildId] = false;
            await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOff)}\n {lavaLinkService.GetCurrentTrackList(guildId)}");
            logger.CommandExecuted(Name);
            return;
        }

        lavaLinkService.IsRepeatingList[guildId] = true;
        await responseBuilder.SendSuccessAsync(message, $"{localizationService.Get(LocalizationKeys.RepeatListCommandRepeatingOn)}\n {lavaLinkService.GetCurrentTrackList(guildId)}");

        lavaLinkService.CloneQueue(guildId);

        logger.CommandExecuted(Name);
    }
}