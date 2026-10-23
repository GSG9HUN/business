using DC_bot.Logging;
using DSharpPlus.EventArgs;
using Microsoft.Extensions.Logging;

namespace DC_bot.Wrapper.Discord;

/// <summary>
/// Records gateway and socket diagnostics with bounded unknown-event payloads.
/// </summary>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class DiscordConnectionEventLogger(ILogger<DiscordClientEventHandler> logger)
{
    private const int DiscordDisallowedIntentsCloseCode = 4014;
    private const int MaxUnknownEventPayloadLength = 1_000;

    /// <summary>
    /// Records socket opened diagnostics using available Discord event state.
    /// </summary>
    internal void LogSocketOpened()
    {
        logger.LogInformation("Discord gateway socket opened.");
    }

    /// <summary>
    /// Records socket closed diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogSocketClosed(SocketClosedEventArgs e)
    {
        if (e.CloseCode == DiscordDisallowedIntentsCloseCode)
        {
            logger.LogCritical("Discord gateway rejected disallowed intents. Check enabled privileged intents in the Developer Portal. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}", e.CloseCode, e.CloseMessage);
            return;
        }

        logger.LogWarning("Discord gateway socket closed. CloseCode: {CloseCode}, CloseMessage: {CloseMessage}", e.CloseCode, e.CloseMessage);
    }

    /// <summary>
    /// Records client ready diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The e supplied for this operation.</param>
    internal void LogClientReady(SessionCreatedEventArgs? e)
    {
        if (e is not null)
        {
            logger.LogInformation("Discord gateway session created. ShardId: {ShardId}, GuildCount: {GuildCount}", e.ShardId, e.GuildIds.Count);
        }

        logger.DiscordClientReady();
    }

    /// <summary>
    /// Records session resumed diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogSessionResumed(SessionResumedEventArgs e)
    {
        logger.LogWarning("Discord gateway session resumed. ShardId: {ShardId}", e.ShardId);
    }

    /// <summary>
    /// Records zombied diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogZombied(ZombiedEventArgs e)
    {
        logger.LogCritical("Discord gateway considered zombied. HeartbeatFailures: {HeartbeatFailures}, GuildDownloadCompleted: {GuildDownloadCompleted}", e.Failures, e.GuildDownloadCompleted);
    }

    /// <summary>
    /// Records unknown event diagnostics using available Discord event state.
    /// </summary>
    /// <param name="e">The event payload supplied by the Discord or Lavalink SDK.</param>
    internal void LogUnknownEvent(UnknownEventArgs e)
    {
        var payload = TruncatePayload(e.Json);

        if (e.EventName.StartsWith("VOICE_", StringComparison.Ordinal))
        {
            logger.LogWarning("Unknown Discord voice event received. EventName: {EventName}, Payload: {Payload}", e.EventName, payload);
            return;
        }

        logger.LogDebug("Unknown Discord event received. EventName: {EventName}, Payload: {Payload}", e.EventName, payload);
    }

    /// <summary>
    /// Limits diagnostic payload length before including it in a log entry.
    /// </summary>
    /// <param name="payload">The payload supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string TruncatePayload(string? payload)
    {
        if (string.IsNullOrWhiteSpace(payload))
        {
            return string.Empty;
        }

        return payload.Length <= MaxUnknownEventPayloadLength ? payload : string.Concat(payload.AsSpan(0, MaxUnknownEventPayloadLength), "...");
    }
}
