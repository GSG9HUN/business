namespace DC_bot.Interface.Service.Music.MusicServiceInterface;

public interface ICurrentTrackService
{
    Task<ILavaLinkTrack?> GetCurrentTrackAsync(ulong guildId, CancellationToken cancellationToken = default);
    Task SetCurrentTrackAsync(ulong guildId, ILavaLinkTrack? track, CancellationToken cancellationToken = default);
    Task<string> GetCurrentTrackFormattedAsync(ulong guildId, CancellationToken cancellationToken = default);
}