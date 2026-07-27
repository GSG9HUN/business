namespace DC_bot.Interface.Service.Music;

public interface IRepeatService
{
    Task InitAsync(ulong guildId);
    Task<bool> IsRepeatingAsync(ulong guildId);
    Task SetRepeatingAsync(ulong guildId, bool value);
    Task<bool> IsRepeatingListAsync(ulong guildId);
    Task SetRepeatingListAsync(ulong guildId, bool value);
    Task SaveRepeatListSnapshotAsync(ulong guildId, ILavaLinkTrack? currentTrack, IReadOnlyCollection<ILavaLinkTrack> queuedTracks);
}
