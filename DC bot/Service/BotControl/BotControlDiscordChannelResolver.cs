using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Finds usable voice and response channels for mobile commands.
/// </summary>
internal static class BotControlDiscordChannelResolver
{

    /// <summary>Chooses the first usable remembered, explicitly requested, or user voice response channel.</summary>
    /// <param name="rememberedChannelId">The music response channel retained by the guild playback session.</param>
    /// <param name="requestedChannelId">The optional response channel supplied with the mobile command.</param>
    /// <param name="voiceChannelId">The validated voice channel of the requesting user.</param>
    /// <param name="isUsableAsync">Checks whether a candidate belongs to the guild and supports messages.</param>
    /// <param name="cancellationToken">Cancels channel lookup before considering further candidates.</param>
    /// <returns>The first usable response channel, or null when none is available.</returns>
    internal static async Task<ulong?> SelectResponseChannelAsync(
        ulong? rememberedChannelId,
        ulong? requestedChannelId,
        ulong? voiceChannelId,
        Func<ulong, CancellationToken, Task<bool>> isUsableAsync,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var candidates = new[] { rememberedChannelId, requestedChannelId, voiceChannelId }.OfType<ulong>()
            .Distinct();

        foreach (var channelId in candidates)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (await isUsableAsync(channelId, cancellationToken))
            {
                return channelId;
            }
        }

        return null;
    }

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
