using DC_bot.Interface;
using DC_bot.Wrapper;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ReactionHandler(
    ILavaLinkService lavaLinkService,
    ILogger<ReactionHandler> logger,
    ILocalizationService localizationService)
{
    public void RegisterHandler(DiscordClient client)
    {
        lavaLinkService.TrackStarted += SendReactionControlMessage;
        client.MessageReactionAdded += OnReactionAdded;
        client.MessageReactionRemoved += OnReactionRemoved;
        logger.LogInformation("Registered reaction handler");
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

        logger.LogInformation($"Reaction added: {args.Emoji.GetDiscordName()} by {args.User.Username}");
        var discordChannelWrapper = new DiscordChannelWrapper(args.Channel);
        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.PauseAsync(discordChannelWrapper);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.ResumeAsync(discordChannelWrapper);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(discordChannelWrapper);
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

        logger.LogInformation($"Reaction removed: {args.Emoji.GetDiscordName()} by {args.User.Username}");

        var discordChannelWrapper = new DiscordChannelWrapper(args.Channel);

        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.ResumeAsync(discordChannelWrapper);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.PauseAsync(discordChannelWrapper);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(discordChannelWrapper);
                break;

            case "🔁": // Repeat emoji
                lavaLinkService.IsRepeating[guildId] = false;
                await args.Message.RespondAsync(localizationService.Get("reaction_handler_repeat_off"));
                break;
        }
    }
}