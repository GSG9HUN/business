using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Finds usable voice and response channels for mobile commands.
/// </summary>
internal static class BotControlDiscordChannelResolver
{

    /// <summary>
    /// Attempts to resolve an available guild for the operation.
    /// </summary>
    /// <param name="discordClient">The discord client supplied for this operation.</param>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal static async Task<DiscordGuild?> TryGetGuildAsync(
        DiscordClient discordClient,
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await discordClient.GetGuildAsync(guildId)
                .WaitAsync(cancellationToken);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Attempts to resolve a usable channel belonging to the selected guild.
    /// </summary>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="channelId">The channel id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal static async Task<DiscordChannel?> TryGetGuildChannelAsync(
        DiscordGuild guild,
        ulong channelId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            var channel = await guild.GetChannelAsync(channelId)
                .WaitAsync(cancellationToken);
            return channel.GuildId == guild.Id ? channel : null;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch
        {
            return null;
        }
    }

    /// <summary>
    /// Checks whether the channel type supports the required voice operation.
    /// </summary>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    internal static bool IsVoiceCapableChannel(DiscordChannel? channel)
    {
        return channel?.Type is DiscordChannelType.Voice;
    }

    /// <summary>
    /// Checks whether the channel type can receive a command response.
    /// </summary>
    /// <param name="channel">The channel supplied for this operation.</param>
    /// <returns>Whether the requested check or operation succeeded.</returns>
    internal static bool IsMessageCapableChannel(DiscordChannel? channel)
    {
        return channel?.Type is DiscordChannelType.Text or DiscordChannelType.News or DiscordChannelType.Voice;
    }
}
