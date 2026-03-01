using DC_bot.Interface;
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
    ILocalizationService localizationService)
{
    private AsyncEventHandler<DiscordClient, MessageReactionAddEventArgs>? _messageReactionAdded;
    private AsyncEventHandler<DiscordClient, MessageReactionRemoveEventArgs>? _messageReactionRemoved;
    private Func<IDiscordChannel, DiscordClient, string, Task>? _sendReactionControlMessage;
    private bool _isRegistered = false;

    public void RegisterHandler(DiscordClient client)
    {
        if (_isRegistered)
        {
            logger.LogInformation("ReactionHandler Service is already registered");
            return;
        }

        _messageReactionAdded = OnReactionAdded;
        _messageReactionRemoved = OnReactionRemoved;
        _sendReactionControlMessage = SendReactionControlMessage;
        
        lavaLinkService.TrackStarted += _sendReactionControlMessage;
        client.MessageReactionAdded += _messageReactionAdded;
        client.MessageReactionRemoved += _messageReactionRemoved;
        _isRegistered = true;
        logger.LogInformation("Registered reaction handler.");
    }

    private async Task SendReactionControlMessage(IDiscordChannel textChannel, DiscordClient client, string msg)
    {
        var message = await textChannel.ToDiscordChannel().SendMessageAsync(
            $"{msg}\n 🎵 **{localizationService.Get("music_control")}** 🎵\n" +
            $"⏸️ - {localizationService.Get("pause")} " +
            $"▶️ - {localizationService.Get("resume")} " +
            $"⏭️ - {localizationService.Get("skip")} " +
            $"🔁 - {localizationService.Get("repeat")}");

        // Reakciók hozzáadása az üzenethez
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":pause_button:"));
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":arrow_forward:"));
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":track_next:"));
        await message.CreateReactionAsync(DiscordEmoji.FromName(client, ":repeat:"));

        logger.LogInformation("Reaction control message sent and reactions added.");
    }

    private async Task OnReactionAdded(DiscordClient sender, MessageReactionAddEventArgs args)
    {
        if (args.User.IsBot) return;

        var guildId = args.Guild.Id;

        logger.LogInformation("Reaction added: {Emoji} by {Username}", args.Emoji.GetDiscordName(), args.User.Username);
        
        var discordAuthor = new DiscordUserWrapper(args.User);
        var discordChannel = new DiscordChannelWrapper(args.Channel);
        var member = await discordChannel.Guild.GetMemberAsync(discordAuthor.Id).ConfigureAwait(false);
        var discordMessageWrapper = new DiscordMessageWrapper(args.Message.Id, args.Message.Content,
            discordChannel, discordAuthor, args.Message.CreationTimestamp,
            args.Message.Embeds.ToList(), args.Message.RespondAsync,
            args.Message.RespondAsync);
        
        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.PauseAsync(discordMessageWrapper, member);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.ResumeAsync(discordMessageWrapper, member);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(discordMessageWrapper, member);
                break;

            case "🔁": // Repeat emoji
                lavaLinkService.IsRepeating[guildId] = true;
                await args.Message.RespondAsync(localizationService.Get("reaction_handler_repeat_on"));
                break;
        }
    }

    private async Task OnReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
    {
        if (args.User.IsBot) return;

        var guildId = args.Guild.Id;

        logger.LogInformation("Reaction removed: {Emoji} by {Username}", args.Emoji.GetDiscordName(), args.User.Username);

        var discordAuthor = new DiscordUserWrapper(args.User);
        var discordChannel = new DiscordChannelWrapper(args.Channel);
        var member = await discordChannel.Guild.GetMemberAsync(discordAuthor.Id).ConfigureAwait(false);
        var discordMessageWrapper = new DiscordMessageWrapper(args.Message.Id, args.Message.Content,
            discordChannel, discordAuthor, args.Message.CreationTimestamp,
            args.Message.Embeds.ToList(), args.Message.RespondAsync,
            args.Message.RespondAsync);
        
        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.ResumeAsync(discordMessageWrapper, member);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.PauseAsync(discordMessageWrapper, member);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(discordMessageWrapper, member);
                break;

            case "🔁": // Repeat emoji
                lavaLinkService.IsRepeating[guildId] = false;
                await args.Message.RespondAsync(localizationService.Get("reaction_handler_repeat_off"));
                break;
        }
    }

    internal void UnRegisterHandler(DiscordClient client)
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
            logger.LogInformation("Unregistered reaction handler");
        }
        else
        {
            logger.LogWarning("Tried to unregister handlers, but it was not registered");
        }
    }
}