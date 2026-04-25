using DC_bot.Interface;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Service.Music.MusicServices;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

public class MusicQueueServiceTests
{
    private const ulong GuildId = 12345UL;
    private const string ValidTrackIdentifier = "QAAA2QMAPFJpY2sgQXN0bGV5IC0gTmV2ZXIgR29ubmEgR2l2ZSBZb3UgVXAgKE9mZmljaWFsIE11c2ljIFZpZGVvKQALUmljayBBc3RsZXkAAAAAAANACAALZFF3NHc5V2dYY1EAAQAraHR0cHM6Ly93d3cueW91dHViZS5jb20vd2F0Y2g/dj1kUXc0dzlXZ1hjUQEANGh0dHBzOi8vaS55dGltZy5jb20vdmkvZFF3NHc5V2dYY1EvbWF4cmVzZGVmYXVsdC5qcGcAAAd5b3V0dWJlAAAAAAAAAAA=";
    private readonly Mock<IQueueRepository> _queueRepositoryMock = new();
    private readonly MusicQueueService _service;

    public MusicQueueServiceTests()
    {
        _service = new MusicQueueService(_queueRepositoryMock.Object, Mock.Of<ILogger<MusicQueueService>>());
    }

    [Fact]
    public async Task HasTracks_WhenRepositoryReturnsItems_ReturnsTrue()
    {
        _queueRepositoryMock
            .Setup(repository => repository.AnyQueuedItemsAsync(GuildId, default))
            .ReturnsAsync(true);

        var result = await _service.HasTracks(GuildId);

        Assert.True(result);
    }

    [Fact]
    public async Task HasTracks_WhenRepositoryReturnsNoItems_ReturnsFalse()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([]);

        var result = await _service.HasTracks(GuildId);

        Assert.False(result);
    }

    [Fact]
    public async Task Enqueue_DelegatesToRepository()
    {
        var track = CreateTrackMock("track-id-1");

        await _service.Enqueue(GuildId, track.Object);

        _queueRepositoryMock.Verify(
            repository => repository.EnqueueAsync(GuildId, "track-id-1", default),
            Times.Once);
    }

    [Fact]
    public async Task EnqueueMany_DelegatesToRepository()
    {
        var track1 = CreateTrackMock("track-id-1");
        var track2 = CreateTrackMock("track-id-2");

        await _service.EnqueueMany(GuildId, [track1.Object, track2.Object]);

        _queueRepositoryMock.Verify(
            repository => repository.EnqueueManyAsync(
                GuildId,
                It.Is<IReadOnlyList<string>>(ids =>
                    ids.Count == 2 &&
                    ids[0] == "track-id-1" &&
                    ids[1] == "track-id-2"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task EnqueueMany_WhenEmptyCollection_DoesNotCallRepository()
    {
        await _service.EnqueueMany(GuildId, []);

        _queueRepositoryMock.Verify(
            repository => repository.EnqueueManyAsync(It.IsAny<ulong>(), It.IsAny<IReadOnlyList<string>>(), default),
            Times.Never);
    }

    [Fact]
    public async Task Dequeue_WhenQueueItemExists_ReturnsTrack()
    {
        var record = CreateRecord(ValidTrackIdentifier, 77);

        _queueRepositoryMock
            .Setup(repository => repository.ClaimNextQueuedItemAsync(GuildId, default))
            .ReturnsAsync(record);

        var result = await _service.Dequeue(GuildId);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task Dequeue_WhenQueueIsEmpty_ReturnsNull()
    {
        _queueRepositoryMock
            .Setup(repository => repository.ClaimNextQueuedItemAsync(GuildId, default))
            .ReturnsAsync((QueueItemRecord?)null);

        var result = await _service.Dequeue(GuildId);

        Assert.Null(result);
        _queueRepositoryMock.Verify(repository => repository.MarkSkippedAsync(It.IsAny<long>(), default), Times.Never);
    }

    [Fact]
    public async Task ViewQueue_ReturnsTracksInRepositoryOrder()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([CreateRecord(ValidTrackIdentifier, 1, 0), CreateRecord(ValidTrackIdentifier, 2, 1)]);

        var result = await _service.ViewQueue(GuildId);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetQueue_ReturnsQueueInRepositoryOrder()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([CreateRecord(ValidTrackIdentifier, 1, 0), CreateRecord(ValidTrackIdentifier, 2, 1)]);

        var queue = await _service.GetQueue(GuildId);

        Assert.Equal(2, queue.Count);
    }

    [Fact]
    public async Task SetQueue_ReordersQueuedItemsUsingTrackIdentifiers()
    {
        var first = CreateTrackMock("track-id-a");
        var second = CreateTrackMock("track-id-b");
        var reorderedQueue = new Queue<ILavaLinkTrack>([second.Object, first.Object]);

        await _service.SetQueue(GuildId, reorderedQueue);

        _queueRepositoryMock.Verify(
            repository => repository.ReorderQueuedItemsAsync(
                GuildId,
                It.Is<IReadOnlyList<string>>(tracks =>
                    tracks.Count == 2 &&
                    tracks[0] == "track-id-b" &&
                    tracks[1] == "track-id-a"),
                default),
            Times.Once);
    }

    [Fact]
    public async Task ClearQueue_DelegatesMarkAllQueuedAsSkippedToRepository()
    {
        await _service.ClearQueue(GuildId);

        _queueRepositoryMock.Verify(
            repository => repository.MarkAllQueuedAsSkippedAsync(GuildId, default),
            Times.Once);
    }

    [Fact]
    public async Task ClearQueue_WhenQueueIsEmpty_DoesNotMarkAnythingSkipped()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([]);

        await _service.ClearQueue(GuildId);

        _queueRepositoryMock.Verify(repository => repository.MarkSkippedAsync(It.IsAny<long>(), default), Times.Never);
    }

    [Fact]
    public void GetRepeatableQueue_ThrowsNotImplementedException()
    {
        Assert.Throws<NotImplementedException>(() => _service.GetRepeatableQueue(GuildId));
    }

    [Fact]
    public async Task Dequeue_WhenTrackIdentifierParseFails_MarksSkippedAndContinues()
    {
        _queueRepositoryMock
            .SetupSequence(repository => repository.ClaimNextQueuedItemAsync(GuildId, default))
            .ReturnsAsync(CreateRecord("not-a-valid-track", 99))
            .ReturnsAsync((QueueItemRecord?)null);

        var result = await _service.Dequeue(GuildId);

        Assert.Null(result);
        _queueRepositoryMock.Verify(repository => repository.MarkSkippedAsync(99, default), Times.Once);
    }

    [Fact]
    public async Task ViewQueue_WhenTrackIdentifierParseFails_SkipsItemAndMarksSkipped()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([CreateRecord("bad-identifier", 1, 0)]);

        var result = await _service.ViewQueue(GuildId);

        Assert.Empty(result);
        _queueRepositoryMock.Verify(repository => repository.MarkSkippedAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task GetQueue_WhenTrackIdentifierParseFails_SkipsItemAndMarksSkipped()
    {
        _queueRepositoryMock
            .Setup(repository => repository.GetQueuedItemsAsync(GuildId, default))
            .ReturnsAsync([CreateRecord("bad-identifier", 1, 0)]);

        var result = await _service.GetQueue(GuildId);

        Assert.Empty(result);
        _queueRepositoryMock.Verify(repository => repository.MarkSkippedAsync(1, default), Times.Once);
    }

    [Fact]
    public async Task SetQueue_WhenQueueExceedsMaxSize_ThrowsInvalidOperationException()
    {
        var tracks = Enumerable.Range(0, 101)
            .Select(i =>
            {
                var t = CreateTrackMock($"track-{i}");
                return t.Object;
            })
            .ToList();

        var tooLargeQueue = new Queue<ILavaLinkTrack>(tracks);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.SetQueue(GuildId, tooLargeQueue));
        _queueRepositoryMock.Verify(
            repository => repository.ReorderQueuedItemsAsync(It.IsAny<ulong>(), It.IsAny<IReadOnlyList<string>>(), default),
            Times.Never);
    }

    private static QueueItemRecord CreateRecord(string trackIdentifier, long id = 1, int position = 0)
    {
        return new QueueItemRecord(id, GuildId, position, trackIdentifier, 0, DateTimeOffset.UtcNow, null, null);
    }

    private static Mock<ILavaLinkTrack> CreateTrackMock(string identifier)
    {
        var track = new Mock<ILavaLinkTrack>();
        track.Setup(t => t.ToString()).Returns(identifier);
        track.SetupGet(t => t.Title).Returns("Title");
        track.SetupGet(t => t.Author).Returns("Author");
        return track;
    }
}
