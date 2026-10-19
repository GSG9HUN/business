using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Interface.Service.Persistence.Queue;

public interface IQueueRepository
{
    Task<IReadOnlyList<QueueItemRecord>> GetQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);
    
    Task<bool> AnyQueuedItemsAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<QueueItemRecord?> GetPreviousItemAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task<QueueItemRecord?> GetByIdAsync(long queueItemId, CancellationToken cancellationToken = default);

    Task EnqueueManyAsync(ulong guildId, IReadOnlyList<QueueItemToEnqueue> queueItems, CancellationToken cancellationToken = default);

    Task ReorderQueuedItemsAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, string? realtimeEventName = null, CancellationToken cancellationToken = default);

    Task<QueueItemRemovalRecord> RemoveQueuedItemAtAsync(ulong guildId, int trackNumber, CancellationToken cancellationToken = default);
    
    Task MarkAllQueuedAsSkippedAsync(ulong guildId, CancellationToken cancellationToken = default);
    
    Task<QueueClaimResult> ClaimForStartAsync(ulong guildId, Guid operationId, long? queueItemId = null, CancellationToken cancellationToken = default);
    
    Task<bool> CompleteStartAsync(ulong guildId, long queueItemId, Guid operationId, string trackIdentifier, string realtimeEventName, bool clearSourceMetadata = false, CancellationToken cancellationToken = default);
  
    Task<bool> ReleaseStartAsync(ulong guildId, long queueItemId, Guid operationId, bool skip, CancellationToken cancellationToken = default);
    
    Task<bool> CompleteAttemptAsync(ulong guildId, long queueItemId, Guid operationId, bool played, CancellationToken cancellationToken = default);
   
    Task<bool> TryMarkQueuedAsSkippedAsync(long queueItemId, CancellationToken cancellationToken = default);
    
    Task<IReadOnlyList<PendingPlaybackStart>> GetPendingStartsAsync(ulong? guildId = null, CancellationToken cancellationToken = default);

    Task<bool> BindStartNodeAsync(ulong guildId, long queueItemId, Guid operationId, string nodeAddress, string nodeSessionId, CancellationToken cancellationToken = default);

    Task<bool> ResolveDisconnectedStartAsync(ulong guildId, long queueItemId, Guid operationId, CancellationToken cancellationToken = default);

    Task<bool> ConfirmStartTransportAsync(ulong guildId, long queueItemId, Guid operationId, CancellationToken cancellationToken = default);
}
