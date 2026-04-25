using DC_bot_tests.TestHelperFiles;
using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class TrackFormatterServiceTests
{
    [Fact]
    public async Task FormatCurrentTrack_DelegatesToCurrentTrackService()
    {
        var current = new Mock<ICurrentTrackService>();
        var queue = new Mock<IMusicQueueService>();
        const ulong guildId = 21;
        current.Setup(x => x.GetCurrentTrackFormattedAsync(guildId, default)).ReturnsAsync("A T");

        var service = new TrackFormatterService(current.Object, queue.Object);

        var result = await service.FormatCurrentTrackAsync(guildId);

        Assert.Equal("A T", result);
    }

    [Fact]
    public async Task FormatCurrentTrackList_WithCurrentAndQueue_ReturnsCombinedLines()
    {
        var current = new Mock<ICurrentTrackService>();
        var queue = new Mock<IMusicQueueService>();
        const ulong guildId = 22;

        current.Setup(x => x.GetCurrentTrackAsync(guildId, default))
            .ReturnsAsync(TrackTestHelper.CreateTrackWrapper("CurA", "CurT", "id0"));

        var t1 = new Mock<ILavaLinkTrack>();
        t1.SetupGet(x => x.Author).Returns("Q1A");
        t1.SetupGet(x => x.Title).Returns("Q1T");
        var t2 = new Mock<ILavaLinkTrack>();
        t2.SetupGet(x => x.Author).Returns("Q2A");
        t2.SetupGet(x => x.Title).Returns("Q2T");

        queue.Setup(x => x.ViewQueue(guildId)).ReturnsAsync(new[] { t1.Object, t2.Object });

        var service = new TrackFormatterService(current.Object, queue.Object);

        var result = await service.FormatCurrentTrackListAsync(guildId);

        Assert.Equal("CurA CurT\nQ1A Q1T\nQ2A Q2T\n", result);
    }

    [Fact]
    public async Task FormatCurrentTrackList_WithoutCurrentTrack_ReturnsQueueOnly()
    {
        var current = new Mock<ICurrentTrackService>();
        var queue = new Mock<IMusicQueueService>();
        const ulong guildId = 23;

        current.Setup(x => x.GetCurrentTrackAsync(guildId, default)).ReturnsAsync((ILavaLinkTrack?)null);

        var t1 = new Mock<ILavaLinkTrack>();
        t1.SetupGet(x => x.Author).Returns("Q1A");
        t1.SetupGet(x => x.Title).Returns("Q1T");
        queue.Setup(x => x.ViewQueue(guildId)).ReturnsAsync(new[] { t1.Object });

        var service = new TrackFormatterService(current.Object, queue.Object);

        var result = await service.FormatCurrentTrackListAsync(guildId);

        Assert.Equal("Q1A Q1T\n", result);
    }
}