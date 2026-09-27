namespace DC_bot.Music;

public sealed record MusicSearchProvider(string Id, string Prefix, string SourceName, bool SupportsPlaylistSearch);

public static class MusicSearchProviders
{
    public static IReadOnlyList<MusicSearchProvider> All { get; } = Array.AsReadOnly(new[]
    {
        new MusicSearchProvider("ytsearch", "ytsearch", "youtube", true),
        new MusicSearchProvider("ytmsearch", "ytmsearch", "youtube", true),
        new MusicSearchProvider("sptfy", "spsearch", "spotify", true),
        new MusicSearchProvider("scsearch", "scsearch", "soundcloud", false),
        new MusicSearchProvider("amsearch", "amsearch", "applemusic", true),
        new MusicSearchProvider("dzsearch", "dzsearch", "deezer", true),
        new MusicSearchProvider("ymsearch", "ymsearch", "yandexmusic", false),
        new MusicSearchProvider("bcsearch", "bcsearch", "bandcamp", false)
    });

    public static MusicSearchProvider? Find(string? id) =>
        All.FirstOrDefault(provider => string.Equals(provider.Id, id, StringComparison.OrdinalIgnoreCase));

    public static bool IsProviderUrl(string providerId, string? value)
    {
        if (!Uri.TryCreate(value, UriKind.Absolute, out var uri) ||
            uri.Scheme is not ("https" or "http") || !uri.IsDefaultPort ||
            !string.IsNullOrEmpty(uri.UserInfo)) return false;

        var host = uri.IdnHost.ToLowerInvariant();
        var allowedHost = providerId switch
        {
            "ytsearch" or "ytmsearch" => host is "youtube.com" or "www.youtube.com" or "m.youtube.com" or "music.youtube.com" or "youtu.be",
            "sptfy" => host == "open.spotify.com",
            "scsearch" => host is "soundcloud.com" or "www.soundcloud.com" or "m.soundcloud.com",
            "amsearch" => host == "music.apple.com",
            "dzsearch" => host is "deezer.com" or "www.deezer.com",
            "ymsearch" => host is "music.yandex.ru" or "music.yandex.com",
            "bcsearch" => host == "bandcamp.com" || host.EndsWith(".bandcamp.com", StringComparison.Ordinal),
            _ => false
        };
        if (!allowedHost) return false;
        // Only provider content routes are enqueueable; redirect/API endpoints are not identities.
        var segments = uri.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        return providerId switch
        {
            "ytsearch" or "ytmsearch" => host == "youtu.be" ? segments.Length == 1 :
                (uri.AbsolutePath == "/watch" && HasQueryValue(uri, "v")) ||
                (uri.AbsolutePath == "/playlist" && HasQueryValue(uri, "list")) ||
                (segments.Length == 2 && segments[0] is "shorts" or "live"),
            "sptfy" => segments.Length >= 2 &&
                (segments[0] is "track" or "playlist" or "album" or "artist" ||
                 segments.Length == 3 && segments[0].StartsWith("intl-", StringComparison.Ordinal) && segments[1] is "track" or "playlist" or "album" or "artist"),
            "scsearch" => (segments.Length == 2 || segments.Length == 3 && segments[1] == "sets") &&
                segments[0] is not ("redirect" or "oauth" or "connect"),
            "amsearch" => segments.Length >= 3 && segments[1] is "song" or "album" or "playlist" or "artist",
            "dzsearch" => segments.Length >= 2 && segments.Take(2).Any(s => s is "track" or "album" or "playlist" or "artist"),
            "ymsearch" => segments.Length >= 2 && segments[0] is "track" or "album" or "playlists" or "users" or "artist",
            "bcsearch" => segments.Length == 2 && segments[0] is "track" or "album",
            _ => false
        };
    }

    private static bool HasQueryValue(Uri uri, string key) => uri.Query.TrimStart('?').Split('&')
        .Any(part => part.StartsWith(key + "=", StringComparison.Ordinal) && part.Length > key.Length + 1);
}
