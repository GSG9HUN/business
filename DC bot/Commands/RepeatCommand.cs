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

        if (!lavaLinkService.IsRepeating[guildId])
        {
            lavaLinkService.IsRepeating[guildId] = true;
            await message.RespondAsync($"Repeat is on for : {lavaLinkService.GetCurrentTrack(guildId)}");
        }
        else
        {
            lavaLinkService.IsRepeating[guildId] = false;
            await message.RespondAsync("Repeating is off.");
        }

        logger.LogInformation("Repeat command executed");
    }
}