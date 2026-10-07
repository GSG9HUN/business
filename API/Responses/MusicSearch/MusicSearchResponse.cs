namespace API.Responses.MusicSearch;

public sealed record MusicSearchResponse(
    IReadOnlyList<MusicSearchResultResponse> Results,
    string? NextPageToken = null,
    bool IsLimited = true);


