using Lavalink4NET.Tracks;

namespace DC_bot.Service.MusicServices;

/// <summary>
/// Manages currently playing track state for guilds.
/// </summary>
public class CurrentTrackService
{
    private readonly Dictionary<ulong, LavalinkTrack?> _currentTrack = new();

    public void Init(ulong guildId)
    {
        _currentTrack.TryAdd(guildId, null);
    }

    public LavalinkTrack? GetCurrentTrack(ulong guildId)
    {
        return _currentTrack.TryGetValue(guildId, out var track) ? track : null;
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

