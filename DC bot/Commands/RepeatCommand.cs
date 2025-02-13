using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatCommand(LavaLinkService lavaLinkService, ILogger<RepeatCommand> logger) : ICommand
{
    public string Name => "repeat";
    public string Description => "Repeats a specified track infinitely.";


    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var guildId = message.Channel.Guild.Id;
        logger.LogInformation("Repeat command invoked");

        if (lavaLinkService.IsRepeatingList[guildId])
        {
            await message.Channel.SendMessageAsync("This list is already repeating.");
            logger.LogInformation("Repeat command executed");
            return;
        }

        if (lavaLinkService.IsRepeating[guildId])
        {
            lavaLinkService.IsRepeating[guildId] = false;
            await message.Channel.SendMessageAsync("Repeating is off.");
            logger.LogInformation("Repeat command executed");
            return;
        }
        lavaLinkService.IsRepeating[guildId] = true;
        await message.Channel.SendMessageAsync($"Repeat is on for : {lavaLinkService.GetCurrentTrack(guildId)}");
        
        logger.LogInformation("Repeat command executed");
     
    }
}