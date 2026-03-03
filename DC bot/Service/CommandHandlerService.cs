using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Helper;
using DC_bot.Interface;
using DC_bot.Logging;
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

        var commandName = TryGetCommandName(message.Content, Prefix);
        if (string.IsNullOrWhiteSpace(commandName)) return;

        using var scope = _logger.BeginCommandScope(commandName, args.Author.Id, args.Channel.Id, args.Guild?.Id);

        if (_commands.TryGetValue(commandName, out var command))
        {
            _logger.CommandInvoked(commandName);

            var discordMessageWrapper = DiscordMessageWrapperFactory.Create(args.Message, args.Channel, args.Author);

            try
            {
                await command.ExecuteAsync(discordMessageWrapper);
                _logger.CommandExecuted(commandName);
            }
            catch (BotException botEx)
            {
                _logger.CommandExecutionFailed(botEx, commandName);
                // Custom bot exceptions are already logged with context
                throw;
            }
            catch (Exception ex)
            {
                _logger.CommandExecutionFailed(ex, commandName);
                throw;
            }
        }
        else
        {
            await message.Channel.SendMessageAsync(_localizationService.Get(LocalizationKeys.UnknownCommandError));
            _logger.CommandHandlerUnknownCommand();
        }
    }

    private static string? TryGetCommandName(string content, string prefix)
    {
        if (content.Length <= prefix.Length) return null;
        var remainder = content.Substring(prefix.Length).TrimStart();
        if (remainder.Length == 0) return null;
        var splitIndex = remainder.IndexOf(' ');
        return splitIndex >= 0 ? remainder[..splitIndex] : remainder;
    }

    internal void UnregisterHandler(DiscordClient client)
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