using System.Threading.Tasks;
using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands
{
    public class ResumeCommand(LavaLinkService _lavaLinkService, ILogger<ResumeCommand> _logger) : ICommand
    {
        public string Name => "resume";
        public string Description => "Resume the current music.";

        public async Task ExecuteAsync(DiscordMessage message)
        {
            var member = await message.Channel.Guild.GetMemberAsync(message.Author.Id);

            if (member.IsBot)
            {
                return;
            }

            if (member?.VoiceState?.Channel == null)
            {
                await message.Channel.SendMessageAsync("You must be in a voice channel.!");
                _logger.LogInformation("User not in a voice channel.");
                return;
            }

            await _lavaLinkService.ResumeAsync(message.Channel);
            _logger.LogInformation("Resume command executed!");
        }
    }  
}