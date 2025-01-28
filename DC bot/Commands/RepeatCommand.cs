using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class RepeatCommand(LavaLinkService _lavaLinkService, ILogger<RepeatCommand> _logger) : ICommand
{
    public string Name => "repeat";
    public string Description => "Repeats a specified track infinitely.";

    public async Task ExecuteAsync(DiscordMessage message)
    {
        _logger.LogInformation("Repeat command invoked");
        if (!_lavaLinkService.IsRepeating)
        {
            _lavaLinkService.IsRepeating = true;
            await message.Channel.SendMessageAsync($"Repeat is on for : {_lavaLinkService.GetCurrentTrack()}");
        }
        else
        {
            _lavaLinkService.IsRepeating = false;
            await message.Channel.SendMessageAsync("Repeating is off.");
        }

        _logger.LogInformation("Repeat command executed");
    }
}