using System;
using System.Linq;
using System.Threading.Tasks;
using DC_bot.Interface;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Services
{
    public class CommandHandler(IServiceProvider services, ILogger<CommandHandler> _logger)
    {
        private readonly string? _prefix = Environment.GetEnvironmentVariable("BOT_PREFIX");

        public void RegisterHandler(DiscordClient client)
        {
            client.MessageCreated += HandleCommandAsync;
            _logger.LogInformation("Registered command handler");
        }
        
        private async Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (_prefix == null)
            {
                _logger.LogError("No prefix provided");
                return;
            }

            if (args.Message is not { } message) return;

            if (args.Author.IsBot) return;

            if (message.Content.StartsWith(_prefix))
            {
                var commandName = message.Content.Substring(1).Split(' ')[0];
                var command = services.GetServices<ICommand>().FirstOrDefault(command => command.Name == commandName);

                if (command != null)
                {
                    await command.ExecuteAsync(message);
                }
                else
                {
                    await message.Channel.SendMessageAsync("Unknown command. Use `!help` to see available commands.");
                    _logger.LogInformation("Unknown command. Use `!help` to see available commands.");
                }
            }
        }
    }
}