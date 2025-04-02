using Lavalink4NET.Tracks;

namespace DC_bot.Interface;

public interface ILavaLinkTrack
{
    string Title { get; }
    string Author { get; }
    
    LavalinkTrack ToLavalinkTrack();
}