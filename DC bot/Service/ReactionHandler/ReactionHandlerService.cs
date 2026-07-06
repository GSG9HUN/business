using DC_bot.Exceptions;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.ReactionHandler;

public class ReactionHandlerService(
    ILavaLinkService lavaLinkService,
    ILogger<ReactionHandlerService> logger,
    IProgressiveTimerService progressTimerService,
    ILocalizationService localizationService,
    bool isTestMode = false,
    ReactionControlMessageService? controlMessageService = null,
    ReactionContextFactory? contextFactory = null,
    ReactionActionDispatcher? actionDispatcher = null)
    : IEventHandler<MessageReactionAddedEventArgs>, IEventHandler<MessageReactionRemovedEventArgs>
{
    private readonly ReactionActionDispatcher _actionDispatcher = actionDispatcher ?? new ReactionActionDispatcher(lavaLinkService, localizationService);
    private readonly ReactionContextFactory _contextFactory = contextFactory ?? new ReactionContextFactory();
    private readonly ReactionControlMessageService _controlMessageService = controlMessageService ?? new ReactionControlMessageService(
        progressTimerService,
        localizationService,
        logger);

    private bool _isRegistered;
    private Func<IDiscordChannel, DiscordEmbed, Task>? _sendReactionControlMessage;

    public Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs eventArgs)
    {
        return _isRegistered ? OnReactionAdded(sender, eventArgs) : Task.CompletedTask;
    }

    public Task HandleEventAsync(DiscordClient sender, MessageReactionRemovedEventArgs eventArgs)
    {
        return _isRegistered ? OnReactionRemoved(sender, eventArgs) : Task.CompletedTask;
    }

    public void RegisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            logger.ReactionHandlerAlreadyRegistered();
            return;
        }

        _sendReactionControlMessage = (textChannel, embed) => _controlMessageService.SendAsync(textChannel, client, embed);

        lavaLinkService.TrackStarted += _sendReactionControlMessage;
        _isRegistered = true;
        logger.ReactionHandlerRegistered();
    }

    private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddedEventArgs args)
    {
        if (args.User.IsBot && !isTestMode) return;

        try
        {
            var context = await _contextFactory.CreateAsync(args.Message, args.User, args.Channel, args.Guild);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionAdded(emojiName, args.User.Username);
            await ExecuteOnReactionAddedAsync(emojiName, context.Message, context.Member);
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionAdded (Context Build)");
        }
    }

    internal async Task ExecuteOnReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        try
        {
            await HandleReactionAddedAsync(emojiName, message, member);
        }
        catch (BotException botEx)
        {
            logger.ReactionHandlerOperationFailed(botEx, "OnReactionAdded");
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionAdded");
        }
    }

    private async Task OnReactionRemoved(DiscordClient sender, MessageReactionRemovedEventArgs args)
    {
        if (args.User.IsBot && !isTestMode) return;

        try
        {
            var context = await _contextFactory.CreateAsync(args.Message, args.User, args.Channel, args.Guild);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionRemoved(emojiName, args.User.Username);
            await ExecuteOnReactionRemovedAsync(emojiName, context.Message, context.Member);
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionRemoved (Context Build)");
        }
    }

    internal async Task ExecuteOnReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        try
        {
            await HandleReactionRemovedAsync(emojiName, message, member);
        }
        catch (BotException botEx)
        {
            logger.ReactionHandlerOperationFailed(botEx, "OnReactionRemoved");
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionRemoved");
        }
    }

    internal Task HandleReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        return _actionDispatcher.DispatchAddedAsync(emojiName, message, member);
    }

    internal Task HandleReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        return _actionDispatcher.DispatchRemovedAsync(emojiName, message, member);
    }
    internal void UnregisterHandler(DiscordClient _)
    {
        if (_isRegistered)
        {
            lavaLinkService.TrackStarted -= _sendReactionControlMessage;
            _sendReactionControlMessage = null;
            _isRegistered = false;
            logger.ReactionHandlerUnregistered();
        }
        else
        {
            logger.ReactionHandlerUnregisterNotRegistered();
        }
    }
}
