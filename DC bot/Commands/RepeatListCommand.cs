using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatListCommand(LavaLinkService lavaLinkService, ILogger<RepeatListCommand> logger) : ICommand
{
    public string Name => "repeatList";
    public string Description => "Repeats the current track list.";
    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var guildId = message.Channel.Guild.Id;
        
        logger.LogInformation("Repeat list command invoked");
        if (!lavaLinkService.IsRepeatingList[guildId])
        {
            lavaLinkService.IsRepeatingList[guildId] = true;
            await message.RespondAsync($"Repeat is on for current list:\n {lavaLinkService.GetCurrentTrackList(message.Channel.Guild.Id)}");
            lavaLinkService.CloneQueue(guildId);
        }
        else
        {
            lavaLinkService.IsRepeatingList[guildId] = false;
            await message.RespondAsync("Repeating is off.");
        }

        logger.LogInformation("Repeat list command executed");
    }
}