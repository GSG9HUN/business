using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Service.Music.MusicServices;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Music;

public class TrackFormatterServiceIntegrationTests
{
    [Fact]
    public async Task FormatCurrentTrackList_WhenQueueChanges_ReflectsLatestState()
    {
        const ulong guildId = 1001;
        var currentTrackService = new CurrentTrackService();
        currentTrackService.Init(guildId);

        var queueServiceMock = new Mock<IMusicQueueService>();
        var formatter = new TrackFormatterService(currentTrackService, queueServiceMock.Object);

        var currentTrack = CreateTrackMock("CurrentAuthor", "CurrentTitle");
        currentTrackService.SetCurrentTrack(guildId, currentTrack.Object);

        var trackA = CreateTrackMock("QueueAuthorA", "QueueTitleA");
        var trackB = CreateTrackMock("QueueAuthorB", "QueueTitleB");

        queueServiceMock
            .SetupSequence(q => q.ViewQueue(guildId))
            .ReturnsAsync((IReadOnlyCollection<ILavaLinkTrack>)new List<ILavaLinkTrack> { trackA.Object, trackB.Object })
            .ReturnsAsync((IReadOnlyCollection<ILavaLinkTrack>)new List<ILavaLinkTrack> { trackB.Object });

        var beforeDequeue = await formatter.FormatCurrentTrackListAsync(guildId);
        var afterDequeue = await formatter.FormatCurrentTrackListAsync(guildId);

        Assert.Equal("CurrentAuthor CurrentTitle\nQueueAuthorA QueueTitleA\nQueueAuthorB QueueTitleB\n", beforeDequeue);
        Assert.Equal("CurrentAuthor CurrentTitle\nQueueAuthorB QueueTitleB\n", afterDequeue);
    }

    [Fact]
    public async Task FormatCurrentTrackList_MultiGuildState_DoesNotLeakBetweenGuilds()
    {
        const ulong guildA = 2001;
        const ulong guildB = 2002;

        var currentTrackService = new CurrentTrackService();
        currentTrackService.Init(guildA);
        currentTrackService.Init(guildB);

        var queueServiceMock = new Mock<IMusicQueueService>();
        var formatter = new TrackFormatterService(currentTrackService, queueServiceMock.Object);

        var aCurrentTrack = CreateTrackMock("A-CurrentAuthor", "A-CurrentTitle");
        var bCurrentTrack = CreateTrackMock("B-CurrentAuthor", "B-CurrentTitle");
        var aQueueTrack = CreateTrackMock("A-QueueAuthor", "A-QueueTitle");
        var bQueueTrack = CreateTrackMock("B-QueueAuthor", "B-QueueTitle");

        currentTrackService.SetCurrentTrack(guildA, aCurrentTrack.Object);
        currentTrackService.SetCurrentTrack(guildB, bCurrentTrack.Object);

        queueServiceMock
            .Setup(q => q.ViewQueue(guildA))
            .ReturnsAsync((IReadOnlyCollection<ILavaLinkTrack>)new List<ILavaLinkTrack> { aQueueTrack.Object });
        queueServiceMock
            .Setup(q => q.ViewQueue(guildB))
            .ReturnsAsync((IReadOnlyCollection<ILavaLinkTrack>)new List<ILavaLinkTrack> { bQueueTrack.Object });

        var resultA = await formatter.FormatCurrentTrackListAsync(guildA);
        var resultB = await formatter.FormatCurrentTrackListAsync(guildB);

        Assert.Contains("A-CurrentAuthor A-CurrentTitle", resultA);
        Assert.Contains("A-QueueAuthor A-QueueTitle", resultA);
        Assert.DoesNotContain("B-CurrentAuthor", resultA);
        Assert.DoesNotContain("B-QueueAuthor", resultA);

        Assert.Contains("B-CurrentAuthor B-CurrentTitle", resultB);
        Assert.Contains("B-QueueAuthor B-QueueTitle", resultB);
        Assert.DoesNotContain("A-CurrentAuthor", resultB);
        Assert.DoesNotContain("A-QueueAuthor", resultB);
    }

    private static Mock<ILavaLinkTrack> CreateTrackMock(string author, string title)
    {
        var mock = new Mock<ILavaLinkTrack>();
        mock.SetupGet(t => t.Author).Returns(author);
        mock.SetupGet(t => t.Title).Returns(title);
        return mock;
    }
}
