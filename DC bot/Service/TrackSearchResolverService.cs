using DC_bot.Helper;
using DC_bot.Interface;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;

namespace DC_bot.Service;

public sealed class TrackSearchResolverService(IOptions<SearchResolverOptions> options) : ITrackSearchResolverService
{
    public TrackSearchMode ResolveSearchMode(string input)
    {
        if (!Uri.TryCreate(input, UriKind.Absolute, out var uri))
            return options.Value.DefaultQueryMode switch
            {
                "ytm" => TrackSearchMode.YouTubeMusic,
                "sc" => TrackSearchMode.SoundCloud,
                "sp" => TrackSearchMode.Spotify,
                _ => TrackSearchMode.YouTube,
            };
        
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
            _ => TrackSearchMode.None,
        };
    }
}