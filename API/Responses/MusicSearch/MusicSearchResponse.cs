namespace API.Responses.MusicSearch;

/// <summary>
/// Carries the music search payload returned to HTTP and mobile clients.
/// </summary>
/// <param name="Results">The results value for this operation.</param>
/// <param name="NextPageToken">The next page token value for this operation.</param>
/// <param name="IsLimited">The is limited value for this operation.</param>
public sealed record MusicSearchResponse(
    IReadOnlyList<MusicSearchResultResponse> Results,
    string? NextPageToken = null,
    bool IsLimited = true);
