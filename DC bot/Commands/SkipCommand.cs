using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class SkipCommand(LavaLinkService _lavaLinkService, ILogger<SkipCommand> _logger) : ICommand
{
    public string Name => "skip";
    public string Description => "Skip the current track.";

    public async Task ExecuteAsync(DiscordMessage message)
    {
        var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id);
        if (member.IsBot)
        {
            return;
        }

        if (member.VoiceState.Channel == null)
        {
            await message.Channel.SendMessageAsync($"You must be in a voice channel.");
            _logger.LogInformation("You must be in a voice channel.");
            return;
        }

        await _lavaLinkService.SkipAsync(message.Channel);
        _logger.LogInformation("Skip command executed!");
    }
}