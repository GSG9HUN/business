using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ResumeCommand(LavaLinkService lavaLinkService, ILogger<ResumeCommand> logger) : ICommand
{
    public string Name => "resume";
    public string Description => "Resume the current music.";

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

        await lavaLinkService.ResumeAsync(message.Channel);
        logger.LogInformation("Resume command executed!");
    }
}