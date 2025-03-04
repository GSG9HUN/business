using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<RepeatCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "repeat";
    public string Description => localizationService.Get("repeat_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Repeat command invoked");
        var validationResult = await userValidation.ValidateUserAsync(message);

        if (validationResult.IsValid is false)
        {
            return;
        }

        var guildId = message.Channel.Guild.Id;

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            await message.Channel.SendMessageAsync(localizationService.Get("repeat_command_list_already_repeating"));
            logger.LogInformation("Repeat command executed");
            return;
        }

        if (lavaLinkService.IsRepeating[guildId])
        {
            lavaLinkService.IsRepeating[guildId] = false;
            await message.Channel.SendMessageAsync(localizationService.Get("repeat_command_repeating_off"));
            logger.LogInformation("Repeat command executed");
            return;
        }

        lavaLinkService.IsRepeating[guildId] = true;
        await message.Channel.SendMessageAsync($"{localizationService.Get("repeat_command_repeating_on")} {lavaLinkService.GetCurrentTrack(guildId)}");

        logger.LogInformation("Repeat command executed");
    }
}