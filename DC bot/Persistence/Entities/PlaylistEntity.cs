namespace DC_bot.Persistence.Entities;

public class PlaylistEntity
{
    public long Id { get; set; }
    public ulong GuildId { get; set; }
    public string Name { get; set; } = string.Empty;
    public GuildDataEntity Guild { get; set; } = null!;
    public ICollection<PlaylistTrackEntity> Tracks { get; set; } = new List<PlaylistTrackEntity>();
}