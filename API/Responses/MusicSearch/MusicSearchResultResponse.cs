namespace API.Responses.MusicSearch;

/// <summary>
/// Carries the music search result payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="Id">The id value for this operation.</param>
/// <param name="ProviderId">The provider id value for this operation.</param>
/// <param name="Kind">The kind value for this operation.</param>
/// <param name="Title">The title value for this operation.</param>
/// <param name="Creator">The creator value for this operation.</param>
/// <param name="ThumbnailUrl">The thumbnail url value for this operation.</param>
/// <param name="DurationSeconds">The duration seconds value for this operation.</param>
/// <param name="ItemCount">The item count value for this operation.</param>
/// <param name="CanonicalUrl">The canonical url value for this operation.</param>
/// <param name="CanEnqueue">The can enqueue value for this operation.</param>
/// <param name="UnavailableReason">The unavailable reason value for this operation.</param>
public sealed record MusicSearchResultResponse(
    string Id,
    string ProviderId,
    string Kind,
    string Title,
    string? Creator,
    string? ThumbnailUrl,
    int? DurationSeconds,
    int? ItemCount,
    string? CanonicalUrl,
    bool CanEnqueue,
    string? UnavailableReason = null);
