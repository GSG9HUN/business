using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace DC_bot.Service.Music.MusicServices;

public class MusicQueueService(IQueueRepository queueRepository, ILogger<MusicQueueService>? logger = null) : IMusicQueueService
{
    private const int MaxQueueSize = 100;
    private readonly ILogger<MusicQueueService> _logger = logger ?? NullLogger<MusicQueueService>.Instance;

    public async Task<bool> HasTracks(ulong guildId)
    {
        var tracks = await queueRepository.AnyQueuedItemsAsync(guildId);
        _logger.LogDebug("Queue state queried for guild {GuildId}. Queued track count: {TrackCount}", guildId, tracks);
        return tracks;
    }

    public async Task Enqueue(ulong guildId, ILavaLinkTrack track)
    {
        await queueRepository.EnqueueAsync(guildId, track.ToString());
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
            .Select(track => track.ToString())
            .ToList();

        await queueRepository.EnqueueManyAsync(guildId, trackIdentifiers);
        _logger.LogInformation("{TrackCount} tracks enqueued for guild {GuildId} in bulk.", trackIdentifiers.Count, guildId);
    }

    public async Task<ILavaLinkTrack?> Dequeue(ulong guildId)
    {
        var queueItemRecord = await queueRepository.ClaimNextQueuedItemAsync(guildId);
        if (queueItemRecord == null)
        {
            _logger.LogDebug("Dequeue requested for guild {GuildId}, but queue is empty.", guildId);
            return null;
        }

        LavalinkTrack track;
        try
        {
            track = LavalinkTrack.Parse(queueItemRecord.TrackIdentifier, null);
            var wrappedTrack = new LavaLinkTrackWrapper(track) 
            { 
                QueueItemId = queueItemRecord.Id 
            };
            _logger.LogInformation("Track dequeued for guild {GuildId}: {Title}", guildId, track.Title);
            return wrappedTrack;
        }
        catch (Exception ex)
        {
            await queueRepository.MarkSkippedAsync(queueItemRecord.Id);
        
            _logger.LogWarning(ex, "Failed to parse track for guild {GuildId}. Item {Id} marked as skipped.", 
                guildId, queueItemRecord.Id);
            
            return null;
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
                var lavaLinkTrack = LavalinkTrack.Parse(track.TrackIdentifier, null);
                lavaLinkTracks.Add(new LavaLinkTrackWrapper(lavaLinkTrack));
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
                var track = LavalinkTrack.Parse(queueItem.TrackIdentifier, null);
                trackQueue.Enqueue(new LavaLinkTrackWrapper(track));
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

    public async Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue)
    {
        _logger.LogInformation("Queue reorder requested for guild {GuildId}. Incoming track count: {TrackCount}", guildId,
            shuffledQueue.Count);
        await SaveQueue(guildId, shuffledQueue);
    }

    public IEnumerable<ILavaLinkTrack> GetRepeatableQueue(ulong guildId)
    {
        throw new NotImplementedException();
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
            .Select(track => track.ToString())
            .ToList();

        await queueRepository.ReorderQueuedItemsAsync(guildId, reorderedTrackIdentifiers);
        _logger.LogInformation("Queue reorder persisted for guild {GuildId}. Track count: {TrackCount}", guildId,
            reorderedTrackIdentifiers.Count);
    }
}