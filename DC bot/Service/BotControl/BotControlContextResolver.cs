using DC_bot.BotControl;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

/// <summary>
/// Resolves guild, member, and channel permissions for durable mobile commands.
/// </summary>
/// <param name="discordClient">The discord client supplied for this operation.</param>
public class BotControlContextResolver(DiscordClient discordClient) : IBotControlContextResolver
{

    /// <summary>
    /// Resolves the command&apos;s guild, member, channels, and Discord notification permissions before mobile dispatch.
    /// </summary>
    /// <param name="command">The command supplied for this operation.</param>
    /// <param name="channelContext">The channel context supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<BotControlExecutionContext> ResolveAsync(
        BotControlCommandRecord command,
        BotControlCommandChannelContext? channelContext,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, command.GuildId, cancellationToken);

        if (guild is null)
        {
            return new BotControlExecutionContext(command.GuildId, command.UserId, VoiceChannelId: null, TextChannelId: null, CanSendDiscordResponse: false);
        }

        var voiceChannelId = await ResolveVoiceChannelIdAsync(guild, command.UserId, channelContext?.VoiceChannelId, cancellationToken);
        var textChannelId = await ResolveTextChannelIdAsync(guild, channelContext?.TextChannelId, voiceChannelId, cancellationToken);
        return new BotControlExecutionContext(command.GuildId, command.UserId, voiceChannelId, textChannelId, textChannelId is not null);
    }

    /// <summary>
    /// Resolves the requested or current member voice channel for a mobile command.
    /// </summary>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="commandUserId">The command user id supplied for this operation.</param>
    /// <param name="channelContextVoiceChannelId">The channel context voice channel id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<ulong?> ResolveVoiceChannelIdAsync(
        DiscordGuild guild,
        ulong commandUserId,
        ulong? channelContextVoiceChannelId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var member = await TryGetMemberAsync(guild, commandUserId, cancellationToken);
        var voiceChannel = member?.VoiceState is null ? null : await member.VoiceState.GetChannelAsync();

        if (channelContextVoiceChannelId is not null
            && voiceChannel?.Id != channelContextVoiceChannelId)
        {
            return null;
        }

        return BotControlDiscordChannelResolver.IsVoiceCapableChannel(voiceChannel) ? voiceChannel?.Id : null;
    }

    /// <summary>
    /// Resolves an available channel for optional mobile-command Discord responses.
    /// </summary>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="channelContextTextChannelId">The channel context text channel id supplied for this operation.</param>
    /// <param name="voiceChannelId">The voice channel id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private async Task<ulong?> ResolveTextChannelIdAsync(
        DiscordGuild guild,
        ulong? channelContextTextChannelId,
        ulong? voiceChannelId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (channelContextTextChannelId is not null)
        {
            var requestedChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, channelContextTextChannelId.Value, cancellationToken);

            if (
                BotControlDiscordChannelResolver.IsMessageCapableChannel(
                requestedChannel))
            {
                return requestedChannel!.Id;
            }
        }

        if (voiceChannelId is not null)
        {
            var voiceChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, voiceChannelId.Value, cancellationToken);

            if (BotControlDiscordChannelResolver.IsMessageCapableChannel(voiceChannel))
            {
                return voiceChannel!.Id;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to resolve the requesting member in the selected guild.
    /// </summary>
    /// <param name="guild">The guild supplied for this operation.</param>
    /// <param name="userId">The user id supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    private static async Task<DiscordMember?> TryGetMemberAsync(
        DiscordGuild guild,
        ulong userId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await guild.GetMemberAsync(userId)
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
}
