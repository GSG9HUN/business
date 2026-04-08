using DC_bot.Constants;
using DC_bot.Exceptions;
using DC_bot.Exceptions.Messaging;
using DC_bot.Helper.Factory;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.ProgressiveTimerInterface;
using DC_bot.Logging;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.AsyncEvents;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ReactionHandler(
    ILavaLinkService lavaLinkService,
    ILogger<ReactionHandler> logger,
    IProgressiveTimerService progressTimerService,
    ILocalizationService localizationService, bool isTestMode = false)
{
    private bool _isRegistered;
    private AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs>? _messageReactionAdded;
    private AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs>? _messageReactionRemoved;
    private Func<IDiscordChannel, DiscordClient, DiscordEmbed, Task>? _sendReactionControlMessage;

    public void RegisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            logger.ReactionHandlerAlreadyRegistered();
            return;
        }

        _messageReactionAdded = OnReactionAdded;
        _messageReactionRemoved = OnReactionRemoved;
        _sendReactionControlMessage = SendReactionControlMessage;

        lavaLinkService.TrackStarted += _sendReactionControlMessage;
        client.MessageReactionAdded += _messageReactionAdded;
        client.MessageReactionRemoved += _messageReactionRemoved;
        _isRegistered = true;
        logger.ReactionHandlerRegistered();
    }

    private async Task SendReactionControlMessage(IDiscordChannel textChannel, DiscordClient client, DiscordEmbed msg)
    {
        try
        {
            var controlText = $"🎵 **{localizationService.Get(LocalizationKeys.MusicControl)}** 🎵\n" +
                              $"⏸️ - {localizationService.Get(LocalizationKeys.PauseReaction)} " +
                              $"▶️ - {localizationService.Get(LocalizationKeys.ResumeReaction)} " +
                              $"⏭️ - {localizationService.Get(LocalizationKeys.SkipReaction)} " +
                              $"🔁 - {localizationService.Get(LocalizationKeys.RepeatReaction)}";

            var builder = new DiscordMessageBuilder()
                .WithContent(controlText)
                .AddEmbed(msg);

            var message = await textChannel.ToDiscordChannel().SendMessageAsync(builder);

            var wrappedMessage =
                DiscordMessageWrapperFactory.Create(message, textChannel.ToDiscordChannel(), client.CurrentUser);

            await progressTimerService.StartAsync(wrappedMessage, textChannel.Guild.Id);

            // Reakciók hozzáadása az üzenethez
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":pause_button:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":arrow_forward:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":track_next:"));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":repeat:"));

            logger.ReactionControlMessageSent();
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerMessageSendFailed(ex, "SendReactionControlMessage");
            throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
        }
    }

    private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot && !isTestMode) return;

        var (member, discordMessageWrapper, _) = await BuildContextAsync(args.Message, args.User, args.Channel);
        logger.ReactionAdded(args.Emoji.GetDiscordName(), args.User.Username);
        await ExecuteOnReactionAddedAsync(args.Emoji.Name, discordMessageWrapper, member);
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

    private async Task OnReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
    {
        if (args.User.IsBot && !isTestMode)
        {
            return;
        }

        var (member, discordMessageWrapper, _) = await BuildContextAsync(args.Message, args.User, args.Channel);
        logger.ReactionRemoved(args.Emoji.GetDiscordName(), args.User.Username);
        await ExecuteOnReactionRemovedAsync(args.Emoji.Name, discordMessageWrapper, member);
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

    internal async Task HandleReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (emojiName)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.PauseAsync(message, member);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(message, member);
                break;

            case "🔁": // Repeat emoji
                await message.RespondAsync(localizationService.Get(LocalizationKeys.ReactionHandlerRepeatOn));
                break;
        }
    }

    internal async Task HandleReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (emojiName)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.PauseAsync(message, member);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(message, member);
                break;

            case "🔁": // Repeat emoji
                await message.RespondAsync(localizationService.Get(LocalizationKeys.ReactionHandlerRepeatOff));
                break;
        }
    }

    private static async Task<(IDiscordMember member, IDiscordMessage messageWrapper, ulong guildId)> BuildContextAsync(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel)
    {
        var discordAuthor = new DiscordUserWrapper(user);
        var discordChannel = new DiscordChannelWrapper(channel);
        var member = await discordChannel.Guild.GetMemberAsync(discordAuthor.Id).ConfigureAwait(false);
        var discordMessageWrapper = DiscordMessageWrapperFactory.Create(message, channel, user);

        return (member, discordMessageWrapper, channel.Guild.Id);
    }

    internal void UnregisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            lavaLinkService.TrackStarted -= _sendReactionControlMessage;
            client.MessageReactionAdded -= _messageReactionAdded;
            client.MessageReactionRemoved -= _messageReactionRemoved;
            _messageReactionAdded = null;
            _messageReactionRemoved = null;
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