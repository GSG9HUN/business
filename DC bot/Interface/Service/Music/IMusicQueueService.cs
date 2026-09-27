namespace DC_bot.Interface.Service.Music;

using Lavalink4NET.Rest.Entities.Tracks;

public interface IMusicQueueService
{
    public Task<bool> HasTracks(ulong guildId);

    public Task Enqueue(
        ulong guildId,
        ILavaLinkTrack track,
        string? sourceQuery,
        TrackSearchMode? sourceSearchMode,
        string? requestedBy = null);
    public Task EnqueueMany(
        ulong guildId,
        IReadOnlyCollection<QueueTrackToEnqueue> tracks);
    public Task<ILavaLinkTrack?> Dequeue(ulong guildId);

    public Task<IReadOnlyCollection<ILavaLinkTrack>> ViewQueue(ulong guildId);

    public Task<Queue<ILavaLinkTrack>> GetQueue(ulong guildId);
    Task<IReadOnlyCollection<ILavaLinkTrack>> GetRepeatableQueue(ulong guildId);

    public Task SetQueue(ulong guildId, Queue<ILavaLinkTrack> shuffledQueue);
    Task ClearQueue(ulong guildId);
    Task<QueueShuffleResult> ShuffleQueue(ulong guildId);
    Task<QueueRemoveResult> RemoveAt(ulong guildId, int trackNumber);
    Task<QueueMoveResult> Move(ulong guildId, int trackIndex, bool moveUp);
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
