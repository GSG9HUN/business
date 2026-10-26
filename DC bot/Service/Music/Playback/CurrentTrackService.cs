using DC_bot.Interface.Service.Music.Playback;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Playback;
using DC_bot.Wrapper.Lavalink;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.Playback;

/// <summary>
/// Reads and persists the current track and formats it for command responses.
/// </summary>
/// <param name="playbackStateRepository">The playback state repository dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
public class CurrentTrackService(
    IPlaybackStateRepository playbackStateRepository,
    ITrackSerializer trackSerializer,
    ILogger<CurrentTrackService>? logger = null) : ICurrentTrackService
{
    private readonly ILogger<CurrentTrackService> _logger = logger ?? NullLogger<CurrentTrackService>.Instance;
    private readonly ITrackSerializer _trackSerializer = trackSerializer ?? throw new ArgumentNullException(nameof(trackSerializer));

    /// <summary>
    /// Restores the persisted current track, returning null when no current track can be resolved.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var state = await playbackStateRepository.GetOrCreateAsync(guildId, cancellationToken);

        if (state.CurrentTrackIdentifier is null)
        {
            _logger.LogDebug("No current track stored for guild {GuildId}.", guildId);
            return null;
        }

        try
        {
            var track = _trackSerializer.Deserialize(state.CurrentTrackIdentifier, state.QueueItemId);
            _logger.LogDebug("Current track loaded for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, state.QueueItemId);
            return track;
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to parse current track identifier for guild {GuildId}.", guildId);
            return null;
        }
    }

    /// <summary>
    /// Persists current-track identity and publishes the corresponding playback update.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="track">The lava link track? dependency used by this boundary.</param>
    /// <param name="realtimeEventName">The realtime event published with persistence, or the operation default when omitted.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SetCurrentTrackAsync(
        ulong guildId,
        ILavaLinkTrack? track,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var identifier = track is null ? null : _trackSerializer.Serialize(track);
        long? queueItemId = null;

        if (track is LavaLinkTrackWrapper wrapper)
        {
            queueItemId = wrapper.QueueItemId;
        }

        await playbackStateRepository.SetCurrentTrackAsync(guildId, identifier, queueItemId, realtimeEventName, cancellationToken);

        if (track is null)
        {
            _logger.LogInformation("Current track cleared for guild {GuildId}.", guildId);
            return;
        }

        _logger.LogDebug("Current track persisted for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, queueItemId);
    }

    /// <summary>
    /// Formats the persisted current track for a command response.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var track = await GetCurrentTrackAsync(guildId, cancellationToken);
        return track is not null ? $"{track.Author} {track.Title}" : string.Empty;
    }
}
