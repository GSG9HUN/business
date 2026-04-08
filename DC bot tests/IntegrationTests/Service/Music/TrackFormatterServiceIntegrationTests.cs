using DC_bot.Interface;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.MusicServices;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Music;

public class TrackFormatterServiceIntegrationTests
{
    [Fact]
    public async Task FormatCurrentTrackList_WhenQueueChanges_ReflectsLatestState()
    {
        const ulong guildId = 1001;

        var repoMock = new Mock<IPlaybackStateRepository>();
        var currentTrack = CreateTrackMock("CurrentAuthor", "CurrentTitle");
        repoMock.Setup(r => r.GetOrCreateAsync(guildId, default))
            .ReturnsAsync(new PlaybackStateRecord(guildId, false, false, null, DateTimeOffset.UtcNow));

        var currentTrackServiceMock = new Mock<ICurrentTrackService>();
        currentTrackServiceMock
            .Setup(s => s.GetCurrentTrackAsync(guildId, default))
            .ReturnsAsync(currentTrack.Object);

        var queueServiceMock = new Mock<IMusicQueueService>();
        var formatter = new TrackFormatterService(currentTrackServiceMock.Object, queueServiceMock.Object);

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

        var aCurrentTrack = CreateTrackMock("A-CurrentAuthor", "A-CurrentTitle");
        var bCurrentTrack = CreateTrackMock("B-CurrentAuthor", "B-CurrentTitle");
        var aQueueTrack = CreateTrackMock("A-QueueAuthor", "A-QueueTitle");
        var bQueueTrack = CreateTrackMock("B-QueueAuthor", "B-QueueTitle");

        var currentTrackServiceMock = new Mock<ICurrentTrackService>();
        currentTrackServiceMock
            .Setup(s => s.GetCurrentTrackAsync(guildA, default))
            .ReturnsAsync(aCurrentTrack.Object);
        currentTrackServiceMock
            .Setup(s => s.GetCurrentTrackAsync(guildB, default))
            .ReturnsAsync(bCurrentTrack.Object);

        var queueServiceMock = new Mock<IMusicQueueService>();
        var formatter = new TrackFormatterService(currentTrackServiceMock.Object, queueServiceMock.Object);

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
