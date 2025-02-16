using DC_bot.Interface;
using DSharpPlus.Lavalink;

namespace DC_bot.Wrapper;

public class LavaLinkTrackWrapper(LavalinkTrack track) : ILavaLinkTrack
{
    public string Title => track.Title;
    public string Author => track.Author;
    public LavalinkTrack ToLavalinkTrack()
    {
        return track;
    }
}