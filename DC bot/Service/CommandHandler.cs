using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service
{
    public class CommandHandler
    {
        private readonly string? _prefix = Environment.GetEnvironmentVariable("BOT_PREFIX");
        private readonly Dictionary<string, ICommand> _commands;
        private readonly ILogger<CommandHandler> _logger;

        public CommandHandler(IServiceProvider services, ILogger<CommandHandler> logger)
        {
            _logger = logger;
            _commands = services.GetServices<ICommand>().ToDictionary(c => c.Name, c => c);
        }

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

            if (!message.Content.StartsWith(_prefix)) return;

            var commandName = message.Content.Substring(1).Split(' ')[0];
            if (_commands.TryGetValue(commandName, out var command))
            {
                var discordMessageWrapper = new DiscordMessageWrapper(args.Message.Id, args.Message.Content,
                    args.Channel, args.Author, args.Message.CreationTimestamp,
                    args.Message.Embeds.ToList(),args.Message.RespondAsync);

                await command.ExecuteAsync(discordMessageWrapper);
            }
            else
            {
                await message.Channel.SendMessageAsync("Unknown command. Use `!help` to see available commands.");
                _logger.LogInformation("Unknown command. Use `!help` to see available commands.");
            }
        }
    }
}