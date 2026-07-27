using DC_bot.Logging;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper;

internal sealed class DiscordConnectionEventLogger(ILogger<DiscordClientEventHandler> logger)
{
    private const int DiscordVoiceDisconnectedCloseCode = 4014;
    private const int MaxUnknownEventPayloadLength = 1_000;

    internal void LogSocketOpened()
    {
        logger.LogInformation("Discord gateway socket opened.");
    }

    internal void LogSocketClosed(SocketClosedEventArgs e)
    {
        if (e.CloseCode == DiscordVoiceDisconnectedCloseCode)
        {
            logger.LogCritical(
                "Discord gateway socket closed with voice disconnect code. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}",
                e.CloseCode,
                e.CloseMessage);
            return;
        }

        logger.LogWarning(
            "Discord gateway socket closed. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}",
            e.CloseCode,
            e.CloseMessage);
    }

    internal void LogClientReady(SessionCreatedEventArgs? e)
    {
        if (e is not null)
        {
            logger.LogInformation(
                "Discord gateway session created. ShardId: {ShardId}, GuildCount: {GuildCount}",
                e.ShardId,
                e.GuildIds.Count);
        }

        logger.DiscordClientReady();
    }

    internal void LogSessionResumed(SessionResumedEventArgs e)
    {
        logger.LogWarning("Discord gateway session resumed. ShardId: {ShardId}", e.ShardId);
    }

    internal void LogZombied(ZombiedEventArgs e)
    {
        logger.LogCritical(
            "Discord gateway considered zombied. HeartbeatFailures: {HeartbeatFailures}, GuildDownloadCompleted: {GuildDownloadCompleted}",
            e.Failures,
            e.GuildDownloadCompleted);
    }

    internal void LogUnknownEvent(UnknownEventArgs e)
    {
        var payload = TruncatePayload(e.Json);

        if (e.EventName.StartsWith("VOICE_", StringComparison.Ordinal))
        {
            logger.LogWarning(
                "Unknown Discord voice event received. EventName: {EventName}, Payload: {Payload}",
                e.EventName,
                payload);
            return;
        }

        logger.LogDebug(
            "Unknown Discord event received. EventName: {EventName}, Payload: {Payload}",
            e.EventName,
            payload);
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
