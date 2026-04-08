using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

public class PlaybackStateRepositoryTests
{
    private static InMemoryDbContextFactory CreateFactory() =>
        new($"PlaybackState_{Guid.NewGuid()}");

    [Fact]
    public async Task GetOrCreateAsync_WhenNoStateExists_CreatesDefaultState()
    {
        var repo = new PlaybackStateRepository(CreateFactory());

        var result = await repo.GetOrCreateAsync(100ul);

        Assert.Equal(100ul, result.GuildId);
        Assert.False(result.IsRepeating);
        Assert.False(result.IsRepeatingList);
        Assert.Null(result.CurrentTrackIdentifier);
    }

    [Fact]
    public async Task GetOrCreateAsync_WhenStateAlreadyExists_ReturnsExistingState()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.GetOrCreateAsync(200ul);
        await repo.SetRepeatStateAsync(200ul, isRepeating: true, isRepeatingList: false);

        var result = await repo.GetOrCreateAsync(200ul);

        Assert.True(result.IsRepeating);
    }

    [Fact]
    public async Task GetOrCreateAsync_CalledTwice_DoesNotCreateDuplicates()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.GetOrCreateAsync(300ul);
        await repo.GetOrCreateAsync(300ul);

        await using var db = factory.CreateDbContext();
        Assert.Single(db.GuildPlaybackStates);
    }

    [Fact]
    public async Task SetRepeatStateAsync_WhenStateDoesNotExist_CreatesWithGivenValues()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetRepeatStateAsync(400ul, isRepeating: true, isRepeatingList: true);

        var result = await repo.GetOrCreateAsync(400ul);
        Assert.True(result.IsRepeating);
        Assert.True(result.IsRepeatingList);
    }

    [Fact]
    public async Task SetRepeatStateAsync_WhenStateAlreadyExists_UpdatesValues()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.GetOrCreateAsync(500ul);
        await repo.SetRepeatStateAsync(500ul, isRepeating: true, isRepeatingList: true);
        await repo.SetRepeatStateAsync(500ul, isRepeating: false, isRepeatingList: false);

        var result = await repo.GetOrCreateAsync(500ul);
        Assert.False(result.IsRepeating);
        Assert.False(result.IsRepeatingList);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenStateDoesNotExist_CreatesWithTrack()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetCurrentTrackAsync(600ul, "track-abc");

        var result = await repo.GetOrCreateAsync(600ul);
        Assert.Equal("track-abc", result.CurrentTrackIdentifier);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenStateAlreadyExists_UpdatesTrack()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.GetOrCreateAsync(700ul);
        await repo.SetCurrentTrackAsync(700ul, "track-first");
        await repo.SetCurrentTrackAsync(700ul, "track-second");

        var result = await repo.GetOrCreateAsync(700ul);
        Assert.Equal("track-second", result.CurrentTrackIdentifier);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenTrackIsNull_ClearsCurrentTrack()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetCurrentTrackAsync(800ul, "track-abc");
        await repo.SetCurrentTrackAsync(800ul, null);

        var result = await repo.GetOrCreateAsync(800ul);
        Assert.Null(result.CurrentTrackIdentifier);
    }
}

