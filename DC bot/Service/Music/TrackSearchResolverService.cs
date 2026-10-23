using DC_bot.Configuration;
using DC_bot.Interface.Service.Music.Search;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace DC_bot.Service.Music;

/// <summary>
/// Selects a search provider from a prefix, URL, or configured default.
/// </summary>
/// <param name="options">The configured typed options consumed by this boundary.</param>
public sealed class TrackSearchResolverService(IOptions<SearchResolverOptions> options) : ITrackSearchResolverService
{

    /// <summary>
    /// Resolves an explicit provider prefix or URL, falling back to the configured default.
    /// </summary>
    /// <param name="input">The input supplied for this operation.</param>
    /// <returns>The value produced for the supplied operation context.</returns>
    public TrackSearchMode ResolveSearchMode(string input)
    {
        if (input.Contains(':'))
        {
            var prefix = input.Split(':')[0].ToLowerInvariant();
            switch (prefix)
            {
                case "sptfy" or "spotify":
                    return TrackSearchMode.Spotify;
                case "scsearch" or "soundcloud":
                    return TrackSearchMode.SoundCloud;
                case "ytmsearch" or "youtubemusic":
                    return TrackSearchMode.YouTubeMusic;
                case "ytsearch" or "youtube":
                    return TrackSearchMode.YouTube;
                case "amsearch" or "applemusic":
                    return TrackSearchMode.AppleMusic;
                case "dzsearch" or "deezer":
                    return TrackSearchMode.Deezer;
                case "ymsearch" or "yandexmusic":
                    return TrackSearchMode.YandexMusic;
                case "bcsearch" or "bandcamp":
                    return TrackSearchMode.Bandcamp;
            }
        }

        if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
        {
            return options.Value.DefaultQueryMode switch
            {
                "ytm" => TrackSearchMode.YouTubeMusic,
                "sc" => TrackSearchMode.SoundCloud,
                "sp" => TrackSearchMode.Spotify,
                _ => TrackSearchMode.YouTube
            };
        }

        var host = uri.Host.ToLowerInvariant();
        return host switch
        {
            "www.youtube.com" or "youtube.com" or "m.youtube.com" or "youtu.be" => TrackSearchMode.YouTube,
            "music.youtube.com" => TrackSearchMode.YouTubeMusic,
            "soundcloud.com" or "m.soundcloud.com" => TrackSearchMode.SoundCloud,
            "open.spotify.com" => TrackSearchMode.Spotify,
            "music.apple.com" => TrackSearchMode.AppleMusic,
            "www.deezer.com" or "deezer.com" => TrackSearchMode.Deezer,
            "music.yandex.ru" or "yandex.ru" => TrackSearchMode.YandexMusic,
            "bandcamp.com" => TrackSearchMode.Bandcamp,
            _ => TrackSearchMode.None
        };
    }
}
