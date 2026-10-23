namespace DC_bot.Interface.Service.Persistence.Playback;

/// <summary>
/// Defines shared storage operations for repeat list repository, including caller cancellation and immutable result contracts.
/// </summary>
public interface IRepeatListRepository
{

    /// <summary>
    /// Reads the legacy repeat-list snapshot in its persisted playback order.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>The matching records in the documented order, or an empty collection when none match.</returns>
    Task<IReadOnlyList<string>> GetTrackIdentifiersAsync(ulong guildId, CancellationToken cancellationToken = default);

    /// <summary>
    /// Replaces the guild&apos;s legacy repeat-list snapshot with the supplied ordered track identifiers.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "trackIdentifiers">The track identities in the requested durable order, preserving duplicate occurrences.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task ReplaceAsync(ulong guildId, IReadOnlyList<string> trackIdentifiers, CancellationToken cancellationToken = default);

    /// <summary>
    /// Removes the guild&apos;s persisted legacy repeat-list snapshot.
    /// </summary>
    /// <param name = "guildId">The Discord guild whose persisted state is addressed.</param>
    /// <param name = "cancellationToken">Cancels supported database work and retry backoff; owned rollback uses its own deadline.</param>
    /// <returns>A task representing completion of the documented operation.</returns>
    Task ClearAsync(ulong guildId, CancellationToken cancellationToken = default);
}
