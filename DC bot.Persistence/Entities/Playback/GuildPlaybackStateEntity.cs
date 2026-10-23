using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playback;

/// <summary>
/// Represents the persisted guild playback state row mapped by the EF Core model.
/// </summary>
public class GuildPlaybackStateEntity
{

    /// <summary>
    /// The Discord guild identifier.
    /// </summary>
    public ulong GuildId { get; set; }

    /// <summary>
    /// Whether the current track should repeat.
    /// </summary>
    public bool IsRepeating { get; set; }

    /// <summary>
    /// Whether the active playback batch should repeat.
    /// </summary>
    public bool IsRepeatingList { get; set; }

    /// <summary>
    /// Whether playback is paused.
    /// </summary>
    public bool IsPaused { get; set; }

    /// <summary>
    /// The persisted playback position in seconds.
    /// </summary>
    public int PositionSeconds { get; set; }

    /// <summary>
    /// The UTC time at which the playback position was recorded, or null when unavailable.
    /// </summary>
    public DateTimeOffset? PositionUpdatedAtUtc { get; set; }

    /// <summary>
    /// The current track identifier, or null when no track is selected.
    /// </summary>
    public string? CurrentTrackIdentifier { get; set; }

    /// <summary>
    /// The associated queue entry identifier, or null when no entry is associated.
    /// </summary>
    public long? QueueItemId { get; set; }

    /// <summary>
    /// The active playback batch identifier, or null when no batch is active.
    /// </summary>
    public long? ActiveBatchId { get; set; }

    /// <summary>
    /// The UTC time of the most recent persisted update.
    /// </summary>
    public DateTimeOffset UpdatedAtUtc { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
