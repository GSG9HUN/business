using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

/// <summary>
/// Handles the view queue text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="musicQueueService">The music queue service dependency used by this boundary.</param>
/// <param name="userValidation">The user validation service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="commandHelper">The command helper dependency used by this boundary.</param>
public class ViewQueueCommand(
    IMusicQueueService musicQueueService,
    IUserValidationService userValidation,
    ILogger<ViewQueueCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{

    /// <inheritdoc />
    public string Name => "viewList";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.ViewListCommandDescription);

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
        var queue = await musicQueueService.ViewQueue(guildId, default);

        if (queue.Count == 0)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewListCommandError);
            logger.QueueIsEmpty();
            return;
        }

        var embed = new DiscordEmbedBuilder().WithTitle(localizationService.Get(guildId, LocalizationKeys.ViewListCommandEmbedTitle))
            .WithColor(DiscordColor.Azure);

        foreach (var track in queue.Take(10))
        {
            embed.AddField(track.Title, $"🎵 {track.Author}");
        }

        if (queue.Count > 10)
        {
            embed.WithFooter($"{localizationService.Get(guildId, LocalizationKeys.ViewListCommandResponse, queue.Count - 10)}");
        }

        await message.RespondAsync(embed);
        logger.CommandExecuted(Name);
    }
}
