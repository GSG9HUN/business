using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

internal sealed class DiscordVoiceEventLogger(ILogger<DiscordClientEventHandler> logger)
{
    internal void LogVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        if (sender.CurrentUser.Id != e.UserId)
        {
            return;
        }

        var beforeChannelId = e.Before?.ChannelId;
        var afterChannelId = e.After?.ChannelId ?? e.ChannelId;

        logger.LogCritical(
            "Bot voice state changed. Guild: {GuildId}, User: {UserId}, BeforeChannel: {BeforeChannelId}, AfterChannel: {AfterChannelId}, SessionId: {SessionId}",
            e.GuildId,
            e.UserId,
            beforeChannelId,
            afterChannelId,
            e.SessionId);

        if (beforeChannelId is not null && afterChannelId is null)
        {
            logger.LogCritical(
                "BOT VOICE DISCONNECT DETECTED. GuildId: {GuildId}, BeforeChannelId: {BeforeChannelId}",
                e.GuildId,
                beforeChannelId);
        }
    }

    internal void LogVoiceServerUpdated(VoiceServerUpdatedEventArgs e)
    {
        logger.LogWarning(
            "Discord voice server updated. GuildId: {GuildId}, Endpoint: {Endpoint}, HasVoiceToken: {HasVoiceToken}",
            e.Guild.Id,
            e.Endpoint,
            !string.IsNullOrWhiteSpace(e.VoiceToken));
    }
}
