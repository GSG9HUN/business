namespace API.Responses.MusicSearch;

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
