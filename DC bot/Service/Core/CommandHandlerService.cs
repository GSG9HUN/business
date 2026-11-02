using DC_bot.Configuration;
using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Core;

/// <summary>
/// Dispatches prefixed Discord messages through the registry and contains command execution failures.
/// </summary>
public class CommandHandlerService : IEventHandler<MessageCreatedEventArgs>
{
    private readonly ICommandRegistry _commandRegistry;
    private readonly bool _isTestMode;
    private readonly ILocalizationService _localizationService;
    private readonly ILogger<CommandHandlerService> _logger;
    private readonly IDiscordMessageFactory _messageFactory;
    private bool _isRegistered;

    /// <summary>
    /// Initializes CommandHandlerService with the supplied dependencies and settings.
    /// </summary>
    /// <param name="commandRegistry">The command registry dependency used by this boundary.</param>
    /// <param name="logger">The structured logger used at this operation boundary.</param>
    /// <param name="localizationService">The localization service dependency used by this boundary.</param>
    /// <param name="botSettings">The bot settings supplied for this operation.</param>
    /// <param name="messageFactory">The Discord message factory dependency used by this boundary.</param>
    /// <param name="isTestMode">Whether explicit test-runtime behavior is enabled.</param>
    public CommandHandlerService(ICommandRegistry commandRegistry, ILogger<CommandHandlerService> logger, ILocalizationService localizationService, BotSettings botSettings, IDiscordMessageFactory messageFactory, bool isTestMode = false)
    {
        _logger = logger;
        _commandRegistry = commandRegistry;
        _ = _commandRegistry.Commands;
        _localizationService = localizationService;
        _messageFactory = messageFactory ?? throw new ArgumentNullException(nameof(messageFactory));
        _isTestMode = isTestMode;
        Prefix = botSettings.Prefix;
    }

    internal string? Prefix { get; set; }

    /// <summary>
    /// Dispatches a message-created event only while this command handler is registered.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="eventArgs">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task HandleEventAsync(DiscordClient sender, MessageCreatedEventArgs eventArgs)
    {
        return _isRegistered ? HandleCommandAsync(sender, eventArgs) : Task.CompletedTask;
    }

    /// <summary>
    /// Registers runtime event handlers while preventing duplicate subscriptions.
    /// </summary>
    /// <param name="_">The _ supplied for this operation.</param>
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

    /// <summary>
    /// Filters bot and non-command messages, resolves the command name, and dispatches the matching registered command.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task HandleCommandAsync(DiscordClient sender, MessageCreatedEventArgs args)
    {
        try
        {
            if (Prefix == null)
            {
                _logger.CommandHandlerNoPrefix();
                return;
            }

            if (args.Message is not { } message)
            {
                return;
            }

            if (args.Author.IsBot && !_isTestMode)
            {
                return;
            }

            if (!message.Content.StartsWith(Prefix))
            {
                return;
            }

            var commandName = TryGetCommandName(message.Content, Prefix);

            if (string.IsNullOrWhiteSpace(commandName))
            {
                return;
            }

            var guildId = TryGetGuildId(args);
            using var scope = _logger.BeginCommandScope(commandName, args.Author.Id, args.Channel.Id, guildId);

            if (_commandRegistry.TryGetCommand(commandName, out var command))
            {
                _logger.CommandInvoked(commandName);
                var discordMessageWrapper = _messageFactory.Create(args.Message, args.Channel, args.Author, guild: args.Guild);
                await ExecuteCommandAsync(commandName, command, discordMessageWrapper);
            }
            else
            {
                await args.Channel.SendMessageAsync(guildId == 0 ? _localizationService.Get(LocalizationKeys.UnknownCommandError) : _localizationService.Get(guildId, LocalizationKeys.UnknownCommandError));
                _logger.CommandHandlerUnknownCommand();
            }
        }
        catch (Exception ex)
        {
            _logger.CommandExecutionFailed(ex, "message_created");
        }
    }

    /// <summary>
    /// Runs the resolved command inside a logging scope and contains execution failures.
    /// </summary>
    /// <param name="commandName">The command name supplied for this operation.</param>
    /// <param name="command">The command dependency used by this boundary.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Parses a prefixed command name from an incoming Discord message.
    /// </summary>
    /// <param name="content">The content supplied for this operation.</param>
    /// <param name="prefix">The prefix supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string? TryGetCommandName(string content, string prefix)
    {
        if (content.Length <= prefix.Length)
        {
            return null;
        }

        var remainder = content[prefix.Length..].TrimStart();

        if (remainder.Length == 0)
        {
            return null;
        }

        var splitIndex = remainder.IndexOf(' ');
        return splitIndex >= 0 ? remainder[..splitIndex] : remainder;
    }

    /// <summary>
    /// Extracts the available guild identity from Discord event data.
    /// </summary>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
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

    /// <summary>
    /// Removes registered event subscriptions and detaches their runtime callbacks.
    /// </summary>
    /// <param name="_">The _ supplied for this operation.</param>
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
