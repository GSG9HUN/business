using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Records voice changes while tolerating missing event entities.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class DiscordVoiceEventLogger(ILogger<DiscordClientEventHandler> logger)
{

    /// <summary>
    /// Records voice state updated diagnostics using available Discord event state.
    /// </summary>
    /// <param name="sender">The sender supplied for this operation.</param>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        if (sender.CurrentUser?.Id is not { } botUserId || botUserId != e.UserId)
        {
            return;
        }

        var beforeChannelId = e.Before?.ChannelId;
        var afterChannelId = e.After?.ChannelId ?? e.ChannelId;
        logger.LogInformation("Bot voice state changed. Guild: {GuildId}, User: {UserId}, BeforeChannel: {BeforeChannelId}, AfterChannel: {AfterChannelId}, SessionId: {SessionId}", e.GuildId, e.UserId, beforeChannelId, afterChannelId, e.SessionId);

        if (beforeChannelId is not null && afterChannelId is null)
        {
            logger.LogInformation("Bot left voice channel. GuildId: {GuildId}, BeforeChannelId: {BeforeChannelId}", e.GuildId, beforeChannelId);
        }
    }

    /// <summary>
    /// Records voice server updated diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogVoiceServerUpdated(VoiceServerUpdatedEventArgs e)
    {
        logger.LogInformation("Discord voice server updated. GuildId: {GuildId}, Endpoint: {Endpoint}, HasVoiceToken: {HasVoiceToken}", e.Guild.Id, e.Endpoint, !string.IsNullOrWhiteSpace(e.VoiceToken));
    }
}
