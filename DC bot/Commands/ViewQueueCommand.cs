using System.Linq;
using System.Threading.Tasks;
using DC_bot.Interface;
using DC_bot.Services;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands
{
    public class ViewQueueCommand(LavaLinkService _lavaLinkService, ILogger<ViewQueueCommand> _logger) : ICommand
    {
        public string Name => "viewList";
        public string Description => "view the list of tracks.";

        public async Task ExecuteAsync(DiscordMessage message)
        {
            var queue = _lavaLinkService.ViewQueue();

            if (!queue.Any())
            {
                await message.Channel.SendMessageAsync("The queue is currently empty.");
                _logger.LogInformation("Queue is empty.");
                return;
            }

            var queueList = string.Join("\n",
                queue.Select((track, index) => $"{index + 1}. {track.Title} ({track.Author})"));
            await message.Channel.SendMessageAsync($"Current Queue:\n{queueList}");
            _logger.LogInformation("View Queue command executed.");
        }
    }
}

