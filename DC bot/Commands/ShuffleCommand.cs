using System.Collections;
using DC_bot.Interface;
using DC_bot.Service;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ShuffleCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ShuffleCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "shuffle";
    public string Description => localizationService.Get("shuffle_command_description");

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.LogInformation("Shuffle command invoked.");
        var validationResult = await userValidation.ValidateUserAsync(message);
        
        if (validationResult.IsValid is false)
        {
            await responseBuilder.SendValidationErrorAsync(message, validationResult.ErrorKey);
            return;
        }

        var guildId = message.Channel.Guild.Id;
        var queue = musicQueueService.GetQueue(guildId);
        
        if (!queue.Any() || queue.Count < 2)
        {
            await responseBuilder.SendCommandErrorResponse(message, Name);
            return;
        }
        
        var shuffledQueue = ShuffleQueue(queue);

        musicQueueService.SetQueue(guildId, shuffledQueue);

        await responseBuilder.SendCommandResponseAsync(message, Name);
        logger.LogInformation("Shuffle command Executed.");
    }

    private Queue<ILavaLinkTrack> ShuffleQueue(Queue<ILavaLinkTrack> queue)
    {
        var trackList = queue.ToList();

        // Megkeverés (Fisher-Yates algoritmus)
        var random = new Random();
        for (var i = trackList.Count - 1; i > 0; i--)
        {
            var j = random.Next(i + 1);
            (trackList[i], trackList[j]) = (trackList[j], trackList[i]);
        }
        
        //Kizárjuk, hogy az eredeti lista maradhasson.
        return trackList.SequenceEqual(queue.ToList()) ? ShuffleQueue(queue) : new Queue<ILavaLinkTrack>(trackList);
    }
}