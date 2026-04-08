using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

public class QueueRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"Queue_{Guid.NewGuid()}");

    [Fact]
    public async Task GetQueuedItemsAsync_WhenEmpty_ReturnsEmptyList()
    {
        var repo = new QueueRepository(CreateFactory());

        var result = await repo.GetQueuedItemsAsync(1ul);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetQueuedItemsAsync_ReturnsOnlyQueuedItemsInPositionOrder()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item1 = await repo.EnqueueAsync(1ul, "track-a");
        var item2 = await repo.EnqueueAsync(1ul, "track-b");
        await repo.MarkPlayedAsync(item1.Id);

        var result = await repo.GetQueuedItemsAsync(1ul);

        Assert.Single(result);
        Assert.Equal(item2.Id, result[0].Id);
    }

    [Fact]
    public async Task GetNextQueuedItemAsync_WhenEmpty_ReturnsNull()
    {
        var repo = new QueueRepository(CreateFactory());

        var result = await repo.GetNextQueuedItemAsync(1ul);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetNextQueuedItemAsync_ReturnsItemWithLowestPosition()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var first = await repo.EnqueueAsync(1ul, "track-a");
        await repo.EnqueueAsync(1ul, "track-b");

        var result = await repo.GetNextQueuedItemAsync(1ul);

        Assert.NotNull(result);
        Assert.Equal(first.Id, result.Id);
    }

    [Fact]
    public async Task GetPreviousItemAsync_WhenNonePlayedOrSkipped_ReturnsNull()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);
        await repo.EnqueueAsync(1ul, "track-a");

        var result = await repo.GetPreviousItemAsync(1ul);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetPreviousItemAsync_AfterMarkPlayed_ReturnsThatItem()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.MarkPlayedAsync(item.Id);

        var result = await repo.GetPreviousItemAsync(1ul);

        Assert.NotNull(result);
        Assert.Equal(item.Id, result!.Id);
    }

    [Fact]
    public async Task GetPreviousItemAsync_AfterMarkSkipped_ReturnsThatItem()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.MarkSkippedAsync(item.Id);

        var result = await repo.GetPreviousItemAsync(1ul);

        Assert.NotNull(result);
        Assert.Equal(item.Id, result!.Id);
    }

    [Fact]
    public async Task EnqueueAsync_AddsItemToQueue()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-x");

        Assert.Equal("track-x", item.TrackIdentifier);
        Assert.Equal(1ul, item.GuildId);
    }

    [Fact]
    public async Task EnqueueAsync_MultipleItems_AssignsIncreasingPositions()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var a = await repo.EnqueueAsync(1ul, "track-a");
        var b = await repo.EnqueueAsync(1ul, "track-b");
        var c = await repo.EnqueueAsync(1ul, "track-c");

        Assert.True(a.Position < b.Position);
        Assert.True(b.Position < c.Position);
    }

    [Fact]
    public async Task EnqueueAsync_WhenQueueFullAtHundredItems_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        for (var i = 0; i < 100; i++)
        {
            await repo.EnqueueAsync(1ul, $"track-{i}");
        }

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.EnqueueAsync(1ul, "track-overflow"));
    }

    [Fact]
    public async Task MarkPlayingAsync_ChangesStateToPlaying()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.MarkPlayingAsync(item.Id);

        var queued = await repo.GetQueuedItemsAsync(1ul);
        Assert.Empty(queued);
    }

    [Fact]
    public async Task MarkPlayedAsync_ItemNoLongerInQueue()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.MarkPlayedAsync(item.Id);

        var queued = await repo.GetQueuedItemsAsync(1ul);
        Assert.Empty(queued);
    }

    [Fact]
    public async Task MarkSkippedAsync_ItemNoLongerInQueue()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.MarkSkippedAsync(item.Id);

        var queued = await repo.GetQueuedItemsAsync(1ul);
        Assert.Empty(queued);
    }

    [Fact]
    public async Task MarkPlayedAsync_WhenItemDoesNotExist_DoesNotThrow()
    {
        var repo = new QueueRepository(CreateFactory());

        var ex = await Record.ExceptionAsync(() => repo.MarkPlayedAsync(99999L));

        Assert.Null(ex);
    }

    [Fact]
    public async Task UpdateQueueItemPositionAsync_ChangesPosition()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        var item = await repo.EnqueueAsync(1ul, "track-a");
        await repo.UpdateQueueItemPositionAsync(item.Id, 999);

        await using var db = factory.CreateDbContext();
        var entity = db.GuildQueueItems.Single(q => q.Id == item.Id);
        Assert.Equal(999, entity.Position);
    }

    [Fact]
    public async Task UpdateQueueItemPositionAsync_WhenItemDoesNotExist_DoesNotThrow()
    {
        var repo = new QueueRepository(CreateFactory());

        var ex = await Record.ExceptionAsync(() => repo.UpdateQueueItemPositionAsync(99999L, 0));

        Assert.Null(ex);
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_ChangesOrderCorrectly()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        await repo.EnqueueAsync(1ul, "track-a");
        await repo.EnqueueAsync(1ul, "track-b");
        await repo.EnqueueAsync(1ul, "track-c");

        await repo.ReorderQueuedItemsAsync(1ul, ["track-c", "track-a", "track-b"]);

        var result = await repo.GetQueuedItemsAsync(1ul);
        Assert.Equal(["track-c", "track-a", "track-b"], result.Select(r => r.TrackIdentifier));
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_WhenCountMismatch_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        await repo.EnqueueAsync(1ul, "track-a");
        await repo.EnqueueAsync(1ul, "track-b");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.ReorderQueuedItemsAsync(1ul, ["track-a"]));
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_WhenTrackNotInQueue_ThrowsInvalidOperationException()
    {
        var factory = CreateFactory();
        var repo = new QueueRepository(factory);

        await repo.EnqueueAsync(1ul, "track-a");
        await repo.EnqueueAsync(1ul, "track-b");

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.ReorderQueuedItemsAsync(1ul, ["track-a", "track-UNKNOWN"]));
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_WhenExceedsMaxItems_ThrowsInvalidOperationException()
    {
        var repo = new QueueRepository(CreateFactory());
        var tooMany = Enumerable.Range(0, 101).Select(i => $"track-{i}").ToList();

        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            repo.ReorderQueuedItemsAsync(1ul, tooMany));
    }

    [Fact]
    public async Task ReorderQueuedItemsAsync_WithNullList_ThrowsArgumentNullException()
    {
        var repo = new QueueRepository(CreateFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(() =>
            repo.ReorderQueuedItemsAsync(1ul, null!));
    }
}

