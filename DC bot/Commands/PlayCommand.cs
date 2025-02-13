using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PlayCommand(LavaLinkService lavaLinkService, ILogger<PlayCommand> logger) : ICommand
{
    public string Name => "play";
    public string Description => "Start playing a music";

    public async Task ExecuteAsync(IDiscordMessageWrapper message)
    {
        var user = message.Author;
        var member = message.Channel.Guild.GetMemberAsync(user.Id).Result;

        if (member.VoiceState?.Channel == null)
        {
            await message.RespondAsync("You must be in a voice channel");
            logger.LogInformation("The user is not in a voice channel");
            return;
        }

        var args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await message.RespondAsync("Please provide URL.");
            logger.LogInformation("The user not provided URL");
            return;
        }

        var textChannel = message.Channel;
        var query = args[1].Trim();
        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            await lavaLinkService.PlayAsyncUrl(member.VoiceState!.Channel, url, textChannel);
        }
        else
        {
            await lavaLinkService.PlayAsyncQuery(member.VoiceState!.Channel, query, textChannel);
        }

        logger.LogInformation("Play command executed!");
    }
}