namespace DC_bot.Interface.Service.Persistence.Exceptions;

/// <summary>Reports a rejected enqueue operation that would exceed the guild's waiting queue capacity.</summary>
/// <param name="limit">The maximum waiting queue size for a free guild.</param>
/// <param name="queuedCount">The number of tracks already waiting.</param>
/// <param name="requestedCount">The number of rejected incoming tracks.</param>
public sealed class QueueCapacityExceededException(int limit, int queuedCount, int requestedCount)
    : InvalidOperationException($"Queue cannot contain more than {limit} queued tracks.")
{

    /// <summary>Gets the maximum number of waiting tracks.</summary>
    public int Limit { get; } = limit;

    /// <summary>Gets the number of tracks already waiting.</summary>
    public int QueuedCount { get; } = queuedCount;

    /// <summary>Gets the number of rejected incoming tracks.</summary>
    public int RequestedCount { get; } = requestedCount;
}
