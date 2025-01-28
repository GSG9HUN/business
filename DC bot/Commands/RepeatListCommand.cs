using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatListCommand(LavaLinkService _lavaLinkService, ILogger<RepeatListCommand> _logger) : ICommand
{
    public string Name => "repeatList";
    public string Description => "Repeats the current track list.";

    public async Task ExecuteAsync(DiscordMessage message)
    {
        _logger.LogInformation("Repeat list command invoked");
        if (!_lavaLinkService.IsRepeatingList)
        {
            _lavaLinkService.IsRepeatingList = true;
            await message.Channel.SendMessageAsync($"Repeat is on for current list:\n {_lavaLinkService.GetCurrentTrackList()}");
            _lavaLinkService.CloneQueue();
        }
        else
        {
            _lavaLinkService.IsRepeatingList = false;
            await message.Channel.SendMessageAsync("Repeating is off.");
        }

        _logger.LogInformation("Repeat list command executed");
    }
}