using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PauseCommand(LavaLinkService lavaLinkService, ILogger<PauseCommand> logger) : ICommand
{
    public string Name => "pause";
    public string Description => "Pause the current music.";

    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id);

        if (member.IsBot)
        {
            return;
        }

        if (member.VoiceState?.Channel == null)
        {
            await message.RespondAsync("You must be in a voice channel.!");
            logger.LogInformation("User not in a voice channel.");
            return;
        }

        await lavaLinkService.PauseAsync(message.Channel);
        logger.LogInformation("Pause command executed!");
    }
}