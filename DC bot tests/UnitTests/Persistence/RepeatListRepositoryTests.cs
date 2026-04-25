using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

public class RepeatListRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"RepeatList_{Guid.NewGuid()}");

    [Fact]
    public async Task GetTrackIdentifiersAsync_WhenEmpty_ReturnsEmptyList()
    {
        var repo = new RepeatListRepository(CreateFactory());

        var result = await repo.GetTrackIdentifiersAsync(1ul);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTrackIdentifiersAsync_AfterReplace_ReturnsTracksInOrder()
    {
        var factory = CreateFactory();
        var repo = new RepeatListRepository(factory);
        var tracks = new[] { "track-a", "track-b", "track-c" };

        await repo.ReplaceAsync(1ul, tracks);
        var result = await repo.GetTrackIdentifiersAsync(1ul);

        Assert.Equal(tracks, result);
    }

    [Fact]
    public async Task ReplaceAsync_WithNewTracks_PersistsTracks()
    {
        var factory = CreateFactory();
        var repo = new RepeatListRepository(factory);

        await repo.ReplaceAsync(2ul, ["x", "y"]);

        var result = await repo.GetTrackIdentifiersAsync(2ul);
        Assert.Equal(["x", "y"], result);
    }

    [Fact]
    public async Task ReplaceAsync_CalledTwice_ReplacesOldTracks()
    {
        var factory = CreateFactory();
        var repo = new RepeatListRepository(factory);

        await repo.ReplaceAsync(3ul, ["old-1", "old-2"]);
        await repo.ReplaceAsync(3ul, ["new-1"]);

        var result = await repo.GetTrackIdentifiersAsync(3ul);
        Assert.Equal(["new-1"], result);
    }

    [Fact]
    public async Task ReplaceAsync_WithEmptyList_ClearsExistingTracks()
    {
        var factory = CreateFactory();
        var repo = new RepeatListRepository(factory);

        await repo.ReplaceAsync(4ul, ["track-a"]);
        await repo.ReplaceAsync(4ul, []);

        var result = await repo.GetTrackIdentifiersAsync(4ul);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ReplaceAsync_WhenExceedsMaxItems_ThrowsInvalidOperationException()
    {
        var repo = new RepeatListRepository(CreateFactory());
        var tooMany = Enumerable.Range(0, 101).Select(i => $"track-{i}").ToList();

        await Assert.ThrowsAsync<InvalidOperationException>(() => repo.ReplaceAsync(5ul, tooMany));
    }

    [Fact]
    public async Task ReplaceAsync_WithNullList_ThrowsArgumentNullException()
    {
        var repo = new RepeatListRepository(CreateFactory());

        await Assert.ThrowsAsync<ArgumentNullException>(() => repo.ReplaceAsync(6ul, null!));
    }

    [Fact]
    public async Task ClearAsync_WhenItemsExist_RemovesAllItems()
    {
        var factory = CreateFactory();
        var repo = new RepeatListRepository(factory);

        await repo.ReplaceAsync(7ul, ["a", "b", "c"]);
        await repo.ClearAsync(7ul);

        var result = await repo.GetTrackIdentifiersAsync(7ul);
        Assert.Empty(result);
    }

    [Fact]
    public async Task ClearAsync_WhenAlreadyEmpty_DoesNotThrow()
    {
        var repo = new RepeatListRepository(CreateFactory());

        var ex = await Record.ExceptionAsync(() => repo.ClearAsync(8ul));

        Assert.Null(ex);
    }
}

