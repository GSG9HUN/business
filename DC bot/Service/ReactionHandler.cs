using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service;

public class ReactionHandler(LavaLinkService lavaLinkService, ILogger<ReactionHandler> logger)
{
    public void RegisterHandler(DiscordClient client)
    {
        lavaLinkService.TrackStarted += SendReactionControlMessage;
        client.MessageReactionAdded += OnReactionAdded;
        client.MessageReactionRemoved += OnReactionRemoved;
        logger.LogInformation("Registered reaction handler");
    }

    private async Task SendReactionControlMessage(DiscordChannel textChannel, DiscordClient client, string msg)
    {
        var message = await textChannel.SendMessageAsync($"{msg}\n 🎵 **Music Controls** 🎵\n" +
                                                         "⏸️ - Pause " +
                                                         "▶️ - Resume " +
                                                         "⏭️ - Skip " +
                                                         "🔁 - Repeat");

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
       
        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.PauseAsync(args.Channel);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.ResumeAsync(args.Channel);
                break;
            
            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(args.Channel);
                break;

            case "🔁": // Repeat emoji
                lavaLinkService.IsRepeating[guildId] = true;
                await args.Message.RespondAsync($"Repeat mode: Enabled");
                break;
        }
    }

    private async Task OnReactionRemoved(DiscordClient sender, MessageReactionRemoveEventArgs args)
    {
        if (args.User.IsBot) return;

        var guildId = args.Guild.Id;
        
        logger.LogInformation($"Reaction removed: {args.Emoji.GetDiscordName()} by {args.User.Username}");
        
        switch (args.Emoji.Name)
        {
            case "⏸️": // Pause emoji
                await lavaLinkService.ResumeAsync(args.Channel);
                break;

            case "▶️": // Resume emoji
                await lavaLinkService.PauseAsync(args.Channel);
                break;

            case "⏭️": // Skip emoji
                await lavaLinkService.SkipAsync(args.Channel);
                break;

            case "🔁": // Repeat emoji
                lavaLinkService.IsRepeating[guildId] = false;
                await args.Message.RespondAsync($"Repeat mode: Disabled");
                break;
        }
    }
}