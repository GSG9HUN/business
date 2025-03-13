using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service
{
    public class CommandHandlerService
    {
        internal string? prefix { get; set; }
        private readonly Dictionary<string, ICommand> _commands;
        private readonly ILogger<CommandHandlerService> _logger;
        private readonly ILocalizationService _localizationService;
        private readonly bool _isTestMode;

        public CommandHandlerService(IServiceProvider services, ILogger<CommandHandlerService> logger,
            ILocalizationService localizationService, bool isTestMode = false)
        {
            _logger = logger;
            _commands = services.GetServices<ICommand>().ToDictionary(c => c.Name, c => c);
            _localizationService = localizationService;
            _isTestMode = isTestMode;
            prefix = Environment.GetEnvironmentVariable("BOT_PREFIX");
        }

        public void RegisterHandler(DiscordClient client)
        {
            client.MessageCreated += HandleCommandAsync;
            _logger.LogInformation("Registered command handler");
        }

        private async Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs args)
        {
            if (prefix == null)
            {
                _logger.LogError("No prefix provided");
                return;
            }

            if (args.Message is not { } message) return;

            if (args.Author.IsBot && !_isTestMode) return;

            if (!message.Content.StartsWith(prefix)) return;

            var commandName = message.Content.Substring(1).Split(' ')[0];
            if (_commands.TryGetValue(commandName, out var command))
            {
                var discordAuthor = new DiscordUserWrapper(args.Author);
                var discordChannel = new DiscordChannelWrapper(args.Channel);
                var discordMessageWrapper = new DiscordMessageWrapper(args.Message.Id, args.Message.Content,
                    discordChannel, discordAuthor, args.Message.CreationTimestamp,
                    args.Message.Embeds.ToList(), args.Message.RespondAsync,
                    args.Message.RespondAsync);

                await command.ExecuteAsync(discordMessageWrapper);
            }
            else
            {
                await message.Channel.SendMessageAsync(_localizationService.Get("unknown_command_error"));
                _logger.LogInformation("Unknown command. Use `!help` to see available commands.");
            }
        }
    }
}