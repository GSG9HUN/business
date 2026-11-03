using DC_bot.Entities.Guilds;

namespace DC_bot.Entities.Playlists;

/// <summary>
/// Represents the persisted playlist row mapped by the EF Core model.
/// </summary>
public class PlaylistEntity
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
    /// The saved playlist name.
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// The guild associated with this object.
    /// </summary>
    public GuildDataEntity Guild { get; set; } = null !;

    /// <summary>
    /// The ordered track entries belonging to the saved playlist.
    /// </summary>
    public ICollection<PlaylistTrackEntity> Tracks { get; set; } = new List<PlaylistTrackEntity>();
}
