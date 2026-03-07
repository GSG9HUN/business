using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Tracks;

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
        var track = new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" };

        service.SetCurrentTrack(guildId, track);

        Assert.Null(service.GetCurrentTrack(guildId));
    }

    [Fact]
    public void SetCurrentTrack_AfterInit_StoresTrack()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 3;
        var track = new LavalinkTrack { Author = "Author", Title = "Title", Identifier = "id" };

        service.Init(guildId);
        service.SetCurrentTrack(guildId, track);

        Assert.Equal(track, service.GetCurrentTrack(guildId));
    }

    [Fact]
    public void GetCurrentTrackFormatted_WithTrack_ReturnsAuthorAndTitle()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 4;
        var track = new LavalinkTrack { Author = "Rick", Title = "Never Gonna", Identifier = "id" };

        service.Init(guildId);
        service.SetCurrentTrack(guildId, track);

        Assert.Equal("Rick Never Gonna", service.GetCurrentTrackFormatted(guildId));
    }

    [Fact]
    public void GetCurrentTrackFormatted_WithoutTrack_ReturnsEmpty()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 5;

        service.Init(guildId);

        Assert.Equal(string.Empty, service.GetCurrentTrackFormatted(guildId));
    }

    [Fact]
    public void TryGetCurrentTrack_WithTrack_ReturnsTrueAndTrack()
    {
        var service = new CurrentTrackService();
        const ulong guildId = 6;
        var track = new LavalinkTrack { Author = "A", Title = "T", Identifier = "id" };

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