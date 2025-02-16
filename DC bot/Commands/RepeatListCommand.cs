using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatListCommand(ILavaLinkService lavaLinkService, IUserValidationService userValidation, ILogger<RepeatListCommand> logger) : ICommand
{
    public string Name => "repeatList";
    public string Description => "Repeats the current track list.";
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            return;
        }
        
        var guildId = message.Channel.Guild.Id;
        logger.LogInformation("Repeat list command invoked");

        if (lavaLinkService.IsRepeating[guildId])
        {
            await message.Channel.SendMessageAsync("This track is already repeating.");
            logger.LogInformation("Repeat list command executed");
            return;
        }

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            lavaLinkService.IsRepeatingList[guildId] = false;
            await message.Channel.SendMessageAsync($"Repeating is off for the list:\n {lavaLinkService.GetCurrentTrackList(guildId)}");
            logger.LogInformation("Repeat list command executed");
            return;
        }    
        lavaLinkService.IsRepeatingList[guildId] = true;
        await message.Channel.SendMessageAsync($"Repeat is on for current list:\n {lavaLinkService.GetCurrentTrackList(guildId)}");
        lavaLinkService.CloneQueue(guildId);
        
        logger.LogInformation("Repeat list command executed");
    }
}