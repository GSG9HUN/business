using DC_bot.BotControl;
using DC_bot.Interface.Service.BotControl;
using DC_bot.Interface.Service.BotControl.Models;
using DC_bot.Interface.Service.Persistence.Models.BotControlCommand;
using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

public class BotControlContextResolver(
    DiscordClient discordClient
) : IBotControlContextResolver
{
    public async Task<BotControlExecutionContext> ResolveAsync(
        BotControlCommandRecord command,
        BotControlCommandChannelContext? channelContext,
        CancellationToken cancellationToken)
    {
        var guild = await BotControlDiscordChannelResolver.TryGetGuildAsync(discordClient, command.GuildId);

        if (guild is null)
        {
            return new BotControlExecutionContext(
                command.GuildId,
                command.UserId,
                VoiceChannelId: null,
                TextChannelId: null,
                CanSendDiscordResponse: false);
        }

        var voiceChannelId = await ResolveVoiceChannelIdAsync(
            guild,
            command.UserId,
            channelContext?.VoiceChannelId);

        var textChannelId = await ResolveTextChannelIdAsync(
            guild,
            channelContext?.TextChannelId,
            voiceChannelId);

        return new BotControlExecutionContext(
            command.GuildId,
            command.UserId,
            voiceChannelId,
            textChannelId,
            textChannelId is not null);
    }

    private async Task<ulong?> ResolveVoiceChannelIdAsync(
        DiscordGuild guild,
        ulong commandUserId,
        ulong? channelContextVoiceChannelId)
    {
        if (channelContextVoiceChannelId is not null)
        {
            var requestedChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, channelContextVoiceChannelId.Value);

            return BotControlDiscordChannelResolver.IsVoiceCapableChannel(requestedChannel) ? requestedChannel?.Id : null;
        }

        var member = await TryGetMemberAsync(guild, commandUserId);
        var voiceChannel = member?.VoiceState is null
            ? null
            : await member.VoiceState.GetChannelAsync();

        return BotControlDiscordChannelResolver.IsVoiceCapableChannel(voiceChannel) ? voiceChannel?.Id : null;
    }

    private async Task<ulong?> ResolveTextChannelIdAsync(
        DiscordGuild guild,
        ulong? channelContextTextChannelId,
        ulong? voiceChannelId)
    {
        if (channelContextTextChannelId is not null)
        {
            var requestedChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, channelContextTextChannelId.Value);

            if (BotControlDiscordChannelResolver.IsMessageCapableChannel(requestedChannel))
            {
                return requestedChannel!.Id;
            }
        }

        if (voiceChannelId is not null)
        {
            var voiceChannel = await BotControlDiscordChannelResolver.TryGetGuildChannelAsync(guild, voiceChannelId.Value);

            if (BotControlDiscordChannelResolver.IsMessageCapableChannel(voiceChannel))
            {
                return voiceChannel!.Id;
            }
        }

        return null;
    }

    private static async Task<DiscordMember?> TryGetMemberAsync(DiscordGuild guild, ulong userId)
    {
        try
        {
            return await guild.GetMemberAsync(userId);
        }
        catch
        {
            return null;
        }
    }

}
