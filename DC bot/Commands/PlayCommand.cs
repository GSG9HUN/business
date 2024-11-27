using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class PlayCommand(LavaLinkService _lavaLinkService, ILogger<PlayCommand> _logger) : ICommand
{
    public string Name => "play";
    public string Description => "Start playing a music";

    public async Task ExecuteAsync(DiscordMessage message)
    {
        var user = message.Author;
        var member = message.Channel.Guild.GetMemberAsync(user.Id).Result;

        if (member?.VoiceState?.Channel == null)
        {
            await message.RespondAsync("You must be in a voice channel");
            _logger.LogInformation("The user is not in a voice channel");
        }

        string[] args = message.Content.Split(" ", 2);
        if (args.Length < 2)
        {
            await message.Channel.SendMessageAsync("Please provide URL.");
            _logger.LogInformation("The user not provided URL");
            return;
        }

        var textChannel = message.Channel;
        var query = args[1].Trim();
        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            await _lavaLinkService.PlayAsync(member!.VoiceState!.Channel, url, textChannel);
        }
        else
        {
            await _lavaLinkService.PlayAsync(member!.VoiceState!.Channel, query, textChannel);
        }

        _logger.LogInformation("Play command executed!");
    }
}