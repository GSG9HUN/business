namespace API.Responses.MusicSearch;

/// <summary>
/// Carries the music search capability payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="ProviderId">The provider id value for this operation.</param>
/// <param name="SupportsTrackSearch">The supports track search value for this operation.</param>
/// <param name="SupportsPlaylistSearch">The supports playlist search value for this operation.</param>
/// <param name="CanEnqueue">The can enqueue value for this operation.</param>
/// <param name="UnavailableReason">The unavailable reason value for this operation.</param>
/// <param name="SupportsPagination">The supports pagination value for this operation.</param>
public sealed record MusicSearchCapabilityResponse(
    string ProviderId,
    bool SupportsTrackSearch,
    bool SupportsPlaylistSearch,
    bool CanEnqueue,
    string? UnavailableReason,
    bool SupportsPagination = false);
