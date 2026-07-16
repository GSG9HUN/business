using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Persistence.Models;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.Playlist;

[Trait("Category", "Unit")]
public class PlaylistServiceMutationTests : PlaylistServiceTestBase
{
    [Fact]
    public async Task DeletePlaylistAsync_WhenPlaylistDoesNotExist_ReturnsDoesNotExist()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((PlaylistRecord?)null);

        var result = await context.Service.DeletePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(DeletePlaylistResult.DoesNotExist, result);
    }

    [Fact]
    public async Task DeletePlaylistAsync_WhenRepositoryDeletes_ReturnsDeleted()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.DeleteByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.DeletePlaylistAsync(GuildId, PlaylistName);

        Assert.Equal(DeletePlaylistResult.Deleted, result);
    }

    [Fact]
    public async Task RenamePlaylistAsync_WhenNewNameExists_ReturnsPlaylistAlreadyExists()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(
                GuildId,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.RenamePlaylistAsync(GuildId, PlaylistName, NewPlaylistName);

        Assert.Equal(RenamePlaylistResult.PlaylistAlreadyExists, result);
    }

    [Fact]
    public async Task RenamePlaylistAsync_WhenRepositoryRenames_ReturnsRenamed()
    {
        var context = CreateContext();
        context.PlaylistRepository.Setup(repository => repository.GetByGuildAndNameAsync(
                GuildId,
                PlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new PlaylistRecord(PlaylistId, GuildId, PlaylistName));
        context.PlaylistRepository.Setup(repository => repository.ExistsAsync(
                GuildId,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        context.PlaylistRepository.Setup(repository => repository.RenameAsync(
                GuildId,
                PlaylistName,
                NewPlaylistName,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var result = await context.Service.RenamePlaylistAsync(GuildId, PlaylistName, NewPlaylistName);

        Assert.Equal(RenamePlaylistResult.Renamed, result);
    }
}
