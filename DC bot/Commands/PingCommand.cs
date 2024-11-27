using DC_bot.Interface;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands
{
    public class PingCommand(ILogger<PingCommand> _logger) : ICommand
    {
        public string Name => "ping";
        public string Description => "Answer with pong!";

        public async Task ExecuteAsync(DiscordMessage message)
        {
            await message.Channel.SendMessageAsync("Pong!");
            _logger.LogInformation("Ping command executed!");
        }
    }
}