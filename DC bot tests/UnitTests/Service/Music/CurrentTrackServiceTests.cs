using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.MusicServices;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class CurrentTrackServiceTests
{
    private static PlaybackStateRecord CreateState(ulong guildId, string? trackIdentifier = null)
        => new(guildId, false, false, trackIdentifier, DateTimeOffset.UtcNow);

    [Fact]
    public async Task GetCurrentTrackAsync_WhenNoTrackStored_ReturnsNull()
    {
        const ulong guildId = 1;
        var repo = new Mock<IPlaybackStateRepository>();
        repo.Setup(r => r.GetOrCreateAsync(guildId, default)).ReturnsAsync(CreateState(guildId));

        var service = new CurrentTrackService(repo.Object);

        var result = await service.GetCurrentTrackAsync(guildId);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetCurrentTrackAsync_WhenInvalidIdentifierStored_ReturnsNull()
    {
        const ulong guildId = 2;
        var repo = new Mock<IPlaybackStateRepository>();
        repo.Setup(r => r.GetOrCreateAsync(guildId, default)).ReturnsAsync(CreateState(guildId, "bad-identifier"));

        var service = new CurrentTrackService(repo.Object);

        var result = await service.GetCurrentTrackAsync(guildId);

        Assert.Null(result);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WithNull_StoresNullIdentifier()
    {
        const ulong guildId = 3;
        var repo = new Mock<IPlaybackStateRepository>();

        var service = new CurrentTrackService(repo.Object);

        await service.SetCurrentTrackAsync(guildId, null);

        repo.Verify(r => r.SetCurrentTrackAsync(guildId, null, default), Times.Once);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WithTrack_StoresIdentifier()
    {
        const ulong guildId = 4;
        var repo = new Mock<IPlaybackStateRepository>();
        var trackMock = new Mock<DC_bot.Interface.ILavaLinkTrack>();
        trackMock.Setup(t => t.ToString()).Returns("some-identifier");

        var service = new CurrentTrackService(repo.Object);

        await service.SetCurrentTrackAsync(guildId, trackMock.Object);

        repo.Verify(r => r.SetCurrentTrackAsync(guildId, "some-identifier", default), Times.Once);
    }

    [Fact]
    public async Task GetCurrentTrackFormattedAsync_WhenNoTrack_ReturnsEmpty()
    {
        const ulong guildId = 5;
        var repo = new Mock<IPlaybackStateRepository>();
        repo.Setup(r => r.GetOrCreateAsync(guildId, default)).ReturnsAsync(CreateState(guildId));

        var service = new CurrentTrackService(repo.Object);

        var result = await service.GetCurrentTrackFormattedAsync(guildId);

        Assert.Equal(string.Empty, result);
    }
}