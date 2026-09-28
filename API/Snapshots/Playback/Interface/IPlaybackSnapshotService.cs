using API.Responses.Playback;

namespace API.Snapshots.Playback.Interface;

public interface IPlaybackSnapshotService
{
    Task<PlaybackStatusResponse> GetSnapshotAsync(
        ulong guildId,
        ulong? userId = null,
        CancellationToken cancellationToken = default);
}