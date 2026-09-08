using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Interface.Service.Persistence.Models.Playlists;
using DC_bot.Repositories;
using DC_bot.Repositories.Playlists;

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
    public async Task GetByPlaylistIdsOrderedAsync_ReturnsTracksForRequestedPlaylistsOnly()
    {
        var factory = CreateFactory();
        var playlistRepository = new PlaylistRepository(factory);
        var firstPlaylistId = await playlistRepository.CreatePlaylistAsync(GuildId, "first");
        var secondPlaylistId = await playlistRepository.CreatePlaylistAsync(GuildId, "second");
        var otherPlaylistId = await playlistRepository.CreatePlaylistAsync(999ul, "other");
        var repository = new PlaylistTrackRepository(factory);
        await repository.AddRangeAsync(firstPlaylistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "first-a", "https://example.com/first-a"),
            new PlaylistTrackCreateRecord("YouTube", "first-b", "https://example.com/first-b")
        ]);
        await repository.AddRangeAsync(secondPlaylistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "second-a", "https://example.com/second-a")
        ]);
        await repository.AddRangeAsync(otherPlaylistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "other-a", "https://example.com/other-a")
        ]);

        var tracks = await repository.GetByPlaylistIdsOrderedAsync([secondPlaylistId, firstPlaylistId]);

        Assert.Equal(
            [firstPlaylistId, firstPlaylistId, secondPlaylistId],
            tracks.Select(track => track.PlaylistId));
        Assert.Equal(["first-a", "first-b", "second-a"], tracks.Select(track => track.TrackIdentifier));
        Assert.DoesNotContain(tracks, track => track.PlaylistId == otherPlaylistId);
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
