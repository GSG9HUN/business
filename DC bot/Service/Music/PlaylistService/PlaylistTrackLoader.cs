using DC_bot.Exceptions.Music;
using DC_bot.Logging;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

/// <summary>
/// Loads source tracks through Lavalink and translates source failures into playlist load errors.
/// </summary>
/// <param name="audioService">The audio service dependency used by this boundary.</param>
/// <param name="logger">The structured logger used at this operation boundary.</param>
internal sealed class PlaylistTrackLoader(IAudioService audioService, ILogger<PlaylistService> logger)
{

    /// <summary>
    /// Loads source tracks through Lavalink and translates source-loading failures to track-load errors.
    /// </summary>
    /// <param name="trackReference">The track reference supplied for this operation.</param>
    /// <param name="searchMode">The search mode supplied for this operation.</param>
    /// <param name="operationName">The operation name supplied for this operation.</param>
    /// <param name="cancellationToken">Cancels waiting and supported downstream I/O; owned cleanup completes independently.</param>
    /// <returns>The asynchronous operation result, including its status or value when completed.</returns>
    internal async Task<TrackLoadResult> LoadTracksAsync(
        string trackReference,
        TrackSearchMode searchMode,
        string operationName,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        try
        {
            return await audioService.Tracks.LoadTracksAsync(trackReference, searchMode, cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, operationName);
            throw new TrackLoadException(trackReference, "Failed to load playlist tracks", ex);
        }
    }
}
