using DC_bot.Interface.Service.Persistence.Models;

namespace DC_bot.Interface.Service.Persistence;

public interface IQueueRepository
{
    Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(
        ulong guildId,
        CancellationToken cancellationToken = default);
    Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<QueueItemRecord?> GetNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<QueueItemRecord> EnqueueAsync(
        ulong guildId,
        string trackIdentifier,
        CancellationToken cancellationToken = default);

    Task EnqueueManyAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken = default);

    Task ReorderQueuedItemsAsync(
        ulong guildId,
        IReadOnlyList<string> trackIdentifiers,
        CancellationToken cancellationToken = default);

    Task UpdateQueueItemPositionAsync(long queueItemId, int newPosition, CancellationToken cancellationToken = default);

    Task MarkPlayingAsync(long queueItemId, CancellationToken cancellationToken = default);

    Task MarkPlayedAsync(long queueItemId, CancellationToken cancellationToken = default);

    Task MarkSkippedAsync(long queueItemId, CancellationToken cancellationToken = default);
    Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task<QueueItemRecord?> ClaimNextQueuedItemAsync(ulong guildId, CancellationToken cancellationToken = default);
}
