using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.MusicServices;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class CurrentTrackServiceTests
{
    private static PlaybackStateRecord CreateState(ulong guildId, string? trackIdentifier = null)
        => new(guildId, false, false, trackIdentifier, null, DateTimeOffset.UtcNow);

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

        repo.Verify(r => r.SetCurrentTrackAsync(
            guildId, 
            null, 
            null, 
            It.IsAny<CancellationToken>()), Times.Once);
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

        repo.Verify(r => r.SetCurrentTrackAsync(
            guildId, 
            "some-identifier", 
            null, 
            It.IsAny<CancellationToken>()), Times.Once);
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

    [Fact]
    public async Task SetCurrentTrackAsync_WithLavaLinkTrackWrapper_PassesQueueItemIdToRepository()
    {
        const ulong guildId = 6;
        var repo = new Mock<IPlaybackStateRepository>();
        var wrapper = DC_bot_tests.TestHelperFiles.TrackTestHelper.CreateTrackWrapper(queueItemId: 99L);

        var service = new CurrentTrackService(repo.Object);

        await service.SetCurrentTrackAsync(guildId, wrapper);

        repo.Verify(r => r.SetCurrentTrackAsync(
            guildId,
            It.IsAny<string>(),
            99L,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WithLavaLinkTrackWrapperWithNullQueueItemId_PassesNullQueueItemIdToRepository()
    {
        const ulong guildId = 7;
        var repo = new Mock<IPlaybackStateRepository>();
        var wrapper = DC_bot_tests.TestHelperFiles.TrackTestHelper.CreateTrackWrapper(); // QueueItemId = null

        var service = new CurrentTrackService(repo.Object);

        await service.SetCurrentTrackAsync(guildId, wrapper);

        repo.Verify(r => r.SetCurrentTrackAsync(
            guildId,
            It.IsAny<string>(),
            null,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetCurrentTrackAsync_WhenQueueItemIdStoredInState_RestoredInWrapper()
    {
        const ulong guildId = 8;
        var repo = new Mock<IPlaybackStateRepository>();
        const string validIdentifier = "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=";
        repo.Setup(r => r.GetOrCreateAsync(guildId, default))
            .ReturnsAsync(new DC_bot.Interface.Service.Persistence.Models.PlaybackStateRecord(guildId, false, false, validIdentifier, 42L, DateTimeOffset.UtcNow));

        var service = new CurrentTrackService(repo.Object);

        var result = await service.GetCurrentTrackAsync(guildId);

        var wrapper = Assert.IsType<DC_bot.Wrapper.LavaLinkTrackWrapper>(result);
        Assert.Equal(42L, wrapper.QueueItemId);
    }
}