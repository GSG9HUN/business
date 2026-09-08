using DC_bot.Interface.Service.Persistence.Models.Playback;

namespace DC_bot.Interface.Service.Persistence.Playback;

public interface IPlaybackStateRepository
{
    Task<PlaybackStateRecord> GetOrCreateAsync(ulong guildId, CancellationToken cancellationToken = default);

    Task SetRepeatStateAsync(
        ulong guildId,
        bool isRepeating,
        bool isRepeatingList,
        CancellationToken cancellationToken = default);

    Task SetCurrentTrackAsync(
        ulong guildId, 
        string? trackIdentifier, 
        long? queueItemId, 
        CancellationToken cancellationToken = default);

    Task SetPlaybackPositionAsync(
        ulong guildId,
        TimeSpan position,
        bool isPaused,
        CancellationToken cancellationToken = default);
}
