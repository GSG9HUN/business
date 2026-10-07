using API.Responses.MusicSearch;
using DC_bot.Music;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace API.Services.MusicSearch;

public sealed class MusicSearchService(
    LavalinkSearchClient client,
    IOptions<MusicSearchOptions> options,
    IMemoryCache cache,
    MusicSearchQuota quota) : IMusicSearchService
{
    public async Task<IReadOnlyList<MusicSearchCapabilityResponse>> GetCapabilitiesAsync(
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(options.Value.Password))
            return MusicSearchProviders.All
                .Select(p => new MusicSearchCapabilityResponse(p.Id, false, false, false, "SearchNotConfigured"))
                .ToArray();

        var info = await GetNodeInfoAsync(cancellationToken);
        return MusicSearchProviders.All.Select(provider =>
        {
            var settings = Settings(provider.Id);
            var enabled = settings.Enabled && info.Sources.Contains(provider.SourceName);
            var reason = !settings.Enabled ? settings.UnavailableReason ?? "ProviderDisabled"
                : !enabled ? "SourceUnavailable"
                : !settings.CanEnqueue ? "EnqueueUnavailable" : null;
            return new MusicSearchCapabilityResponse(provider.Id, enabled,
                enabled && settings.PlaylistSearch && provider.SupportsPlaylistSearch && info.HasLavaSearch,
                enabled && settings.CanEnqueue, reason);
        }).ToArray();
    }

    public async Task<MusicSearchResponse> SearchAsync(string? provider, string? kind, string? query, string? pageToken,
        CancellationToken cancellationToken)
    {
        var source = MusicSearchProviders.Find(provider?.Trim())
                     ?? throw new MusicSearchException("InvalidProvider", "Unknown search provider.", 400);
        kind = kind?.Trim().ToLowerInvariant();
        if (kind is not ("track" or "playlist"))
            throw new MusicSearchException("InvalidKind", "Kind must be track or playlist.", 400);
        query = query?.Trim();
        if (query is null || query.Length is < 2 or > 200 || query.Any(char.IsControl))
            throw new MusicSearchException("InvalidQuery",
                "Query must contain 2 to 200 characters without control characters.", 400);
        if (pageToken is not null)
            throw new MusicSearchException("InvalidPageToken",
                "This search provider does not support continuation tokens.", 400);
        if (!Settings(source.Id).Enabled)
            throw new MusicSearchException("ProviderDisabled", "This search provider is disabled.", 422);

        quota.Acquire(source.Id);

        var capability = (await GetCapabilitiesAsync(cancellationToken)).Single(p => p.ProviderId == source.Id);
        if (!capability.SupportsTrackSearch)
            throw new MusicSearchException("ProviderUnavailable", "This search provider is unavailable.", 503);
        if (kind == "playlist" && !capability.SupportsPlaylistSearch)
            throw new MusicSearchException("UnsupportedKind",
                "Playlist search is not enabled for this provider. Use a playlist URL instead.", 422);

        var identifier = Uri.EscapeDataString(source.Prefix + ":" + query);
        var path = kind == "playlist"
            ? $"v4/loadsearch?query={identifier}&types=playlist"
            : $"v4/loadtracks?identifier={identifier}";
        using var document = await client.GetAsync(path, cancellationToken);
        return document is null
            ? new MusicSearchResponse([])
            : MusicSearchResultMapper.Map(document.RootElement, source, kind, capability.CanEnqueue,
                options.Value.MaxResults);
    }

    private MusicSearchProviderOptions Settings(string id) =>
        options.Value.Providers.GetValueOrDefault(id) ?? new MusicSearchProviderOptions();

    private async Task<NodeInfo> GetNodeInfoAsync(CancellationToken cancellationToken)
    {
        var key = (typeof(MusicSearchService), options.Value.BaseAddress);
        if (cache.TryGetValue<NodeInfo>(key, out var cached)) return cached!;
        quota.Acquire("node-info");
        using var document = await client.GetAsync("v4/info", cancellationToken);
        if (document is null)
            throw new MusicSearchException("InvalidProviderResponse", "Missing search service information.");
        var sources = MusicSearchResultMapper.Property(document.RootElement, "sourceManagers");
        var plugins = MusicSearchResultMapper.Property(document.RootElement, "plugins");
        if (sources.ValueKind != JsonValueKind.Array || plugins.ValueKind != JsonValueKind.Array ||
            sources.EnumerateArray().Any(s => s.ValueKind != JsonValueKind.String))
            throw new MusicSearchException("InvalidProviderResponse", "Invalid search service information.");
        var info = new NodeInfo(
            sources.EnumerateArray().Select(s => s.GetString()!).ToHashSet(StringComparer.OrdinalIgnoreCase),
            plugins.EnumerateArray().Any(p =>
                MusicSearchResultMapper.Property(p, "name").ToString()
                    .Equals("lavasearch-plugin", StringComparison.OrdinalIgnoreCase)));
        cache.Set(key, info, TimeSpan.FromSeconds(30));
        return info;
    }

    private sealed record NodeInfo(HashSet<string> Sources, bool HasLavaSearch);
}