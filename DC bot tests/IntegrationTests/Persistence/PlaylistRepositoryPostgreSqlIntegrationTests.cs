using DC_bot.Interface.Service.Persistence.Models;
using DC_bot.Persistence.Db;
using DC_bot.Persistence.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace DC_bot_tests.IntegrationTests.Persistence;

[Collection("Integration Tests")]
[Trait("Category", "Integration")]
public class PlaylistRepositoryPostgreSqlIntegrationTests
{
    [Fact]
    public async Task PlaylistRepositories_CreateListRenameDeletePlaylistWithTracks()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var playlistRepository = new PlaylistRepository(factory);
        var trackRepository = new PlaylistTrackRepository(factory);
        const ulong guildId = 42ul;

        var playlistId = await playlistRepository.CreatePlaylistAsync(guildId, "mix");
        await trackRepository.AddRangeAsync(playlistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a", "https://example.com/a"),
            new PlaylistTrackCreateRecord("YouTube", "track-b", "https://example.com/b")
        ]);

        var summaries = await playlistRepository.GetByGuildAsync(guildId);
        Assert.Single(summaries);
        Assert.Equal("mix", summaries[0].Name);
        Assert.Equal(2, summaries[0].TrackCount);

        Assert.True(await playlistRepository.RenameAsync(guildId, "mix", "renamed"));
        Assert.Null(await playlistRepository.GetByGuildAndNameAsync(guildId, "mix"));
        Assert.NotNull(await playlistRepository.GetByGuildAndNameAsync(guildId, "renamed"));

        Assert.True(await playlistRepository.DeleteByGuildAndNameAsync(guildId, "renamed"));
        Assert.Empty(await playlistRepository.GetByGuildAsync(guildId));
        Assert.Empty(await trackRepository.GetByPlaylistIdOrderedAsync(playlistId));
    }

    [Fact]
    public async Task PlaylistTrackRepository_RemoveTrack_ReordersWithoutUniqueConstraintCollision()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var factory = services.GetRequiredService<IDbContextFactory<BotDbContext>>();
        var playlistRepository = new PlaylistRepository(factory);
        var trackRepository = new PlaylistTrackRepository(factory);
        var playlistId = await playlistRepository.CreatePlaylistAsync(84ul, "mix");
        await trackRepository.AddRangeAsync(playlistId,
        [
            new PlaylistTrackCreateRecord("YouTube", "track-a", "https://example.com/a"),
            new PlaylistTrackCreateRecord("YouTube", "track-b", "https://example.com/b"),
            new PlaylistTrackCreateRecord("YouTube", "track-c", "https://example.com/c")
        ]);

        await trackRepository.RemoveTrackAsync(playlistId, 2);

        var tracks = await trackRepository.GetByPlaylistIdOrderedAsync(playlistId);
        Assert.Equal(["track-a", "track-c"], tracks.Select(track => track.TrackIdentifier));
        Assert.Equal([1, 2], tracks.Select(track => track.OrderNumber));
    }

    [Fact]
    public async Task PlaylistRepository_CreateDuplicateNameInSameGuild_ThrowsDbUpdateException()
    {
        var database = await PostgreSqlTestDatabase.TryCreateAsync();
        if (database is null) return;
        await using var _ = database;
        await database.MigrateAsync();
        await using var services = database.CreateServiceProvider();
        var repository = new PlaylistRepository(services.GetRequiredService<IDbContextFactory<BotDbContext>>());
        const ulong guildId = 168ul;

        await repository.CreatePlaylistAsync(guildId, "mix");

        await Assert.ThrowsAsync<DbUpdateException>(() => repository.CreatePlaylistAsync(guildId, "mix"));
    }
}
