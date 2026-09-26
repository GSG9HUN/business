using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DSharpPlus;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.BotControl;

public class BotControlDiscordResponseService(
    DiscordClient discordClient,
    ILogger<BotControlDiscordResponseService> logger
) : IBotControlDiscordResponseService
{
    public async Task<bool> TrySendAsync(BotControlCommandResult result, CancellationToken cancellationToken)
    {
        if (!result.ShouldNotifyDiscord)
        {
            return false;
        }

        if (result.TextChannelId is null)
        {
            return false;
        }

        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, result.GuildId);

        if (guild is null)
        {
            return false;
        }

        var channel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, result.TextChannelId.Value);

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
            await channel.SendMessageAsync(result.Message);
            return true;
        }
        catch (Exception ex)
        {
            logger.LogWarning(
                ex,
                "Failed to send bot control Discord response. GuildId: {GuildId}, TextChannelId: {TextChannelId}",
                result.GuildId,
                result.TextChannelId);
            return false;
        }
    }
}
