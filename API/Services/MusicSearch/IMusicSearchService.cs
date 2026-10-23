using API.Responses.MusicSearch;

namespace API.Services.MusicSearch;

/// <summary>
/// Defines provider capability discovery and validated track or playlist searches.
/// </summary>
public interface IMusicSearchService
{

    /// <summary>
    /// Returns deployed provider search and enqueue capabilities based on the Lavalink node configuration.
    /// </summary>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed get capabilities result.</returns>
    Task<IReadOnlyList<MusicSearchCapabilityResponse>> GetCapabilitiesAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Validates the music query and requests matching results from an enabled provider.
    /// </summary>
    /// <param name="provider">The provider value for this operation.</param>
    /// <param name="kind">The kind value for this operation.</param>
    /// <param name="query">The query value for this operation.</param>
    /// <param name="pageToken">The page token value for this operation.</param>
    /// <param name="cancellationToken">Cancels the underlying I/O and waiting operations.</param>
    /// <returns>The computed search result.</returns>
    Task<MusicSearchResponse> SearchAsync(
        string? provider,
        string? kind,
        string? query,
        string? pageToken,
        CancellationToken cancellationToken);
}
