using DC_bot.Constants;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Persistence.Queue;
using DC_bot.Interface.Service.Presentation;
using DSharpPlus.Entities;

namespace DC_bot.Commands.TextCommands.Queue;
// Read-only scaffold: batch details and replay can be added without changing playback state here.

/// <summary>
/// Handles the history text command through validation, service, and localized response boundaries.
/// </summary>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="localizationService">The localization service dependency used by this boundary.</param>
/// <param name="responseBuilder">The response builder dependency used by this boundary.</param>
public sealed class HistoryCommand(
    IQueueRepository queueRepository,
    ILocalizationService localizationService,
    IResponseBuilder responseBuilder) : ICommand
{

    /// <inheritdoc />
    public string Name => "history";

    /// <inheritdoc />
    public string Description => localizationService.Get(LocalizationKeys.HistoryCommandDescription);

    /// <summary>
    /// Lists the guild&apos;s recent playback batches without requiring voice membership or changing playback state.
    /// </summary>
    /// <param name="message">The message supplying command context and the response channel.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ExecuteAsync(IDiscordMessage message)
    {
        var guildId = message.Channel.Guild?.Id ?? 0;

        if (guildId == 0)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.SlashCommandGuildOnly);
            return;
        }

        var batches = await queueRepository.GetRecentBatchesAsync(guildId);

        if (batches.Count == 0)
        {
            await responseBuilder.SendWarningAsync(message, LocalizationKeys.HistoryCommandEmpty);
            return;
        }

        var embed = new DiscordEmbedBuilder().WithTitle(localizationService.Get(guildId, LocalizationKeys.HistoryCommandTitle))
            .WithColor(DiscordColor.Azure);

        foreach (var batch in batches)
        {
            var status = localizationService.Get(guildId, batch.CompletedAtUtc is null ? LocalizationKeys.HistoryCommandActive : LocalizationKeys.HistoryCommandCompleted);
            embed.AddField($"#{batch.Id} · {status}", localizationService.Get(guildId, LocalizationKeys.HistoryCommandEntry, batch.TrackCount, batch.CreatedAtUtc.ToString("yyyy-MM-dd HH:mm 'UTC'")));
        }

        await message.RespondAsync(embed);
    }
}
