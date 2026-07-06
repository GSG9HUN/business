using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Logging;
using DSharpPlus;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

public class DiscordClientEventHandler(
    ILogger<DiscordClientEventHandler> logger,
    IGuildDataRepository guildDataRepository,
    ILocalizationService localizationService,
    ILavaLinkService lavaLinkService)
{
    private const int DiscordVoiceDisconnectedCloseCode = 4014;
    private const int MaxUnknownEventPayloadLength = 1_000;

    public Task OnSocketOpened(DiscordClient sender, SocketOpenedEventArgs e)
    {
        try
        {
            logger.LogInformation("Discord gateway socket opened.");
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnSocketOpened));
        }

        return Task.CompletedTask;
    }

    public Task OnSocketClosed(DiscordClient sender, SocketClosedEventArgs e)
    {
        try
        {
            if (e.CloseCode == DiscordVoiceDisconnectedCloseCode)
            {
                logger.LogCritical(
                    "Discord gateway socket closed with voice disconnect code. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}",
                    e.CloseCode,
                    e.CloseMessage);
            }
            else
            {
                logger.LogWarning(
                    "Discord gateway socket closed. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}",
                    e.CloseCode,
                    e.CloseMessage);
            }
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnSocketClosed));
        }

        return Task.CompletedTask;
    }

    public async Task OnClientReady(DiscordClient sender, SessionCreatedEventArgs? e)
    {
        try
        {
            if (e is not null)
            {
                logger.LogInformation(
                    "Discord gateway session created. ShardId: {ShardId}, GuildCount: {GuildCount}",
                    e.ShardId,
                    e.GuildIds.Count);
            }

            logger.DiscordClientReady();
            await lavaLinkService.ConnectAsync();
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnClientReady));
        }
    }

    public Task OnSessionResumed(DiscordClient sender, SessionResumedEventArgs e)
    {
        try
        {
            logger.LogWarning("Discord gateway session resumed. ShardId: {ShardId}", e.ShardId);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnSessionResumed));
        }

        return Task.CompletedTask;
    }

    public Task OnZombied(DiscordClient sender, ZombiedEventArgs e)
    {
        try
        {
            logger.LogCritical(
                "Discord gateway considered zombied. HeartbeatFailures: {HeartbeatFailures}, GuildDownloadCompleted: {GuildDownloadCompleted}",
                e.Failures,
                e.GuildDownloadCompleted);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnZombied));
        }

        return Task.CompletedTask;
    }

    public async Task OnGuildAvailable(DiscordClient sender, GuildAvailableEventArgs e)
    {
        try
        {
            logger.DiscordClientGuildAvailable(e.Guild.Name);

            await guildDataRepository.EnsureGuildExistsAsync(e.Guild.Id, CancellationToken.None);
            localizationService.LoadLanguage(e.Guild.Id);
            await lavaLinkService.Init(e.Guild.Id);
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnGuildAvailable));
        }
    }

    public Task OnVoiceStateUpdated(DiscordClient sender, VoiceStateUpdatedEventArgs e)
    {
        try
        {
            if (sender.CurrentUser.Id != e.UserId) return Task.CompletedTask;

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
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnVoiceStateUpdated));
        }
        
        return Task.CompletedTask;
    }

    public Task OnVoiceServerUpdated(DiscordClient sender, VoiceServerUpdatedEventArgs e)
    {
        try
        {
            logger.LogWarning(
                "Discord voice server updated. GuildId: {GuildId}, Endpoint: {Endpoint}, HasVoiceToken: {HasVoiceToken}",
                e.Guild.Id,
                e.Endpoint,
                !string.IsNullOrWhiteSpace(e.VoiceToken));
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnVoiceServerUpdated));
        }

        return Task.CompletedTask;
    }

    public Task OnUnknownEvent(DiscordClient sender, UnknownEventArgs e)
    {
        try
        {
            var payload = TruncatePayload(e.Json);

            if (e.EventName.StartsWith("VOICE_", StringComparison.Ordinal))
            {
                logger.LogWarning(
                    "Unknown Discord voice event received. EventName: {EventName}, Payload: {Payload}",
                    e.EventName,
                    payload);
            }
            else
            {
                logger.LogDebug(
                    "Unknown Discord event received. EventName: {EventName}, Payload: {Payload}",
                    e.EventName,
                    payload);
            }
        }
        catch (Exception exception)
        {
            logger.DiscordClientEventFailed(exception, nameof(OnUnknownEvent));
        }

        return Task.CompletedTask;
    }

    private static string TruncatePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        return payload.Length <= MaxUnknownEventPayloadLength
            ? payload
            : string.Concat(payload.AsSpan(0, MaxUnknownEventPayloadLength), "...");
    }
}
