using System.Net;
using System.Text.Json;
using API.Responses.MusicSearch;
using DC_bot.Music.Search;

namespace API.Services.MusicSearch;

/// <summary>
/// Validates Lavalink search payloads and maps provider-safe URLs and metadata to music results.
/// </summary>
public static class MusicSearchResultMapper
{

    /// <summary>
    /// Validates provider track or playlist results and returns deduplicated, bounded search items.
    /// </summary>
    /// <param name="root">The root value for this operation.</param>
    /// <param name="provider">The injected provider dependency.</param>
    /// <param name="kind">The kind value for this operation.</param>
    /// <param name="canEnqueue">The can enqueue value for this operation.</param>
    /// <param name="maxResults">The max results value for this operation.</param>
    /// <returns>The computed map result.</returns>
    public static MusicSearchResponse Map(
        JsonElement root,
        MusicSearchProvider provider,
        string kind,
        bool canEnqueue,
        int maxResults)
    {
        JsonElement entries;

        if (kind == "playlist")
        {
            entries = Property(root, "playlists");
        }
        else
        {
            var loadType = Text(root, "loadType");

            if (loadType == "empty")
            {
                return new MusicSearchResponse([]);
            }

            if (loadType == "error")
            {
                throw new MusicSearchException("ProviderFailure", "The search provider could not load results.");
            }

            if (loadType != "search")
            {
                throw InvalidResponse();
            }

            entries = Property(root, "data");
        }

        if (entries.ValueKind != JsonValueKind.Array)
        {
            throw InvalidResponse();
        }

        var results = new List<MusicSearchResultResponse>();
        var seen = new HashSet<string>(StringComparer.Ordinal);

        foreach (var entry in entries.EnumerateArray())
        {
            var info = Property(entry, "info");

            if (info.ValueKind != JsonValueKind.Object)
            {
                throw InvalidResponse();
            }

            var playlist = kind == "playlist";
            var metadata = playlist ? Property(entry, "pluginInfo") : info;
            var url = Text(metadata, "url") ?? Text(metadata, "uri");
            var canonicalUrl = MusicSearchProviders.IsProviderUrl(provider.Id, url) ? url : null;
            var id = Text(metadata, "identifier") ?? canonicalUrl;

            if (id is null || !seen.Add(id))
            {
                continue;
            }

            var length = Number(info, "length");
            var count = Number(metadata, "totalTracks");
            var isStream = Property(info, "isStream").ValueKind == JsonValueKind.True;
            var duration = !playlist && !isStream && length is >= 0 && length / 1000 <= int.MaxValue ? (int? )(length / 1000) : null;
            var enqueue = canEnqueue && canonicalUrl is not null;
            results.Add(new MusicSearchResultResponse(id, provider.Id, kind, Text(info, playlist ? "name" : "title") ?? "Untitled", Text(metadata, "author"), Artwork(Text(metadata, "artworkUrl")), duration, playlist && count is >= 0 and <= int.MaxValue ? (int? )count : null, canonicalUrl, enqueue, enqueue ? null : canonicalUrl is null ? "CanonicalUrlUnavailable" : "EnqueueUnavailable"));

            if (results.Count >= maxResults)
            {
                break;
            }
        }

        return new MusicSearchResponse(results);
    }

    /// <summary>
    /// Accepts only HTTPS artwork URLs without local hosts, IP literals, credentials, or custom ports.
    /// </summary>
    /// <param name="value">The value value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private static string? Artwork(string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri)
            || uri.Scheme != "https"
            || !uri.IsDefaultPort
            || !string.IsNullOrEmpty(uri.UserInfo)
            || uri.IsLoopback
            || IPAddress.TryParse(uri.Host, out _)
            || !uri.Host.Contains('.')
            || uri.Host.EndsWith(".local", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return value;
    }

    /// <summary>
    /// Reads a JSON object property or returns an undefined JSON value when it is absent.
    /// </summary>
    /// <param name="element">The element value for this operation.</param>
    /// <param name="name">The name value for this operation.</param>
    /// <returns>The computed property result.</returns>
    internal static JsonElement Property(JsonElement element, string name) => element.ValueKind == JsonValueKind.Object && element.TryGetProperty(name, out var value) ? value : default;

    /// <summary>
    /// Reads a nonempty JSON string property or returns null.
    /// </summary>
    /// <param name="element">The element value for this operation.</param>
    /// <param name="name">The name value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private static string? Text(JsonElement element, string name)
    {
        var value = Property(element, name);
        return value.ValueKind == JsonValueKind.String && !string.IsNullOrWhiteSpace(value.GetString()) ? value.GetString() : null;
    }

    /// <summary>
    /// Reads an integer JSON property representable as a signed 64-bit value or returns null.
    /// </summary>
    /// <param name="element">The element value for this operation.</param>
    /// <param name="name">The name value for this operation.</param>
    /// <returns>The converted value, or null when the input is absent or cannot be represented.</returns>
    private static long? Number(JsonElement element, string name)
    {
        var value = Property(element, name);
        return value.ValueKind == JsonValueKind.Number && value.TryGetInt64(out var number) ? number : null;
    }

    /// <summary>
    /// Creates the safe invalid-provider-response error used for malformed search payloads.
    /// </summary>
    /// <returns>The computed invalid response result.</returns>
    private static MusicSearchException InvalidResponse() => new("InvalidProviderResponse", "The search provider returned invalid data.");
}
