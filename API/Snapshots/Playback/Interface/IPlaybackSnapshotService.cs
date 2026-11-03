using API.Responses.Playback;

namespace API.Snapshots.Playback.Interface;

/// <summary>
/// Defines the shared guild playback snapshot query.
/// </summary>
public interface IPlaybackSnapshotService
{

    /// <summary>
    /// Returns a guild&apos;s assembled playback read model with bot presence and queue metadata.
    /// </summary>
    /// <param name="guildId">The guild id value for this operation.</param>
    /// <param name="userId">The user id value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get snapshot result.</returns>
    Task<PlaybackStatusResponse> GetSnapshotAsync(ulong guildId, ulong? userId = null, CancellationToken cancellationToken = default);
}
