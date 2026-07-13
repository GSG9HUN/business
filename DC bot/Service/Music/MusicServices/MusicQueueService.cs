using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.MusicServices;

public class MusicQueueService(
    IQueueRepository queueRepository,
    IRepeatListRepository repeatListRepository,
    ILogger<MusicQueueService>? logger = null,
    ITrackSerializer? trackSerializer = null) : IMusicQueueService
{
    private const int MaxQueueSize = 50;
    private readonly ILogger<MusicQueueService> _logger = logger ?? NullLogger<MusicQueueService>.Instance;
    private readonly ITrackSerializer _trackSerializer = trackSerializer ?? new LavalinkTrackSerializer();

    public async Task<bool> HasTracks(ulong guildId)
    {
        var tracks = await queueRepository.AnyQueuedItemsAsync(guildId);
        _logger.LogDebug("Queue state queried for guild {GuildId}. Queued track count: {TrackCount}", guildId, tracks);
        return tracks;
    }

    public async Task Enqueue(ulong guildId, ILavaLinkTrack track)
    {
        await queueRepository.EnqueueAsync(guildId, _trackSerializer.Serialize(track));
        _logger.LogInformation("Track enqueued for guild {GuildId}: {Author} - {Title}", guildId, track.Author, track.Title);
    }

    public async Task EnqueueMany(ulong guildId, IReadOnlyCollection<ILavaLinkTrack> tracks)
    {
        ArgumentNullException.ThrowIfNull(tracks);
        if (tracks.Count == 0)
        {
            return;
        }

        var trackIdentifiers = tracks
            .Select(_trackSerializer.Serialize)
            .ToList();

        await queueRepository.EnqueueManyAsync(guildId, trackIdentifiers);
        _logger.LogInformation("{TrackCount} tracks enqueued for guild {GuildId} in bulk.", trackIdentifiers.Count, guildId);
    }

    public async Task<ILavaLinkTrack?> Dequeue(ulong guildId)
    {
        while (true)
        {
            var queueItemRecord = await queueRepository.ClaimNextQueuedItemAsync(guildId);
            if (queueItemRecord == null)
            {
                _logger.LogDebug("Dequeue requested for guild {GuildId}, but queue is empty.", guildId);
                return null;
            }

            try
            {
                var wrappedTrack = _trackSerializer.Deserialize(queueItemRecord.TrackIdentifier, queueItemRecord.Id);
                _logger.LogInformation("Track dequeued for guild {GuildId}: {Title}", guildId, wrappedTrack.Title);
                return wrappedTrack;
            }
            catch (Exception ex)
            {
                await queueRepository.MarkSkippedAsync(queueItemRecord.Id);

                _logger.LogWarning(ex, "Failed to parse track for guild {GuildId}. Item {Id} marked as skipped. Trying next item.",
                    guildId, queueItemRecord.Id);
            }
        }
    }

    public async Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId)
    {
        var tracks = await queueRepository.GetQueuedItemsAsync(guildId);
        var lavaLinkTracks = new List<ILavaLinkTrack>(tracks.Count);
        foreach (var track in tracks)
        {
            try
            {
                lavaLinkTracks.Add(_trackSerializer.Deserialize(track.TrackIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Skipping unparsable track identifier in ViewQueue for guild {GuildId}. QueueItemId: {QueueItemId}",
                    guildId,
                    track.Id);
                await queueRepository.MarkSkippedAsync(track.Id);
            }
        }

        _logger.LogDebug("Queue view loaded for guild {GuildId}. Track count: {TrackCount}", guildId, lavaLinkTracks.Count);
        return lavaLinkTracks;
    }
    
    public async Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId)
    {
        var queue = await queueRepository.GetQueuedItemsAsync(guildId);
        var trackQueue = new Queue<ILavaLinkTrack>(queue.Count);
        foreach (var queueItem in queue)
        {
            try
            {
                trackQueue.Enqueue(_trackSerializer.Deserialize(queueItem.TrackIdentifier));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Skipping unparsable track identifier in GetQueue for guild {GuildId}. QueueItemId: {QueueItemId}",
                    guildId,
                    queueItem.Id);
                await queueRepository.MarkSkippedAsync(queueItem.Id);
            }
        }

        _logger.LogDebug("Queue snapshot created for guild {GuildId}. Track count: {TrackCount}", guildId, trackQueue.Count);
        return trackQueue;
    }

    public async Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(ulong guildId)
    {
        var trackIdentifiers = await repeatListRepository.GetTrackIdentifiersAsync(guildId);
        var repeatableQueue = new List<ILavaLinkTrack>(trackIdentifiers.Count);

        foreach (var identifier in trackIdentifiers)
        {
            try
            {
                repeatableQueue.Add(_trackSerializer.Deserialize(identifier));
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex,
                    "Skipping unparsable repeat-list track identifier for guild {GuildId}. Identifier: {Identifier}",
                    guildId,
                    identifier);
            }
        }

        _logger.LogDebug("Repeatable queue snapshot loaded for guild {GuildId}. Track count: {TrackCount}",
            guildId,
            repeatableQueue.Count);
        return repeatableQueue;
    }

    public async Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue)
    {
        _logger.LogInformation("Queue reorder requested for guild {GuildId}. Incoming track count: {TrackCount}", guildId,
            shuffledQueue.Count);
        await SaveQueue(guildId, shuffledQueue);
    }

    public async Task ClearQueue(ulong guildId)
    {
        _logger.LogInformation("Queue clear requested for guild {GuildId}.", guildId);

        await queueRepository.MarkAllQueuedAsSkippedAsync(guildId);

        _logger.LogInformation("Queue clear completed for guild {GuildId}.", guildId);
    }

    private async Task SaveQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue)
    {
        if (shuffledQueue.Count > MaxQueueSize)
        {
            _logger.LogWarning("Queue reorder rejected for guild {GuildId}. Requested count {RequestedCount} exceeds max {MaxQueueSize}.",
                guildId,
                shuffledQueue.Count,
                MaxQueueSize);
            throw new InvalidOperationException($"Queue cannot contain more than {MaxQueueSize} tracks.");
        }

        var reorderedTrackIdentifiers = shuffledQueue
            .Select(_trackSerializer.Serialize)
            .ToList();

        await queueRepository.ReorderQueuedItemsAsync(guildId, reorderedTrackIdentifiers);
        _logger.LogInformation("Queue reorder persisted for guild {GuildId}. Track count: {TrackCount}", guildId,
            reorderedTrackIdentifiers.Count);
    }
}
