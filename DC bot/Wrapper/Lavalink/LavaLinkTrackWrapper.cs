using DC_bot.Interface.Service.Music.Tracks;
using Lavalink4NET.Tracks;

namespace DC_bot.Wrapper.Lavalink;

public class LavaLinkTrackWrapper(LavalinkTrack track) : ILavaLinkTrack
{
    public string Title => track.Title;
    public string Author => track.Author;
    public TimeSpan Duration => track.Duration;
    public Uri? ArtworkUri => track.ArtworkUri;
    public TimeSpan? StartPosition => track.StartPosition;
    public long? QueueItemId { get; init; }
    public LavalinkTrack ToLavalinkTrack()
    {
        return track;
    }

    public override string ToString()
    {
        return track.ToString();
    }
}