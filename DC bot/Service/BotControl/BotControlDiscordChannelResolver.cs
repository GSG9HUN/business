using DSharpPlus;
using DSharpPlus.Entities;

namespace DC_bot.Service.BotControl;

internal static class BotControlDiscordChannelResolver
{
    internal static async Task<DiscordGuild?> TryGetGuildAsync(
        DiscordClient discordClient,
        ulong guildId)
    {
        try
        {
            return await discordClient.GetGuildAsync(guildId);
        }
        catch
        {
            return null;
        }
    }

    internal static async Task<DiscordChannel?> TryGetGuildChannelAsync(
        DiscordGuild guild,
        ulong channelId)
    {
        try
        {
            var channel = await guild.GetChannelAsync(channelId);
            return channel.GuildId == guild.Id ? channel : null;
        }
        catch
        {
            return null;
        }
    }

    internal static bool IsVoiceCapableChannel(DiscordChannel? channel)
    {
        return channel?.Type is DiscordChannelType.Voice;
    }

    internal static bool IsMessageCapableChannel(DiscordChannel? channel)
    {
        return channel?.Type is
            DiscordChannelType.Text or
            DiscordChannelType.News or
            DiscordChannelType.Voice;
    }
}
