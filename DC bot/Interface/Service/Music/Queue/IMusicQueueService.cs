using DC_bot.Interface.Service.Music.Queue.Models;
using DC_bot.Interface.Service.Music.Tracks;

namespace DC_bot.Interface.Service.Music.Queue;

public interface IMusicQueueService
{
    public Task<bool> HasTracks(ulong guildId);

    public Task EnqueueMany(
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks);

    public Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId);

    public Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId);
    Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(ulong guildId);

    public Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue);
    Task ClearQueue(ulong guildId);
    Task<QueueShuffleResult> ShuffleQueue(ulong guildId);
    Task<QueueRemoveResult> RemoveAt(ulong guildId, int trackNumber);
    Task<QueueMoveResult> MoveToIndex(ulong guildId, int trackIndex, int targetIndex);
}

public sealed record QueueShuffleResult(bool Success, int TrackCount);

public sealed record QueueRemoveResult(
    bool Success,
    int TrackNumber,
    int QueueSize,
    string? RemovedTrackTitle);

public sealed record QueueMoveResult(
    bool Success,
    int From,
    int To,
    int QueueSize);
