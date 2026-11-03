using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playback;

/// <summary>
/// Represents the persisted playback batch row mapped by the EF Core model.
/// </summary>
public class PlaybackBatchEntity
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
    /// The UTC time at which this record was created.
    /// </summary>
    public DateTimeOffset CreatedAtUtc { get; set; }

    /// <summary>
    /// The UTC completion time, or null while the operation remains open.
    /// </summary>
    public DateTimeOffset? CompletedAtUtc { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
