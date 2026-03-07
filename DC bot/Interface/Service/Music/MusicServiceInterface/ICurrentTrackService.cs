using Lavalink4NET.Tracks;

namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ICurrentTrackService
{
    void Init(ulong guildId);
    LavalinkTrack? GetCurrentTrack(ulong guildId);
    void SetCurrentTrack(ulong guildId, LavalinkTrack? track);
    string GetCurrentTrackFormatted(ulong guildId);
    bool TryGetCurrentTrack(ulong guildId, out LavalinkTrack? track);
}