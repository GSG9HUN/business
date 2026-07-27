using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Logging;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands.TextCommands.Queue;

public class ViewQueueCommand(
    IMusicQueueService musicQueueService,
    IUserValidationService userValidation,
    ILogger<ViewQueueCommand> logger,
    IResponseBuilder responseBuilder,
    ILocalizationService localizationService,
    ICommandHelper commandHelper) : ICommand
{
    public string Name => "viewList";
    public string Description => localizationService.Get(LocalizationKeys.ViewListCommandDescription);

    public async Task ExecuteAsync(IDiscordMessage message)
    {
        logger.CommandInvoked(Name);
        var validationResult = await commandHelper.TryValidateUserAsync(userValidation, responseBuilder, message);
        if (validationResult is null) return;

        var guildId = message.Channel.Guild.Id;
        var queue = await musicQueueService.ViewQueue(guildId);

        if (queue.Count == 0)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.ViewListCommandError);
            logger.QueueIsEmpty();
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle(localizationService.Get(guildId, LocalizationKeys.ViewListCommandEmbedTitle))
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
