using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>
/// Handles the shuffle text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class ShuffleCommand(
    IUserValidationService userValidation,
    IMusicQueueService musicQueueService,
    ILogger<ShuffleCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "shuffle";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.ShuffleCommandDescription);

    /// <summary>
    /// Validates the message context, performs this command&apos;s action, and sends its localized outcome.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message, default);

        if (validationResult is null)
        {
            return;
        }

        var guildId = message.Channel.Guild.Id;
        var queue = await musicQueueService.GetQueue(guildId, default);

        if (queue.Count is 0 or < 2)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.ShuffleCommandNotEnoughTracks);
            return;
        }

        var shuffledQueue = ShuffleQueue(queue);
        await musicQueueService.SetQueue(guildId, shuffledQueue, default);
        await responseBuilder.SendSuccessAsync(message, LocalizationKeys.ShuffleCommandResponse);
        logger.CommandExecuted(Name);
    }

    /// <summary>
    /// Shuffles waiting tracks and persists their order while retaining queue-item identities.
    /// </summary>
    /// <param name="queue">The queue supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
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
            {
                break;
            }

            attempts++;
        }

        return new Queue<ILavaLinkTrack>(trackList);
    }
}
