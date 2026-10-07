using DC_bot.Interface.Service.Music;
using DC_bot.Music;
using Lavalink4NET.Rest.Entities.Tracks;

namespace DC_bot.Service.BotControl;

public sealed record MobilePlayInput(string Query, Uri? Url, TrackSearchMode SearchMode)
{
    private static readonly IReadOnlyDictionary<string, TrackSearchMode> Modes = new Dictionary<string, TrackSearchMode>(StringComparer.OrdinalIgnoreCase)
    {
        ["YouTube"] = TrackSearchMode.YouTube, ["YouTubeMusic"] = TrackSearchMode.YouTubeMusic,
        ["Spotify"] = TrackSearchMode.Spotify, ["SoundCloud"] = TrackSearchMode.SoundCloud,
        ["AppleMusic"] = TrackSearchMode.AppleMusic, ["Deezer"] = TrackSearchMode.Deezer,
        ["YandexMusic"] = TrackSearchMode.YandexMusic, ["Bandcamp"] = TrackSearchMode.Bandcamp,
        ["None"] = TrackSearchMode.None
    };

    public static bool TryParse(string input, string? searchMode, ITrackSearchResolverService resolver, out MobilePlayInput? result)
    {
        result = null;
        var query = input.Trim();
        if (query.Length is < 2 or > 2048 || query.Any(char.IsControl)) return false;
        var mode = resolver.ResolveSearchMode(query);
        if (!string.IsNullOrWhiteSpace(searchMode))
        {
            // Persisted clients may send the enum name or a known search prefix.
            var provider = MusicSearchProviders.Find(searchMode.Trim());
            if (provider is not null) mode = resolver.ResolveSearchMode(provider.Id + ":query");
            else if (!Modes.TryGetValue(searchMode.Trim(), out mode)) return false;
        }

        var separator = query.IndexOf(':');
        if (separator > 0)
        {
            var prefix = query[..separator].ToLowerInvariant();
            if (MusicSearchProviders.Find(prefix) is not null || prefix is "spotify" or "soundcloud" or "youtube" or "youtubemusic" or "applemusic" or "deezer" or "yandexmusic" or "bandcamp")
                query = query[(separator + 1)..].Trim();
        }
        if (query.Length < 2) return false;
        if (Uri.TryCreate(query, UriKind.Absolute, out var url))
        {
            if (!MusicSearchProviders.All.Any(p => MusicSearchProviders.IsProviderUrl(p.Id, query))) return false;
            // Concrete identities must be resolved directly, never fed back into keyword search.
            result = new MobilePlayInput(query, url, TrackSearchMode.None);
        }
        else
        {
            result = new MobilePlayInput(query, null, mode);
        }
        return true;
    }
}
