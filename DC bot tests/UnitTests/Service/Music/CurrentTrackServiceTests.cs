using DC_bot_tests.TestHelperFiles;
using DC_bot.Service.Music.MusicServices;

namespace DC_bot_tests.UnitTests.Service.Music;

public class CurrentTrackServiceTests
{
    [Fact]
    public void Init_ThenGetCurrentTrack_ReturnsNull()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 1;

        service.Init(guildId);

        Assert.Null(service.GetCurrentTrack(guildId));
    }

    [Fact]
    public void SetCurrentTrack_WithoutInit_DoesNothing()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 2;
        var track = TrackTestHelper.CreateTrackWrapper("A", "T");

        service.SetCurrentTrack(guildId, track);

        Assert.Null(service.GetCurrentTrack(guildId));
    }

    [Fact]
    public void SetCurrentTrack_AfterInit_StoresTrack()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 3;
        var track = TrackTestHelper.CreateTrackWrapper("Author", "Title");

        service.Init(guildId);
        service.SetCurrentTrack(guildId, track);

        Assert.Equal(track, service.GetCurrentTrack(guildId));
    }

    [Fact]
    public void GetCurrentTrackFormatted_WithTrack_ReturnsAuthorAndTitle()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 4;
        var track = TrackTestHelper.CreateTrackWrapper("Rick", "Never Gonna");

        service.Init(guildId);
        service.SetCurrentTrack(guildId, track);

        Assert.Equal("Rick Never Gonna", service.GetCurrentTrackFormatted(guildId));
    }

    [Fact]
    public void TryGetCurrentTrack_WithTrack_ReturnsTrueAndTrack()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 6;
        var track = TrackTestHelper.CreateTrackWrapper("A", "T");

        service.Init(guildId);
        service.SetCurrentTrack(guildId, track);

        var ok = service.TryGetCurrentTrack(guildId, out var actual);

        Assert.True(ok);
        Assert.Equal(track, actual);
    }

    [Fact]
    public void TryGetCurrentTrack_WithoutTrack_ReturnsFalse()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 7;

        service.Init(guildId);

        var ok = service.TryGetCurrentTrack(guildId, out var actual);

        Assert.False(ok);
        Assert.Null(actual);
    }
}