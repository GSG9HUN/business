using DC_bot.Interface;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ViewQueueCommand(ILavaLinkService lavaLinkService, IUserValidationService userValidation, ILogger<ViewQueueCommand> logger) : ICommand
{
    public string Name => "viewList";
    public string Description => "view the list of tracks.";

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            return;
        }
        
        var queue = lavaLinkService.ViewQueue(message.Channel.Guild.Id);

        if (queue.Count == 0)
        {
            await message.RespondAsync("The queue is currently empty.");
            logger.LogInformation("Queue is empty.");
            return;
        }

        var queueList = string.Join("\n",
            queue.Select((track, index) => $"{index + 1}. {track.Title} ({track.Author})"));

        await message.RespondAsync($"Current Queue:\n{queueList}");

        logger.LogInformation("View Queue command executed.");
    }
}