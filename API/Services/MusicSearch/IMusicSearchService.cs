using API.Responses.MusicSearch;

namespace API.Services.MusicSearch;

public interface IMusicSearchService
{
    Task<IReadOnlyList<MusicSearchCapabilityResponse>> GetCapabilitiesAsync(CancellationToken cancellationToken);
    Task<MusicSearchResponse> SearchAsync(string? provider, string? kind, string? query, string? pageToken, CancellationToken cancellationToken);
}
