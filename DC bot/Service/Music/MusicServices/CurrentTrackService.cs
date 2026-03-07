using DC_bot.Interface.Service.Music.MusicServiceInterface;
using Lavalink4NET.Tracks;

namespace DC_bot.Service.Music.MusicServices;

public class CurrentTrackService : ICurrentTrackService
{
    private readonly Dictionary<ulong, LavalinkTrack?> _currentTrack = new();

    public void Init(ulong guildId)
    {
        _currentTrack.TryAdd(guildId, null);
    }

    public LavalinkTrack? GetCurrentTrack(ulong guildId)
    {
        return _currentTrack.GetValueOrDefault(guildId);
    }

    public void SetCurrentTrack(ulong guildId, LavalinkTrack? track)
    {
        if (_currentTrack.ContainsKey(guildId))
        {
            _currentTrack[guildId] = track;
        }
    }

    public string GetCurrentTrackFormatted(ulong guildId)
    {
        return _currentTrack.TryGetValue(guildId, out var track) && track != null
            ? $"{track.Author} {track.Title}"
            : string.Empty;
    }

    public bool TryGetCurrentTrack(ulong guildId, out LavalinkTrack? track)
    {
        track = null;
        if (!_currentTrack.TryGetValue(guildId, out var current) || current is null)
            return false;

        track = current;
        return true;
    }
}

