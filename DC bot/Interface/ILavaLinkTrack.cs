using Lavalink4NET.Tracks;

namespace DC_bot.Interface;

public interface ILavaLinkTrack
{
    string Title { get; }
    string Author { get; }
    TimeSpan Duration { get; }
    TimeSpan? StartPosition { get; }
    Uri? ArtworkUri { get; }
    LavalinkTrack ToLavalinkTrack();
    string ToString();
}