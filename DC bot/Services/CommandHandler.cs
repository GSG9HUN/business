using DC_bot.Interface;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Services
{
    public class CommandHandler(IServiceProvider services, ILogger<CommandHandler> _logger)
    {
        public async Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (args.Message is not { } message) return;

            if (args.Author.IsBot) return;

            if (message.Content.StartsWith("!"))
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