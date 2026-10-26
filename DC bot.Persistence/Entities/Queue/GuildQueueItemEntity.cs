using DC_bot.Entities.Guilds;
using DC_bot.Interface.Service.Persistence.Models.Queue;

namespace DC_bot.Entities.Queue;

/// <summary>
/// Represents the persisted guild queue item row mapped by the EF Core model.
/// </summary>
public class GuildQueueItemEntity
{

    /// <summary>
    /// The identifier of this persisted record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// The playback batch identifier, or null for a queue item not yet assigned to a batch.
    /// </summary>
    public long? BatchId { get; set; }

    /// <summary>
    /// The canonical playback batch item identifier, or null when no batch item is assigned.
    /// </summary>
    public long? BatchItemId { get; set; }

    /// <summary>
    /// The item's position in its ordered collection.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// The stored track identity used to resolve the audio track.
    /// </summary>
    public string TrackIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The original search query or URL, or null when unavailable.
    /// </summary>
    public string? SourceQuery { get; set; }

    /// <summary>
    /// The original search provider or mode, or null when unspecified.
    /// </summary>
    public string? SourceSearchMode { get; set; }

    /// <summary>
    /// The stored requester information, or null when unavailable.
    /// </summary>
    public string? RequestedBy { get; set; }

    /// <summary>
    /// The identifier of the playback attempt that owns this queue entry.
    /// </summary>
    public Guid? PlaybackOperationId { get; set; }

    /// <summary>
    /// The state saved before a start reservation, used when rolling back that reservation.
    /// </summary>
    public QueueItemState? StartPreviousState { get; set; }

    /// <summary>
    /// The previous playback operation identifier saved for start rollback.
    /// </summary>
    public Guid? StartPreviousOperationId { get; set; }

    /// <summary>
    /// The Lavalink node address bound to the playback attempt, or null when not bound.
    /// </summary>
    public string? PlaybackNodeAddress { get; set; }

    /// <summary>
    /// The Lavalink node session identifier bound to the playback attempt, or null when not bound.
    /// </summary>
    public string? PlaybackNodeSessionId { get; set; }

    /// <summary>
    /// Whether the playback start was confirmed at the transport boundary.
    /// </summary>
    public bool PlaybackTransportConfirmed { get; set; }

    /// <summary>
    /// The persisted lifecycle state of the queue entry.
    /// </summary>
    public QueueItemState State { get; set; }

    /// <summary>
    /// The UTC time at which the item was added.
    /// </summary>
    public DateTimeOffset AddedAtUtc { get; set; }

    /// <summary>
    /// The recorded UTC playback completion time, or null before completion.
    /// </summary>
    public DateTimeOffset? PlayedAtUtc { get; set; }

    /// <summary>
    /// The UTC time at which the queue entry was skipped, or null if it was not skipped.
    /// </summary>
    public DateTimeOffset? SkippedAtUtc { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
