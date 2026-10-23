using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playback;

/// <summary>
/// Represents the persisted guild repeat list item row mapped by the EF Core model.
/// </summary>
public class GuildRepeatListItemEntity
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
    /// The item's position in its ordered collection.
    /// </summary>
    public int Position { get; set; }

    /// <summary>
    /// The stored track identity used to resolve the audio track.
    /// </summary>
    public string TrackIdentifier { get; set; } = string.Empty;

    /// <summary>
    /// The UTC time at which the item was added.
    /// </summary>
    public DateTimeOffset AddedAtUtc { get; set; }

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;
}
