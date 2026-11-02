namespace DC_bot.Entities.Playback;

/// <summary>
/// Represents the persisted playback batch item row mapped by the EF Core model.
/// </summary>
public class PlaybackBatchItemEntity
{

    /// <summary>
    /// The identifier of this persisted record.
    /// </summary>
    public long Id { get; set; }

    /// <summary>
    /// The identifier of the playback batch containing this canonical item.
    /// </summary>
    public long BatchId { get; set; }

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
    /// The UTC time at which the item was added.
    /// </summary>
    public DateTimeOffset AddedAtUtc { get; set; }

    /// <summary>
    /// The playback batch containing this item.
    /// </summary>
    public PlaybackBatchEntity Batch { get; set; } = null !;
}
