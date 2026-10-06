namespace API.Responses.MusicSearch;

public sealed record MusicSearchCapabilityResponse(
    string ProviderId,
    bool SupportsTrackSearch,
    bool SupportsPlaylistSearch,
    bool CanEnqueue,
    string? UnavailableReason,
    bool SupportsPagination = false);