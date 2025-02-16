using DSharpPlus.Lavalink;

namespace DC_bot.Interface;

public interface ILavaLinkTrack
{
    string Title { get; }
    string Author { get; }
    
    LavalinkTrack ToLavalinkTrack();
}