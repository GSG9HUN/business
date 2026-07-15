using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Repositories;

namespace DC_bot_tests.UnitTests.Persistence;

[Trait("Category", "Unit")]
public class PlaylistRepositoryTests
{
    private const ulong GuildId = 42ul;

    private static InMemoryDbContextFactory CreateFactory() =>
        new($"Playlist_{Guid.NewGuid()}");

    [Fact]
    public async Task CreatePlaylistAsync_CreatesGuildDataAndPlaylist()
    {
        var factory = CreateFactory();
        var repository = new PlaylistRepository(factory);

        var playlistId = await repository.CreatePlaylistAsync(GuildId, "mix");

        Assert.True(playlistId > 0);
        Assert.True(await repository.ExistsAsync(GuildId, "mix"));

        await using var dbContext = factory.CreateDbContext();
        Assert.Contains(dbContext.GuildData, guild => guild.GuildId == GuildId);
        Assert.Contains(dbContext.Playlists, playlist => playlist.GuildId == GuildId && playlist.Name == "mix");
    }

    [Fact]
    public async Task GetByGuildAndNameAsync_WhenPlaylistExists_ReturnsRecord()
    {
        var factory = CreateFactory();
        var repository = new PlaylistRepository(factory);
        var playlistId = await repository.CreatePlaylistAsync(GuildId, "mix");

        var result = await repository.GetByGuildAndNameAsync(GuildId, "mix");

        Assert.NotNull(result);
        Assert.Equal(new PlaylistRecord(playlistId, GuildId, "mix"), result);
    }

    [Fact]
    public async Task GetByGuildAsync_ReturnsOnlyGuildPlaylistsOrderedByNameWithTrackCounts()
    {
        var factory = CreateFactory();
        var playlistRepository = new PlaylistRepository(factory);
        var trackRepository = new PlaylistTrackRepository(factory);
        var betaId = await playlistRepository.CreatePlaylistAsync(GuildId, "beta");
        var alphaId = await playlistRepository.CreatePlaylistAsync(GuildId, "alpha");
        await playlistRepository.CreatePlaylistAsync(999ul, "other-guild");
        await trackRepository.AddRangeAsync(betaId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-b-1", "https://example.com/b1"),
            new PlaylistTrackCreateRecord("YouTube", "track-b-2", "https://example.com/b2")
        ]);
        await trackRepository.AddRangeAsync(alphaId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a-1", "https://example.com/a1")
        ]);

        var result = await playlistRepository.GetByGuildAsync(GuildId);

        Assert.Equal(["alpha", "beta"], result.Select(playlist => playlist.Name));
        Assert.Equal([1, 2], result.Select(playlist => playlist.TrackCount));
    }

    [Fact]
    public async Task ExistsAsync_WhenPlaylistIsInDifferentGuild_ReturnsFalse()
    {
        var repository = new PlaylistRepository(CreateFactory());
        await repository.CreatePlaylistAsync(999ul, "mix");

        var exists = await repository.ExistsAsync(GuildId, "mix");

        Assert.False(exists);
    }
}
