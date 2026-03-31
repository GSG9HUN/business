namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ICurrentTrackService
{
    void Init(ulong guildId);
    ILavaLinkTrack? GetCurrentTrack(ulong guildId);
    void SetCurrentTrack(ulong guildId, ILavaLinkTrack? track);
    string GetCurrentTrackFormatted(ulong guildId);
    bool TryGetCurrentTrack(ulong guildId, out ILavaLinkTrack? track);
}