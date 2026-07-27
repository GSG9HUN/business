using DC_bot.Exceptions.Music;
using DC_bot.Logging;
using Lavalink4NET;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Logging;

namespace DC_bot.Service.Music.PlaylistService;

internal sealed class PlaylistTrackLoader(
    IAudioService audioService,
    ILogger<PlaylistService> logger)
{
    internal async Task<TrackLoadResult> LoadTracksAsync(
        string trackReference,
        TrackSearchMode searchMode,
        string operationName)
    {
        try
        {
            return await audioService.Tracks.LoadTracksAsync(trackReference, searchMode).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            logger.LavalinkOperationFailed(ex, operationName);
            throw new TrackLoadException(trackReference, "Failed to load playlist tracks", ex);
        }
    }
}
