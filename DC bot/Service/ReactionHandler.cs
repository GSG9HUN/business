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
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ReactionHandler(
    ILavaLinkService lavaLinkService,
    ILogger<ReactionHandler> logger,
    IProgressiveTimerService progressTimerService,
    ILocalizationService localizationService, bool isTestMode = false)
    : IEventHandler<MessageReactionAddedEventArgs>, IEventHandler<MessageReactionRemovedEventArgs>
{
    private const string PauseEmojiName = ":pause_button:";
    private const string ResumeEmojiName = ":arrow_forward:";
    private const string SkipEmojiName = ":track_next:";
    private const string RepeatEmojiName = ":repeat:";
    private const string PauseEmoji = "\u23F8\uFE0F";
    private const string ResumeEmoji = "\u25B6\uFE0F";
    private const string SkipEmoji = "\u23ED\uFE0F";
    private const string RepeatEmoji = "\uD83D\uDD01";

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

        _sendReactionControlMessage = (textChannel, msg) => SendReactionControlMessage(textChannel, client, msg);

        lavaLinkService.TrackStarted += _sendReactionControlMessage;
        _isRegistered = true;
        logger.ReactionHandlerRegistered();
    }

    private async Task SendReactionControlMessage(IDiscordChannel textChannel, DiscordClient client, DiscordEmbed msg)
    {
        try
        {
            var guildId = textChannel.Guild.Id;
            var controlText = $"**{localizationService.Get(guildId, LocalizationKeys.MusicControl)}**\n" +
                              $"{PauseEmoji} - {localizationService.Get(guildId, LocalizationKeys.PauseReaction)} " +
                              $"{ResumeEmoji} - {localizationService.Get(guildId, LocalizationKeys.ResumeReaction)} " +
                              $"{SkipEmoji} - {localizationService.Get(guildId, LocalizationKeys.SkipReaction)} " +
                              $"{RepeatEmoji} - {localizationService.Get(guildId, LocalizationKeys.RepeatReaction)}";

            var builder = new DiscordMessageBuilder()
                .WithContent(controlText)
                .AddEmbed(msg);

            var message = await textChannel.ToDiscordChannel().SendMessageAsync(builder);

            var wrappedMessage =
                DiscordMessageWrapperFactory.Create(message, textChannel.ToDiscordChannel(), client.CurrentUser);

            await progressTimerService.StartAsync(wrappedMessage, textChannel.Guild.Id);

            await message.CreateReactionAsync(DiscordEmoji.FromName(client, PauseEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, ResumeEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, SkipEmojiName));
            await message.CreateReactionAsync(DiscordEmoji.FromName(client, RepeatEmojiName));

            logger.ReactionControlMessageSent();
        }
        catch (Exception ex)
        {
            logger.ReactionHandlerMessageSendFailed(ex, "SendReactionControlMessage");
            throw new MessageSendException("SendReactionControlMessage", "Failed to send reaction control message", ex);
        }
    }

    private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddedEventArgs args)
    {
        if (args.User.IsBot && !isTestMode) return;

        try
        {
            var (member, discordMessageWrapper, _) = await BuildContextAsync(
                args.Message,
                args.User,
                args.Channel,
                args.Guild);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionAdded(emojiName, args.User.Username);
            await ExecuteOnReactionAddedAsync(emojiName, discordMessageWrapper, member);
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
            var (member, discordMessageWrapper, _) = await BuildContextAsync(
                args.Message,
                args.User,
                args.Channel,
                args.Guild);
            var emojiName = args.Emoji.GetDiscordName();
            logger.ReactionRemoved(emojiName, args.User.Username);
            await ExecuteOnReactionRemovedAsync(emojiName, discordMessageWrapper, member);
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

    internal async Task HandleReactionAddedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (NormalizeEmojiName(emojiName))
        {
            case PauseEmojiName:
                await lavaLinkService.PauseAsync(message, member);
                break;

            case ResumeEmojiName:
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member);
                break;

            case RepeatEmojiName:
                await message.RespondAsync(
                    localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOn));
                break;
        }
    }

    internal async Task HandleReactionRemovedAsync(string emojiName, IDiscordMessage message, IDiscordMember member)
    {
        switch (NormalizeEmojiName(emojiName))
        {
            case PauseEmojiName:
                await lavaLinkService.ResumeAsync(message, member);
                break;

            case ResumeEmojiName:
                await lavaLinkService.PauseAsync(message, member);
                break;

            case SkipEmojiName:
                await lavaLinkService.SkipAsync(message, member);
                break;

            case RepeatEmojiName:
                await message.RespondAsync(
                    localizationService.Get(message.Channel.Guild.Id, LocalizationKeys.ReactionHandlerRepeatOff));
                break;
        }
    }

    private static string NormalizeEmojiName(string emojiName)
    {
        return emojiName switch
        {
            PauseEmoji => PauseEmojiName,
            ResumeEmoji => ResumeEmojiName,
            SkipEmoji => SkipEmojiName,
            RepeatEmoji => RepeatEmojiName,
            _ => emojiName
        };
    }

    private static async Task<(IDiscordMember member, IDiscordMessage messageWrapper, ulong guildId)> BuildContextAsync(
        DiscordMessage message,
        DiscordUser user,
        DiscordChannel channel,
        DiscordGuild? guild = null)
    {
        var resolvedGuild = guild ?? (user as DiscordMember)?.Guild ?? channel.Guild;
        if (resolvedGuild is null)
        {
            throw new InvalidOperationException("Reaction event context does not contain a guild.");
        }

        var discordAuthor = new DiscordUserWrapper(user);
        var discordChannel = new DiscordChannelWrapper(channel, guild: resolvedGuild);
        var member = user is DiscordMember discordMember
            ? new DiscordMemberWrapper(discordMember)
            : await discordChannel.Guild.GetMemberAsync(discordAuthor.Id).ConfigureAwait(false);
        var discordMessageWrapper = DiscordMessageWrapperFactory.Create(message, channel, user, guild: resolvedGuild);

        return (member, discordMessageWrapper, resolvedGuild.Id);
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
