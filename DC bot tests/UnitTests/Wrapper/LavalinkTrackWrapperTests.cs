using DC_bot.Wrapper;
using Lavalink4NET.Tracks;

namespace DC_bot_tests.UnitTests.Wrapper;

public class LavalinkTrackWrapperTests
{
    private static LavalinkTrack BuildTrack(
        string title = "Test Song",
        string author = "Test Artist",
        TimeSpan? duration = null,
        Uri? artworkUri = null,
        TimeSpan? startPosition = null)
    {
        return new LavalinkTrack
        {
            Title = title,
            Author = author,
            Duration = duration ?? TimeSpan.FromMinutes(3),
            ArtworkUri = artworkUri,
            StartPosition = startPosition,
            Identifier = "test-id",
            IsSeekable = true,
            IsLiveStream = false,
            Uri = new Uri("https://example.com/track"),
            SourceName = "youtube",
            ProbeInfo = null
        };
    }

    [Fact]
    public void Properties_AreExposedFromUnderlyingTrack()
    {
        var artwork = new Uri("https://example.com/art.jpg");
        var start = TimeSpan.FromSeconds(10);
        var track = BuildTrack("My Song", "My Artist", TimeSpan.FromMinutes(4), artwork, start);
        var wrapper = new LavaLinkTrackWrapper(track);

        Assert.Equal("My Song", wrapper.Title);
        Assert.Equal("My Artist", wrapper.Author);
        Assert.Equal(TimeSpan.FromMinutes(4), wrapper.Duration);
        Assert.Equal(artwork, wrapper.ArtworkUri);
        Assert.Equal(start, wrapper.StartPosition);
    }

    [Fact]
    public void ArtworkUri_WhenNull_ReturnsNull()
    {
        var wrapper = new LavaLinkTrackWrapper(BuildTrack(artworkUri: null));

        Assert.Null(wrapper.ArtworkUri);
    }

    [Fact]
    public void StartPosition_WhenNull_ReturnsNull()
    {
        var wrapper = new LavaLinkTrackWrapper(BuildTrack(startPosition: null));

        Assert.Null(wrapper.StartPosition);
    }

    [Fact]
    public void ToLavalinkTrack_ReturnsSameInstance()
    {
        var track = BuildTrack();
        var wrapper = new LavaLinkTrackWrapper(track);

        Assert.Equal(track, wrapper.ToLavalinkTrack());
    }

    [Fact]
    public void ToString_DelegatesToUnderlyingTrack()
    {
        var track = BuildTrack("Song", "Artist");
        var wrapper = new LavaLinkTrackWrapper(track);

        Assert.Equal(track.ToString(), wrapper.ToString());
    }
}

