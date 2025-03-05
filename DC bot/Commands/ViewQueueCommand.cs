using DC_bot.Interface;
using DSharpPlus.Entities;
using Microsoft.Extensions.Logging;

namespace DC_bot.Commands;

public class ViewQueueCommand(
    ILavaLinkService lavaLinkService,
    IUserValidationService userValidation,
    ILogger<ViewQueueCommand> logger,
    ILocalizationService localizationService) : ICommand
{
    public string Name => "viewList";
    public string Description => localizationService.Get("view_list_command_description");

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
            await message.RespondAsync(localizationService.Get("view_list_command_error"));
            logger.LogInformation("Queue is empty.");
            return;
        }

        var embed = new DiscordEmbedBuilder()
            .WithTitle(localizationService.Get("view_list_command_embed_title"))
            .WithColor(DiscordColor.Azure);

        foreach (var track in queue.Take(10))
        {
            embed.AddField(track.Title, $"🎵 {track.Author}");
        }

        if (queue.Count > 10)
        {
            embed.WithFooter($"{localizationService.Get("view_list_command_response", queue.Count - 10)}");
        }

        await message.RespondAsync(embed);

        logger.LogInformation("View Queue command executed.");
    }
}