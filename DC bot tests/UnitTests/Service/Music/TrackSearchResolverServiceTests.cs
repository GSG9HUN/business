using DC_bot.Configuration;
using DC_bot.Service.Music;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class TrackSearchResolverServiceTests
{
    private readonly TrackSearchResolverService _resolverService;

    public TrackSearchResolverServiceTests()
    {
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(new SearchResolverOptions { DefaultQueryMode = "youtube" });
        _resolverService = new TrackSearchResolverService(mockOptions.Object);
    }

    #region Invalid URL Tests

    [Fact]
    public void ResolveSearchMode_UnknownUrl_ReturnsNone()
    {
        var result1 = _resolverService.ResolveSearchMode("https://unknown-domain.com/song");
        var result2 = _resolverService.ResolveSearchMode("https://example.com/audio");

        Assert.Equal(TrackSearchMode.None, result1);
        Assert.Equal(TrackSearchMode.None, result2);
    }

    #endregion

    #region Prefix Resolution Tests

    [Fact]
    public void ResolveSearchMode_SpotifyPrefix_ReturnsSpotifyMode()
    {
        var result1 = _resolverService.ResolveSearchMode("spotify:test");
        var result2 = _resolverService.ResolveSearchMode("sptfy:test");

        Assert.Equal(TrackSearchMode.Spotify, result1);
        Assert.Equal(TrackSearchMode.Spotify, result2);
    }

    [Fact]
    public void ResolveSearchMode_SoundCloudPrefix_ReturnsSoundCloudMode()
    {
        var result1 = _resolverService.ResolveSearchMode("soundcloud:test");
        var result2 = _resolverService.ResolveSearchMode("scsearch:test");

        Assert.Equal(TrackSearchMode.SoundCloud, result1);
        Assert.Equal(TrackSearchMode.SoundCloud, result2);
    }

    [Fact]
    public void ResolveSearchMode_YouTubePrefix_ReturnsYouTubeMode()
    {
        var result1 = _resolverService.ResolveSearchMode("youtube:test");
        var result2 = _resolverService.ResolveSearchMode("ytsearch:test");

        Assert.Equal(TrackSearchMode.YouTube, result1);
        Assert.Equal(TrackSearchMode.YouTube, result2);
    }

    [Fact]
    public void ResolveSearchMode_YouTubeMusicPrefix_ReturnsYouTubeMusicMode()
    {
        var result1 = _resolverService.ResolveSearchMode("youtubemusic:test");
        var result2 = _resolverService.ResolveSearchMode("ytmsearch:test");

        Assert.Equal(TrackSearchMode.YouTubeMusic, result1);
        Assert.Equal(TrackSearchMode.YouTubeMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_AppleMusicPrefix_ReturnsAppleMusicMode()
    {
        var result1 = _resolverService.ResolveSearchMode("applemusic:test");
        var result2 = _resolverService.ResolveSearchMode("amsearch:test");

        Assert.Equal(TrackSearchMode.AppleMusic, result1);
        Assert.Equal(TrackSearchMode.AppleMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_DeezerPrefix_ReturnsDeezerMode()
    {
        var result1 = _resolverService.ResolveSearchMode("deezer:test");
        var result2 = _resolverService.ResolveSearchMode("dzsearch:test");

        Assert.Equal(TrackSearchMode.Deezer, result1);
        Assert.Equal(TrackSearchMode.Deezer, result2);
    }

    [Fact]
    public void ResolveSearchMode_YandexMusicPrefix_ReturnsYandexMusicMode()
    {
        var result1 = _resolverService.ResolveSearchMode("yandexmusic:test");
        var result2 = _resolverService.ResolveSearchMode("ymsearch:test");

        Assert.Equal(TrackSearchMode.YandexMusic, result1);
        Assert.Equal(TrackSearchMode.YandexMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_BandcampPrefix_ReturnsBandcampMode()
    {
        var result1 = _resolverService.ResolveSearchMode("bandcamp:test");
        var result2 = _resolverService.ResolveSearchMode("bcsearch:test");

        Assert.Equal(TrackSearchMode.Bandcamp, result1);
        Assert.Equal(TrackSearchMode.Bandcamp, result2);
    }

    #endregion

    #region URL Resolution Tests

    [Fact]
    public void ResolveSearchMode_YouTubeUrl_ReturnsYouTubeMode()
    {
        var result1 = _resolverService.ResolveSearchMode("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var result2 = _resolverService.ResolveSearchMode("https://youtube.com/watch?v=test");
        var result3 = _resolverService.ResolveSearchMode("https://youtu.be/test");
        var result4 = _resolverService.ResolveSearchMode("https://m.youtube.com/watch?v=test");

        Assert.Equal(TrackSearchMode.YouTube, result1);
        Assert.Equal(TrackSearchMode.YouTube, result2);
        Assert.Equal(TrackSearchMode.YouTube, result3);
        Assert.Equal(TrackSearchMode.YouTube, result4);
    }

    [Fact]
    public void ResolveSearchMode_YouTubeMusicUrl_ReturnsYouTubeMusicMode()
    {
        var result = _resolverService.ResolveSearchMode("https://music.youtube.com/browse/VLPlaylist");

        Assert.Equal(TrackSearchMode.YouTubeMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_SoundCloudUrl_ReturnsSoundCloudMode()
    {
        var result1 = _resolverService.ResolveSearchMode("https://soundcloud.com/artist/track");
        var result2 = _resolverService.ResolveSearchMode("https://m.soundcloud.com/artist/track");

        Assert.Equal(TrackSearchMode.SoundCloud, result1);
        Assert.Equal(TrackSearchMode.SoundCloud, result2);
    }

    [Fact]
    public void ResolveSearchMode_SpotifyUrl_ReturnsSpotifyMode()
    {
        var result = _resolverService.ResolveSearchMode("https://open.spotify.com/track/123");

        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    [Fact]
    public void ResolveSearchMode_AppleMusicUrl_ReturnsAppleMusicMode()
    {
        var result = _resolverService.ResolveSearchMode("https://music.apple.com/us/album/123");

        Assert.Equal(TrackSearchMode.AppleMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_DeezerUrl_ReturnsDeezerMode()
    {
        var result1 = _resolverService.ResolveSearchMode("https://www.deezer.com/track/123");
        var result2 = _resolverService.ResolveSearchMode("https://deezer.com/track/123");

        Assert.Equal(TrackSearchMode.Deezer, result1);
        Assert.Equal(TrackSearchMode.Deezer, result2);
    }

    [Fact]
    public void ResolveSearchMode_YandexMusicUrl_ReturnsYandexMusicMode()
    {
        var result1 = _resolverService.ResolveSearchMode("https://music.yandex.ru/album/123");
        var result2 = _resolverService.ResolveSearchMode("https://yandex.ru/music/album/123");

        Assert.Equal(TrackSearchMode.YandexMusic, result1);
        Assert.Equal(TrackSearchMode.YandexMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_BandcampUrl_ReturnsBandcampMode()
    {
        var result = _resolverService.ResolveSearchMode("https://bandcamp.com/track/test");

        Assert.Equal(TrackSearchMode.Bandcamp, result);
    }

    #endregion

    #region Query Resolution Tests

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_UsesDefaultMode()
    {
        var options = new SearchResolverOptions { DefaultQueryMode = "youtube" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        var result = service.ResolveSearchMode("never gonna give you up");

        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultYouTubeMusic()
    {
        var options = new SearchResolverOptions { DefaultQueryMode = "ytm" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        var result = service.ResolveSearchMode("some song");

        Assert.Equal(TrackSearchMode.YouTubeMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultSoundCloud()
    {
        var options = new SearchResolverOptions { DefaultQueryMode = "sc" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        var result = service.ResolveSearchMode("audio track");

        Assert.Equal(TrackSearchMode.SoundCloud, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultSpotify()
    {
        var options = new SearchResolverOptions { DefaultQueryMode = "sp" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        var result = service.ResolveSearchMode("another track");

        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ResolveSearchMode_EmptyString_UsesDefaultMode()
    {
        var result = _resolverService.ResolveSearchMode("");

        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_WhitespaceOnly_UsesDefaultMode()
    {
        var result = _resolverService.ResolveSearchMode("   ");

        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_MixedCasePrefix_ResolvesCorrectly()
    {
        var result = _resolverService.ResolveSearchMode("SPOTIFY:test");

        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    [Fact]
    public void ResolveSearchMode_MultipleColons_UsesFirstPrefix()
    {
        var result = _resolverService.ResolveSearchMode("youtube:test:extra");

        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    #endregion
}
