using DC_bot.Interface.Service.Music.Tracks;

namespace DC_bot.Interface.Service.Music.Playback;

public interface ICurrentTrackService
{
    Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task SetCurrentTrackAsync(
        ulong guildId,
        ILavaLinkTrack? track,
        string? realtimeEventName = null,
        CancellationToken cancellationToken = default);
    Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default);
}
