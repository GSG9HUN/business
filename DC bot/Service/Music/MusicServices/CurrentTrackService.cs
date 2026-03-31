using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;

namespace DC_bot.Service.Music.MusicServices;

public class CurrentTrackService : ICurrentTrackService
{
    private readonly Dictionary<ulong, ILavaLinkTrack?> _currentTrack = new();

    public void Init(ulong guildId)
    {
        _currentTrack.TryAdd(guildId, null);
    }

    public ILavaLinkTrack? GetCurrentTrack(ulong guildId)
    {
        return _currentTrack.GetValueOrDefault(guildId);
    }

    public void SetCurrentTrack(ulong guildId, ILavaLinkTrack? track)
    {
        if (_currentTrack.ContainsKey(guildId)) _currentTrack[guildId] = track;
    }

    public string GetCurrentTrackFormatted(ulong guildId)
    {
        return _currentTrack.TryGetValue(guildId, out var track) && track != null
            ? $"{track.Author} {track.Title}"
            : string.Empty;
    }

    public bool TryGetCurrentTrack(ulong guildId, out ILavaLinkTrack? track)
    {
        track = null;
        if (!_currentTrack.TryGetValue(guildId, out var current) || current is null)
            return false;

        track = current;
        return true;
    }
}