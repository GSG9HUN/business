using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.Queue;

public class ShuffleCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ShuffleCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "shuffle";
    public string Description => localizationService.Get(LocalizationKeys.ShuffleCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;
        var queue = musicQueueService.GetQueue(guildId);

        if (queue.Count is 0 or < 2)
        {
            await responseBuilder.SendCommandErrorResponse(message, Name);
            return;
        }

        var shuffledQueue = ShuffleQueue(queue);

        musicQueueService.SetQueue(guildId, shuffledQueue);

        await responseBuilder.SendCommandResponseAsync(message, Name);
        logger.CommandExecuted(Name);
    }

    private Queue<ILavaLinkTrack> ShuffleQueue(Queue<ILavaLinkTrack> queue)
    {
        var trackList = queue.ToList();

        // Megkeverés (Fisher-Yates algoritmus)
        var random = new Random();
        const int maxAttempts = 10;
        var attempts = 0;
        while (attempts < maxAttempts)
        {
            for (var i = trackList.Count - 1; i > 0; i--)
            {
                var j = random.Next(i + 1);
                (trackList[i], trackList[j]) = (trackList[j], trackList[i]);
            }

            if (!trackList.SequenceEqual(queue.ToList()))
                break;

            attempts++;
        }

        return new Queue<ILavaLinkTrack>(trackList);
    }
}