using DC_bot.Configuration;
using DC_bot.Service.Music;
using Lavalink4NET.Rest.Entities.Tracks;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

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
        // Act
        var result1 = _resolverService.ResolveSearchMode("https://unknown-domain.com/song");
        var result2 = _resolverService.ResolveSearchMode("https://example.com/audio");

        // Assert
        Assert.Equal(TrackSearchMode.None, result1);
        Assert.Equal(TrackSearchMode.None, result2);
    }

    #endregion

    #region Prefix Resolution Tests

    [Fact]
    public void ResolveSearchMode_SpotifyPrefix_ReturnsSpotifyMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("spotify:test");
        var result2 = _resolverService.ResolveSearchMode("sptfy:test");

        // Assert
        Assert.Equal(TrackSearchMode.Spotify, result1);
        Assert.Equal(TrackSearchMode.Spotify, result2);
    }

    [Fact]
    public void ResolveSearchMode_SoundCloudPrefix_ReturnsSoundCloudMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("soundcloud:test");
        var result2 = _resolverService.ResolveSearchMode("scsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.SoundCloud, result1);
        Assert.Equal(TrackSearchMode.SoundCloud, result2);
    }

    [Fact]
    public void ResolveSearchMode_YouTubePrefix_ReturnsYouTubeMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("youtube:test");
        var result2 = _resolverService.ResolveSearchMode("ytsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result1);
        Assert.Equal(TrackSearchMode.YouTube, result2);
    }

    [Fact]
    public void ResolveSearchMode_YouTubeMusicPrefix_ReturnsYouTubeMusicMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("youtubemusic:test");
        var result2 = _resolverService.ResolveSearchMode("ytmsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.YouTubeMusic, result1);
        Assert.Equal(TrackSearchMode.YouTubeMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_AppleMusicPrefix_ReturnsAppleMusicMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("applemusic:test");
        var result2 = _resolverService.ResolveSearchMode("amsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.AppleMusic, result1);
        Assert.Equal(TrackSearchMode.AppleMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_DeezerPrefix_ReturnsDeezerMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("deezer:test");
        var result2 = _resolverService.ResolveSearchMode("dzsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.Deezer, result1);
        Assert.Equal(TrackSearchMode.Deezer, result2);
    }

    [Fact]
    public void ResolveSearchMode_YandexMusicPrefix_ReturnsYandexMusicMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("yandexmusic:test");
        var result2 = _resolverService.ResolveSearchMode("ymsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.YandexMusic, result1);
        Assert.Equal(TrackSearchMode.YandexMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_BandcampPrefix_ReturnsBandcampMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("bandcamp:test");
        var result2 = _resolverService.ResolveSearchMode("bcsearch:test");

        // Assert
        Assert.Equal(TrackSearchMode.Bandcamp, result1);
        Assert.Equal(TrackSearchMode.Bandcamp, result2);
    }

    #endregion

    #region URL Resolution Tests

    [Fact]
    public void ResolveSearchMode_YouTubeUrl_ReturnsYouTubeMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("https://www.youtube.com/watch?v=dQw4w9WgXcQ");
        var result2 = _resolverService.ResolveSearchMode("https://youtube.com/watch?v=test");
        var result3 = _resolverService.ResolveSearchMode("https://youtu.be/test");
        var result4 = _resolverService.ResolveSearchMode("https://m.youtube.com/watch?v=test");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result1);
        Assert.Equal(TrackSearchMode.YouTube, result2);
        Assert.Equal(TrackSearchMode.YouTube, result3);
        Assert.Equal(TrackSearchMode.YouTube, result4);
    }

    [Fact]
    public void ResolveSearchMode_YouTubeMusicUrl_ReturnsYouTubeMusicMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("https://music.youtube.com/browse/VLPlaylist");

        // Assert
        Assert.Equal(TrackSearchMode.YouTubeMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_SoundCloudUrl_ReturnsSoundCloudMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("https://soundcloud.com/artist/track");
        var result2 = _resolverService.ResolveSearchMode("https://m.soundcloud.com/artist/track");

        // Assert
        Assert.Equal(TrackSearchMode.SoundCloud, result1);
        Assert.Equal(TrackSearchMode.SoundCloud, result2);
    }

    [Fact]
    public void ResolveSearchMode_SpotifyUrl_ReturnsSpotifyMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("https://open.spotify.com/track/123");

        // Assert
        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    [Fact]
    public void ResolveSearchMode_AppleMusicUrl_ReturnsAppleMusicMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("https://music.apple.com/us/album/123");

        // Assert
        Assert.Equal(TrackSearchMode.AppleMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_DeezerUrl_ReturnsDeezerMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("https://www.deezer.com/track/123");
        var result2 = _resolverService.ResolveSearchMode("https://deezer.com/track/123");

        // Assert
        Assert.Equal(TrackSearchMode.Deezer, result1);
        Assert.Equal(TrackSearchMode.Deezer, result2);
    }

    [Fact]
    public void ResolveSearchMode_YandexMusicUrl_ReturnsYandexMusicMode()
    {
        // Act
        var result1 = _resolverService.ResolveSearchMode("https://music.yandex.ru/album/123");
        var result2 = _resolverService.ResolveSearchMode("https://yandex.ru/music/album/123");

        // Assert
        Assert.Equal(TrackSearchMode.YandexMusic, result1);
        Assert.Equal(TrackSearchMode.YandexMusic, result2);
    }

    [Fact]
    public void ResolveSearchMode_BandcampUrl_ReturnsBandcampMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("https://bandcamp.com/track/test");

        // Assert
        Assert.Equal(TrackSearchMode.Bandcamp, result);
    }

    #endregion

    #region Query Resolution Tests

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_UsesDefaultMode()
    {
        // Arrange
        var options = new SearchResolverOptions { DefaultQueryMode = "youtube" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        // Act
        var result = service.ResolveSearchMode("never gonna give you up");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultYouTubeMusic()
    {
        // Arrange
        var options = new SearchResolverOptions { DefaultQueryMode = "ytm" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        // Act
        var result = service.ResolveSearchMode("some song");

        // Assert
        Assert.Equal(TrackSearchMode.YouTubeMusic, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultSoundCloud()
    {
        // Arrange
        var options = new SearchResolverOptions { DefaultQueryMode = "sc" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        // Act
        var result = service.ResolveSearchMode("audio track");

        // Assert
        Assert.Equal(TrackSearchMode.SoundCloud, result);
    }

    [Fact]
    public void ResolveSearchMode_PlainTextQuery_WithDefaultSpotify()
    {
        // Arrange
        var options = new SearchResolverOptions { DefaultQueryMode = "sp" };
        var mockOptions = new Mock<IOptions<SearchResolverOptions>>();
        mockOptions.Setup(x => x.Value).Returns(options);
        var service = new TrackSearchResolverService(mockOptions.Object);

        // Act
        var result = service.ResolveSearchMode("another track");

        // Assert
        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    #endregion

    #region Edge Cases Tests

    [Fact]
    public void ResolveSearchMode_EmptyString_UsesDefaultMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_WhitespaceOnly_UsesDefaultMode()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("   ");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    [Fact]
    public void ResolveSearchMode_MixedCasePrefix_ResolvesCorrectly()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("SPOTIFY:test");

        // Assert
        Assert.Equal(TrackSearchMode.Spotify, result);
    }

    [Fact]
    public void ResolveSearchMode_MultipleColons_UsesFirstPrefix()
    {
        // Act
        var result = _resolverService.ResolveSearchMode("youtube:test:extra");

        // Assert
        Assert.Equal(TrackSearchMode.YouTube, result);
    }

    #endregion
}