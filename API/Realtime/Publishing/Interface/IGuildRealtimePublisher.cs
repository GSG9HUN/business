namespace API.Realtime.Publishing.Interface;

/// <summary>
/// Defines guild playback, queue, and bot presence snapshot publication.
/// </summary>
public interface IGuildRealtimePublisher
{

    /// <summary>
    /// Reads and publishes the current playback snapshot to authorized guild subscribers.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    Task PublishPlaybackAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads and publishes the current queue snapshot to authorized guild subscribers.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    Task PublishQueueAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);

    /// <summary>
    /// Reads and publishes the current bot presence snapshot to authorized guild subscribers.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="eventName">The event name value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>A task that completes when the operation and its required side effects finish.</returns>
    Task PublishGuildBotStatusAsync(ulong guildId, string eventName, CancellationToken cancellationToken = default);
}
