using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Logging;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class CommandHandlerService
{
    internal string? Prefix { get; set; }
    private readonly Dictionary<string, ICommand> _commands;
    private readonly ILogger<CommandHandlerService> _logger;
    private readonly ILocalizationService _localizationService;
    private readonly bool _isTestMode;
    private AsyncEventHandler<DiscordClient, MessageCreateEventArgs>? _messageHandler;
    private bool _isRegistered;


    public CommandHandlerService(IServiceProvider services, ILogger<CommandHandlerService> logger,
        ILocalizationService localizationService, BotSettings botSettings, bool isTestMode = false)
    {
        _logger = logger;
        _commands = services.GetServices<ICommand>().ToDictionary(c => c.Name, c => c);
        _localizationService = localizationService;
        _isTestMode = isTestMode;
        Prefix = botSettings.Prefix;
    }

    public void RegisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            _logger.CommandHandlerAlreadyRegistered();
            return;
        }

        _messageHandler = HandleCommandAsync; // Tároljuk a referenciát
        client.MessageCreated += _messageHandler;
        _logger.CommandHandlerRegistered();
        _isRegistered = true;
    }

    private async Task HandleCommandAsync(DiscordClient sender, MessageCreateEventArgs args)
    {
        if (Prefix == null)
        {
            _logger.CommandHandlerNoPrefix();
            return;
        }

        if (args.Message is not { } message) return;

        if (args.Author.IsBot && !_isTestMode) return;

        if (!message.Content.StartsWith(Prefix)) return;

        var commandName = message.Content.Substring(1).Split(' ')[0];
        using var scope = _logger.BeginCommandScope(commandName, args.Author.Id, args.Channel.Id, args.Guild?.Id);

        if (_commands.TryGetValue(commandName, out var command))
        {
            _logger.CommandInvoked(commandName);

            var discordAuthor = new DiscordUserWrapper(args.Author);
            var discordChannel = new DiscordChannelWrapper(args.Channel);
            var discordMessageWrapper = new DiscordMessageWrapper(args.Message.Id, args.Message.Content,
                discordChannel, discordAuthor, args.Message.CreationTimestamp,
                args.Message.Embeds.ToList(), args.Message.RespondAsync,
                args.Message.RespondAsync);

            await command.ExecuteAsync(discordMessageWrapper);

            _logger.CommandExecuted(commandName);
        }
        else
        {
            await message.Channel.SendMessageAsync(_localizationService.Get(LocalizationKeys.UnknownCommandError));
            _logger.CommandHandlerUnknownCommand();
        }
    }

    internal void UnRegisterHandler(DiscordClient client)
    {
        if (_messageHandler != null)
        {
            client.MessageCreated -= _messageHandler; // Eltávolítás az eltárolt referenciával
            _logger.CommandHandlerUnregistered();
            _messageHandler = null; // Megszüntetjük a referenciát
            _isRegistered = false;
        }
        else
        {
            _logger.CommandHandlerUnregisterNotRegistered();
        }
    }
}