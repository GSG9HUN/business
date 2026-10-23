using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Publishes mobile-command outcomes to the selected Discord channel when requested.
/// </summary>
/// <param name="discordClient">The discord client supplied for this operation.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
public class BotControlDiscordResponseService(
    DiscordClient discordClient,
    ILogger<BotControlDiscordResponseService> logger) : IBotControlDiscordResponseService
{

    /// <summary>
    /// Attempts Discord response delivery and reports whether delivery succeeded.
    /// </summary>
    /// <param name="result">The result supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> TrySendAsync(BotControlCommandResult result, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (!result.ShouldNotifyDiscord)
        {
            return false;
        }

        if (result.TextChannelId is null)
        {
            return false;
        }

        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, result.GuildId, cancellationToken);

        if (guild is null)
        {
            return false;
        }

        var channel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, result.TextChannelId.Value, cancellationToken);

        if (channel is null || channel.GuildId != guild.Id)
        {
            return false;
        }

        if (!BotControlDiscordChannelResolver.IsMessageCapableChannel(channel))
        {
            return false;
        }

        try
        {
            await channel.SendMessageAsync(result.Message)
                .WaitAsync(cancellationToken);
            return true;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Failed to send bot control Discord response. GuildId: {GuildId}, TextChannelId: {TextChannelId}", result.GuildId, result.TextChannelId);
            return false;
        }
    }
}
