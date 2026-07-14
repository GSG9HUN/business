using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class PlaylistTrackRepositoryTests
{
    private const ulong GuildId = 42ul;

    private static InMemoryDbContextFactory CreateFactory() =>
        new($"PlaylistTracks_{Guid.NewGuid()}");

    [Fact]
    public async Task AddRangeAsync_AddsTracksWithIncreasingOrderNumbers()
    {
        var factory = CreateFactory();
        var playlistId = await new PlaylistRepository(factory).CreatePlaylistAsync(GuildId, "mix");
        var repository = new PlaylistTrackRepository(factory);

        await repository.AddRangeAsync(playlistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a", "https://example.com/a"),
            new PlaylistTrackCreateRecord("YouTube", "track-b", "https://example.com/b")
        ]);

        var tracks = await repository.GetByPlaylistIdOrderedAsync(playlistId);

        Assert.Equal(["track-a", "track-b"], tracks.Select(track => track.TrackIdentifier));
        Assert.Equal([1, 2], tracks.Select(track => track.OrderNumber));
    }

    [Fact]
    public async Task AddTrackAsync_AppendsAfterExistingTracks()
    {
        var factory = CreateFactory();
        var playlistId = await new PlaylistRepository(factory).CreatePlaylistAsync(GuildId, "mix");
        var repository = new PlaylistTrackRepository(factory);
        await repository.AddRangeAsync(playlistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a", "https://example.com/a"),
            new PlaylistTrackCreateRecord("YouTube", "track-b", "https://example.com/b")
        ]);

        await repository.AddTrackAsync(playlistId,
            new PlaylistTrackCreateRecord("YouTube", "track-c", "https://example.com/c"));

        var tracks = await repository.GetByPlaylistIdOrderedAsync(playlistId);

        Assert.Equal(["track-a", "track-b", "track-c"], tracks.Select(track => track.TrackIdentifier));
        Assert.Equal([1, 2, 3], tracks.Select(track => track.OrderNumber));
    }

    [Fact]
    public async Task RemoveTrackAsync_RemovesTrackAndCompactsOrderNumbers()
    {
        var factory = CreateFactory();
        var playlistId = await new PlaylistRepository(factory).CreatePlaylistAsync(GuildId, "mix");
        var repository = new PlaylistTrackRepository(factory);
        await repository.AddRangeAsync(playlistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a", "https://example.com/a"),
            new PlaylistTrackCreateRecord("YouTube", "track-b", "https://example.com/b"),
            new PlaylistTrackCreateRecord("YouTube", "track-c", "https://example.com/c")
        ]);

        await repository.RemoveTrackAsync(playlistId, 2);

        var tracks = await repository.GetByPlaylistIdOrderedAsync(playlistId);

        Assert.Equal(["track-a", "track-c"], tracks.Select(track => track.TrackIdentifier));
        Assert.Equal([1, 2], tracks.Select(track => track.OrderNumber));
    }

    [Fact]
    public async Task AddRangeAsync_WithEmptyCollection_DoesNotAddTracks()
    {
        var factory = CreateFactory();
        var playlistId = await new PlaylistRepository(factory).CreatePlaylistAsync(GuildId, "mix");
        var repository = new PlaylistTrackRepository(factory);

        await repository.AddRangeAsync(playlistId, []);

        Assert.Empty(await repository.GetByPlaylistIdOrderedAsync(playlistId));
    }
}
