using DC_bot.Interface;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands
{
    public class HelpCommand(ILogger<PlayCommand> _logger) : ICommand
    {
        public string Name => "help";
        public string Description => "Lists all available commands";

        public async Task ExecuteAsync(DiscordMessage message)
        {
            await message.RespondAsync($"Available commands:\n semmi");
            _logger.LogInformation("Help Command executed!");
        }
    }
}