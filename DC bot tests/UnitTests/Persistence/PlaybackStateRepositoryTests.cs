using DC_bot.Repositories;
using DC_bot.Repositories.Playback;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
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
        Assert.False(result.IsPaused);
        Assert.Equal(0, result.PositionSeconds);
        Assert.Null(result.PositionUpdatedAtUtc);
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

        await repo.SetCurrentTrackAsync(600ul, "track-abc", 1);

        var result = await repo.GetOrCreateAsync(600ul);
        Assert.Equal("track-abc", result.CurrentTrackIdentifier);
        Assert.Equal(1, result.QueueItemId);
        Assert.False(result.IsPaused);
        Assert.Equal(0, result.PositionSeconds);
        Assert.NotNull(result.PositionUpdatedAtUtc);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenStateAlreadyExists_UpdatesTrack()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.GetOrCreateAsync(700ul);
        await repo.SetCurrentTrackAsync(700ul, "track-first", 1);
        await repo.SetCurrentTrackAsync(700ul, "track-second", 2);

        var result = await repo.GetOrCreateAsync(700ul);
        Assert.Equal("track-second", result.CurrentTrackIdentifier);
        Assert.Equal(2, result.QueueItemId);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenTrackIsNull_ClearsCurrentTrack()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetCurrentTrackAsync(800ul, "track-abc", 1);
        await repo.SetCurrentTrackAsync(800ul, null, null);

        var result = await repo.GetOrCreateAsync(800ul);
        Assert.Null(result.CurrentTrackIdentifier);
        Assert.Null(result.QueueItemId);
        Assert.False(result.IsPaused);
        Assert.Equal(0, result.PositionSeconds);
        Assert.Null(result.PositionUpdatedAtUtc);
    }

    [Fact]
    public async Task SetPlaybackPositionAsync_WhenStateExists_UpdatesPositionFields()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetCurrentTrackAsync(900ul, "track-abc", 1);
        await repo.SetPlaybackPositionAsync(900ul, TimeSpan.FromSeconds(37), isPaused: true);

        var result = await repo.GetOrCreateAsync(900ul);
        Assert.True(result.IsPaused);
        Assert.Equal(37, result.PositionSeconds);
        Assert.NotNull(result.PositionUpdatedAtUtc);
    }

    [Fact]
    public async Task SetCurrentTrackAsync_WhenStateWasPaused_ResetsPositionFields()
    {
        var factory = CreateFactory();
        var repo = new PlaybackStateRepository(factory);

        await repo.SetCurrentTrackAsync(901ul, "track-first", 1);
        await repo.SetPlaybackPositionAsync(901ul, TimeSpan.FromSeconds(37), isPaused: true);
        await repo.SetCurrentTrackAsync(901ul, "track-second", 2);

        var result = await repo.GetOrCreateAsync(901ul);
        Assert.Equal("track-second", result.CurrentTrackIdentifier);
        Assert.False(result.IsPaused);
        Assert.Equal(0, result.PositionSeconds);
        Assert.NotNull(result.PositionUpdatedAtUtc);
    }
}

