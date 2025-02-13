using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class SkipCommand(LavaLinkService lavaLinkService, ILogger<SkipCommand> logger) : ICommand
{
    public string Name => "skip";
    public string Description => "Skip the current track.";

    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id);

        if (member.IsBot)
        {
            return;
        }

        if (member.VoiceState?.Channel == null)
        {
            await message.RespondAsync($"You must be in a voice channel.");
            logger.LogInformation("You must be in a voice channel.");
            return;
        }

        await lavaLinkService.SkipAsync(message.Channel);
        logger.LogInformation("Skip command executed!");
    }
}