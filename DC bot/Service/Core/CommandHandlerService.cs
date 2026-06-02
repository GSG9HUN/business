using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Helper.Factory;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

public class CommandHandlerService : IEventHandler<MessageCreatedEventArgs>
{
    private readonly Dictionary<string, ICommand> _commands;
    private readonly bool _isTestMode;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<CommandHandlerService> _logger;
    private readonly IDiscordMessageFactory _messageFactory;
    private bool _isRegistered;

    public CommandHandlerService(
        IServiceProvider services,
        ILogger<CommandHandlerService> logger,
        ILocalizationService localizationService,
        BotSettings botSettings,
        bool isTestMode = false,
        IDiscordMessageFactory? messageFactory = null)
    {
        _logger = logger;
        _commands = services.GetServices<ICommand>().ToDictionary(c => c.Name, c => c);
        _localizationService = localizationService;
        _messageFactory = messageFactory ?? new DiscordMessageWrapperFactory();
        _isTestMode = isTestMode;
        Prefix = botSettings.Prefix;
    }

    internal string? Prefix { get; set; }

    public Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        return _isRegistered ? HandleCommandAsync(sender, eventArgs) : Task.CompletedTask;
    }

    public void RegisterHandler(DiscordClient _)
    {
        if (_isRegistered)
        {
            _logger.CommandHandlerAlreadyRegistered();
            return;
        }

        _logger.CommandHandlerRegistered();
        _isRegistered = true;
    }

    private async Task HandleCommandAsync(DiscordClient sender, MessageCreatedEventArgs args)
    {
        try
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

            var guildId = TryGetGuildId(args);
            using var scope = _logger.BeginCommandScope(commandName, args.Author.Id, args.Channel.Id, guildId);

            if (_commands.TryGetValue(commandName, out var command))
            {
                _logger.CommandInvoked(commandName);

                var discordMessageWrapper = _messageFactory.Create(
                    args.Message,
                    args.Channel,
                    args.Author,
                    guild: args.Guild);

                await ExecuteCommandAsync(commandName, command, discordMessageWrapper);
            }
            else
            {
                await args.Channel.SendMessageAsync(
                    guildId == 0
                        ? _localizationService.Get(LocalizationKeys.UnknownCommandError)
                        : _localizationService.Get(guildId, LocalizationKeys.UnknownCommandError));
                _logger.CommandHandlerUnknownCommand();
            }
        }
        catch (Exception ex)
        {
            _logger.CommandExecutionFailed(ex, "message_created");
        }
    }

    private async Task ExecuteCommandAsync(string commandName, ICommand command, IDiscordMessage message)
    {
        try
        {
            await command.ExecuteAsync(message);
            _logger.CommandExecuted(commandName);
        }
        catch (BotException botEx)
        {
            _logger.CommandExecutionFailed(botEx, commandName);
        }
        catch (Exception ex)
        {
            _logger.CommandExecutionFailed(ex, commandName);
        }
    }

    private static string? TryGetCommandName(string content, string prefix)
    {
        if (content.Length <= prefix.Length) return null;
        var remainder = content[prefix.Length..].TrimStart();
        if (remainder.Length == 0) return null;
        var splitIndex = remainder.IndexOf(' ');
        return splitIndex >= 0 ? remainder[..splitIndex] : remainder;
    }

    private static ulong TryGetGuildId(MessageCreatedEventArgs args)
    {
        try
        {
            return args.Guild.Id;
        }
        catch
        {
            return 0;
        }
    }

    internal void UnregisterHandler(DiscordClient _)
    {
        if (_isRegistered)
        {
            _logger.CommandHandlerUnregistered();
            _isRegistered = false;
        }
        else
        {
            _logger.CommandHandlerUnregisterNotRegistered();
        }
    }
}
