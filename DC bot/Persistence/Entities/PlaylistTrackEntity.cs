namespace DC_bot.Persistence.Entities;

public class PlaylistTrackEntity
{
    public long Id { get; set; }
    public long PlaylistId { get; set; }
    public int OrderNumber { get; set; }
    public string Source { get; set; } = string.Empty;
    public string TrackIdentifier { get; set; } = string.Empty;
    public string TrackUri { get; set; } = string.Empty;
    public PlaylistEntity Playlist { get; set; } = null!;
}