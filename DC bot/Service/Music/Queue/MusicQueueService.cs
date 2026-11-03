using DC_bot.BotControl;
using DC_bot.Interface.Service.Music.Queue;
using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;
using DC_bot.Interface.Service.Persistence.Models.Queue;
using DC_bot.Interface.Service.Persistence.Queue;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.Queue;

/// <summary>
/// Restores durable queue entries and applies queue edits and canonical batch-repeat operations.
/// </summary>
/// <param name="queueRepository">The queue repository dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
/// <param name="trackSerializer">The track serializer dependency used by this boundary.</param>
public class MusicQueueService(
    IQueueRepository queueRepository,
    ITrackSerializer trackSerializer,
    ILogger<MusicQueueService>? logger = null) : IMusicQueueService
{
    private const int MaxQueueSize = 50;
    private readonly ILogger<MusicQueueService> _logger = logger ?? NullLogger<MusicQueueService>.Instance;
    private readonly ITrackSerializer _trackSerializer = trackSerializer ?? throw new ArgumentNullException(nameof(trackSerializer));

    /// <summary>
    /// Atomically enables repetition of the live canonical batch when it contains active playback work.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public Task<bool> TryEnableBatchRepeatAsync(ulong guildId, CancellationToken cancellationToken = default) => queueRepository.TryEnableBatchRepeatAsync(guildId, cancellationToken: cancellationToken);

    /// <summary>
    /// Reports whether the guild has durable waiting queue entries.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task yielding whether the requested check or operation succeeded.</returns>
    public async Task<bool> HasTracks(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tracks = await queueRepository.AnyQueuedItemsAsync(guildId, cancellationToken: cancellationToken);
        _logger.LogDebug("Queue state queried for guild {GuildId}. Queued track count: {TrackCount}", guildId, tracks);
        return tracks;
    }

    /// <summary>
    /// Serializes incoming tracks and appends their source metadata to the active batch and waiting queue.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="tracks">The tracks supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task EnqueueMany(
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ArgumentNullException.ThrowIfNull(tracks);

        if (tracks.Count == 0)
        {
            return;
        }

        var queueItems = tracks.Select(track => new QueueItemToEnqueue(_trackSerializer.Serialize(track.Track), track.SourceQuery, SerializeSearchMode(track.SourceSearchMode), track.RequestedBy))
            .ToList();
        await queueRepository.EnqueueManyAsync(guildId, queueItems, cancellationToken: cancellationToken);
        _logger.LogInformation("{TrackCount} tracks enqueued for guild {GuildId} in bulk.", queueItems.Count, guildId);
    }

    /// <summary>
    /// Returns a read-only view of parseable waiting tracks in durable queue order.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var tracks = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken: cancellationToken);
        var lavaLinkTracks = new List<ILavaLinkTrack>(tracks.Count);

        foreach (var track in tracks)
        {
            try
            {
                lavaLinkTracks.Add(_trackSerializer.Deserialize(track.TrackIdentifier));
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping unparsable track identifier in ViewQueue for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, track.Id);
                var skipped = await queueRepository.TryMarkQueuedAsSkippedAsync(track.Id, cancellationToken);

                if (!skipped)
                {
                    _logger.LogDebug("Queue item state changed before parse-failure cleanup in ViewQueue for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, track.Id);
                }
            }
        }

        _logger.LogDebug("Queue view loaded for guild {GuildId}. Track count: {TrackCount}", guildId, lavaLinkTracks.Count);
        return lavaLinkTracks;
    }

    /// <summary>
    /// Restores waiting tracks and marks unparseable persisted queue entries skipped.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var queue = await queueRepository.GetQueuedItemsAsync(guildId, cancellationToken: cancellationToken);
        var trackQueue = new Queue<ILavaLinkTrack>(queue.Count);

        foreach (var queueItem in queue)
        {
            try
            {
                trackQueue.Enqueue(_trackSerializer.Deserialize(queueItem.TrackIdentifier));
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping unparsable track identifier in GetQueue for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, queueItem.Id);
                var skipped = await queueRepository.TryMarkQueuedAsSkippedAsync(queueItem.Id, cancellationToken);

                if (!skipped)
                {
                    _logger.LogDebug("Queue item state changed before parse-failure cleanup in GetQueue for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, queueItem.Id);
                }
            }
        }

        _logger.LogDebug("Queue snapshot created for guild {GuildId}. Track count: {TrackCount}", guildId, trackQueue.Count);
        return trackQueue;
    }

    /// <summary>
    /// Restores all canonical items of the active batch, including played tracks, logging unparseable entries.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(
        ulong guildId,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var batchItems = await queueRepository.GetActiveBatchItemsAsync(guildId, cancellationToken: cancellationToken);
        var repeatableQueue = new List<ILavaLinkTrack>(batchItems.Count);

        foreach (var item in batchItems)
        {
            try
            {
                repeatableQueue.Add(_trackSerializer.Deserialize(item.TrackIdentifier));
            }
            catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Skipping unparsable repeat-list track identifier for guild {GuildId}. Identifier: {Identifier}", guildId, item.TrackIdentifier);
            }
        }

        _logger.LogDebug("Repeatable queue snapshot loaded for guild {GuildId}. Track count: {TrackCount}", guildId, repeatableQueue.Count);
        return repeatableQueue;
    }

    /// <summary>
    /// Persists the requested order of waiting tracks without rebuilding canonical batch membership.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="shuffledQueue">The shuffled queue supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Queue reorder requested for guild {GuildId}. Incoming track count: {TrackCount}", guildId, shuffledQueue.Count);
        await SaveQueue(guildId, shuffledQueue, MobileRealtimeEventNames.QueueItemMoved, cancellationToken);
    }

    /// <summary>
    /// Marks waiting tracks skipped while preserving the playing track and canonical batch history.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    public async Task ClearQueue(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        _logger.LogInformation("Queue clear requested for guild {GuildId}.", guildId);
        await queueRepository.MarkAllQueuedAsSkippedAsync(guildId, cancellationToken: cancellationToken);
        _logger.LogInformation("Queue clear completed for guild {GuildId}.", guildId);
    }

    /// <summary>
    /// Shuffles waiting tracks and persists their order while retaining queue-item identities.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<QueueShuffleResult> ShuffleQueue(ulong guildId, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var queue = await GetQueue(guildId, cancellationToken);

        if (queue.Count < 2)
        {
            return new QueueShuffleResult(false, queue.Count);
        }

        var shuffled = queue.OrderBy(_ => Random.Shared.Next())
            .ToList();
        await SaveQueue(guildId, new Queue<ILavaLinkTrack>(shuffled), MobileRealtimeEventNames.QueueShuffled, cancellationToken);
        return new QueueShuffleResult(true, shuffled.Count);
    }

    /// <summary>
    /// Removes a numbered waiting track and reports its title and the remaining queue size.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="trackNumber">The track number supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<QueueRemoveResult> RemoveAt(ulong guildId, int trackNumber, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var result = await queueRepository.RemoveQueuedItemAtAsync(guildId, trackNumber, cancellationToken: cancellationToken);

        if (!result.Success || result.RemovedItem is null)
        {
            return new QueueRemoveResult(false, trackNumber, result.QueueSize, null);
        }

        ILavaLinkTrack? removedTrack = null;
        try
        {
            removedTrack = _trackSerializer.Deserialize(result.RemovedItem.TrackIdentifier, result.RemovedItem.Id);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Removed queue item could not be parsed for guild {GuildId}. QueueItemId: {QueueItemId}", guildId, result.RemovedItem.Id);
        }

        return new QueueRemoveResult(true, trackNumber, result.QueueSize, removedTrack?.Title);
    }

    /// <summary>
    /// Moves a waiting track to the requested position and reports the resulting queue indices.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="trackIndex">The track index supplied for this operation.</param>
    /// <param name="targetIndex">The target index supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    public async Task<QueueMoveResult> MoveToIndex(
        ulong guildId,
        int trackIndex,
        int targetIndex,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        var queue = await GetQueue(guildId, cancellationToken);
        var tracks = queue.ToList();

        if (trackIndex < 0
            || trackIndex >= tracks.Count
            || targetIndex < 0
            || targetIndex >= tracks.Count)
        {
            return new QueueMoveResult(false, trackIndex, targetIndex, tracks.Count);
        }

        if (trackIndex == targetIndex)
        {
            return new QueueMoveResult(true, trackIndex, targetIndex, tracks.Count);
        }

        var track = tracks[trackIndex];
        tracks.RemoveAt(trackIndex);
        tracks.Insert(targetIndex, track);
        await SaveQueue(guildId, new Queue<ILavaLinkTrack>(tracks), MobileRealtimeEventNames.QueueItemMoved, cancellationToken);
        return new QueueMoveResult(true, trackIndex, targetIndex, tracks.Count);
    }

    /// <summary>
    /// Checks the queue limit, serializes the requested track order, and persists that order with the selected realtime queue event.
    /// </summary>
    /// <param name="guildId">The Discord guild whose state or operation is addressed.</param>
    /// <param name="shuffledQueue">The shuffled queue supplied for this operation.</param>
    /// <param name="realtimeEventName">The realtime event published with persistence, or the operation default when omitted.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>A task representing completion of this operation.</returns>
    private async Task SaveQueue(
        ulong guildId,
        Queue<ILavaLinkTrack> shuffledQueue,
        string realtimeEventName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (shuffledQueue.Count > MaxQueueSize)
        {
            _logger.LogWarning("Queue reorder rejected for guild {GuildId}. Requested count {RequestedCount} exceeds max {MaxQueueSize}.", guildId, shuffledQueue.Count, MaxQueueSize);
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueueSize} tracks.");
        }

        var reorderedTrackIdentifiers = shuffledQueue.Select(_trackSerializer.Serialize)
            .ToList();
        await queueRepository.ReorderQueuedItemsAsync(guildId, reorderedTrackIdentifiers, realtimeEventName, cancellationToken: cancellationToken);
        _logger.LogInformation("Queue reorder persisted for guild {GuildId}. Track count: {TrackCount}", guildId, reorderedTrackIdentifiers.Count);
    }

    /// <summary>
    /// Converts the selected provider to its persisted source-search-mode representation.
    /// </summary>
    /// <param name="sourceSearchMode">The source search mode supplied for this operation.</param>
    /// <returns>The formatted, serialized, or resolved text value.</returns>
    private static string? SerializeSearchMode(TrackSearchMode? sourceSearchMode)
    {
        if (sourceSearchMode is null)
        {
            return null;
        }

        var mode = sourceSearchMode.Value;

        if (mode == TrackSearchMode.YouTube)
        {
            return nameof(TrackSearchMode.YouTube);
        }

        if (mode == TrackSearchMode.YouTubeMusic)
        {
            return nameof(TrackSearchMode.YouTubeMusic);
        }

        if (mode == TrackSearchMode.SoundCloud)
        {
            return nameof(TrackSearchMode.SoundCloud);
        }

        if (mode == TrackSearchMode.Spotify)
        {
            return nameof(TrackSearchMode.Spotify);
        }

        if (mode == TrackSearchMode.AppleMusic)
        {
            return nameof(TrackSearchMode.AppleMusic);
        }

        if (mode == TrackSearchMode.Deezer)
        {
            return nameof(TrackSearchMode.Deezer);
        }

        if (mode == TrackSearchMode.YandexMusic)
        {
            return nameof(TrackSearchMode.YandexMusic);
        }

        if (mode == TrackSearchMode.Bandcamp)
        {
            return nameof(TrackSearchMode.Bandcamp);
        }

        if (mode == TrackSearchMode.None)
        {
            return nameof(TrackSearchMode.None);
        }

        return null;
    }
}
