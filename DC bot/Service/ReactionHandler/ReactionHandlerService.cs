using DC_bot.Exceptions;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.ReactionHandler;

/// <summary>
/// Registers reaction events and routes supported controls for bot control messages.
/// </summary>
/// <param name="lavaLinkService">The lava link service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="isTestMode">Whether explicit test-runtime behavior is enabled.</param>
/// <param name="controlMessageService">The control message service supplied for this operation.</param>
/// <param name="contextFactory">The context factory supplied for this operation.</param>
/// <param name="actionDispatcher">The action dispatcher supplied for this operation.</param>
public class ReactionHandlerService(
    ILavaLinkService lavaLinkService,
    ILogger<ReactionHandlerService> logger,
    ReactionControlMessageService controlMessageService,
    ReactionContextFactory contextFactory,
    ReactionActionDispatcher actionDispatcher,
    bool isTestMode = false) : IEventHandler<MessageReactionAddedEventArgs>, IEventHandler<MessageReactionRemovedEventArgs>
{
    private readonly ReactionActionDispatcher _actionDispatcher = actionDispatcher ?? throw new ArgumentNullException(nameof(actionDispatcher));
    private readonly ReactionContextFactory _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
    private readonly ReactionControlMessageService _controlMessageService = controlMessageService ?? throw new ArgumentNullException(nameof(controlMessageService));
    private bool _isRegistered;
    private Func<IDiscordChannel, DiscordEmbed, Task>? _sendReactionControlMessage;

    /// <summary>
    /// Dispatches the added or removed reaction only while the reaction handler is registered.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="eventArgs">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task HandleEventAsync(DiscordClient sender, MessageReactionAddedEventArgs eventArgs)
    {
        return _isRegistered ? OnReactionAdded(sender, eventArgs) : Task.CompletedTask;
    }

    /// <summary>
    /// Dispatches the added or removed reaction only while the reaction handler is registered.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="eventArgs">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public Task HandleEventAsync(DiscordClient sender, MessageReactionRemovedEventArgs eventArgs)
    {
        return _isRegistered ? OnReactionRemoved(sender, eventArgs) : Task.CompletedTask;
    }

    /// <summary>
    /// Registers runtime event handlers while preventing duplicate subscriptions.
    /// </summary>
    /// <param name="client">The client supplied for this operation.</param>
    public void RegisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            logger.ReactionHandlerAlreadyRegistered();
            return;
        }

        _sendReactionControlMessage = (textChannel, embed) => _controlMessageService.SendAsync(textChannel, client, embed, default);
        lavaLinkService.TrackStarted += _sendReactionControlMessage;
        _isRegistered = true;
        logger.ReactionHandlerRegistered();
    }

    /// <summary>
    /// Processes on reaction added within the reaction handler service boundary using the supplied context.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddedEventArgs args)
    {
        if (args.User.IsBot && !isTestMode)
        {
            return;
        }

        try
        {
            var context = await _contextFactory.CreateAsync(args.Message, args.User, args.Channel, args.Guild, default);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionAdded(emojiName, args.User.Username);
            await ExecuteOnReactionAddedAsync(emojiName, context.Message, context.Member);
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionAdded (Context Build)");
        }
    }

    /// <summary>
    /// Processes execute on reaction added within the reaction handler service boundary using the supplied context.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Processes on reaction removed within the reaction handler service boundary using the supplied context.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="args">The event payload supplied by the Discord or Lavalink SDK.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task OnReactionRemoved(DiscordClient sender, MessageReactionRemovedEventArgs args)
    {
        if (args.User.IsBot && !isTestMode)
        {
            return;
        }

        try
        {
            var context = await _contextFactory.CreateAsync(args.Message, args.User, args.Channel, args.Guild, default);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionRemoved(emojiName, args.User.Username);
            await ExecuteOnReactionRemovedAsync(emojiName, context.Message, context.Member);
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerOperationFailed(ex, "OnReactionRemoved (Context Build)");
        }
    }

    /// <summary>
    /// Processes execute on reaction removed within the reaction handler service boundary using the supplied context.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
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

    /// <summary>
    /// Routes the reaction added operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    internal Task HandleReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        return _actionDispatcher.DispatchAddedAsync(emojiName, message, member, default);
    }

    /// <summary>
    /// Routes the reaction removed operation through its resolved context and records the outcome.
    /// </summary>
    /// <param name="emojiName">The emoji name supplied for this operation.</param>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <param name="member">The member whose guild and voice context are used by the operation.</param>
    /// <returns>A task representing completion of this operation.</returns>
    internal Task HandleReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        return _actionDispatcher.DispatchRemovedAsync(emojiName, message, member, default);
    }

    /// <summary>
    /// Removes registered event subscriptions and detaches their runtime callbacks.
    /// </summary>
    /// <param name="_">The _ supplied for this operation.</param>
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
