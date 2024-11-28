using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands
{
    public class HelpCommand(ILogger<HelpCommand> _logger) : ICommand
    {
        public string Name => "help";
        public string Description => "Lists all available commands";

        public async Task ExecuteAsync(DiscordMessage message)
        {
            var messageWrapper = new MessageWrapper(message);
            await ExecuteAsync(messageWrapper);
        }

        public async Task ExecuteAsync(IMessageWrapper message)
        {
            await message.RespondAsync($"Available commands:\n semmi");
            _logger.LogInformation("Help Command executed!");
        }
    }
}