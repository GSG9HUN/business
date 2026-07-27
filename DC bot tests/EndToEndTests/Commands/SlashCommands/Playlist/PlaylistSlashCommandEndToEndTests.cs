using DC_bot.Commands.SlashCommands.Playlist;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.EndToEndTests.Commands.SlashCommands.Playlist;

[Collection("E2E Tests")]
[Trait("Category", "E2E")]
public class PlaylistSlashCommandEndToEndTests : SlashCommandPipelineEndToEndTestBase
{
    private const ulong GuildId = 123UL;
    private const string PlaylistName = "road trip";
    private const string RenamedPlaylistName = "renamed mix";
    private const string PlaylistUrl = "https://example.com/playlist";
    private const string SongUrl = "https://example.com/song";

    [Fact]
    public async Task PlaylistSlashCommands_ShouldRunThroughTextCommandPipeline()
    {
        PlaylistServiceMock
            .Setup(service => service.CreatePlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(CreatePlaylistResult.Created);
        PlaylistServiceMock
            .Setup(service => service.SavePlaylistAsync(GuildId, PlaylistName, PlaylistUrl))
            .ReturnsAsync(SavePlaylistResult.Saved);
        PlaylistServiceMock
            .Setup(service => service.ListPlaylistsAsync(GuildId))
            .ReturnsAsync(new ListPlaylistsResult(
                ListPlaylistsStatus.Listed,
                [new PlaylistSummaryDto(PlaylistName, 1)]));
        PlaylistServiceMock
            .Setup(service => service.ViewPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new ViewPlaylistResult(
                ViewPlaylistStatus.Viewed,
                PlaylistName,
                [new PlaylistViewTrackDto(1, "Song", "Artist", TimeSpan.FromSeconds(95), SongUrl)]));
        PlaylistServiceMock
            .Setup(service => service.AddSongToPlaylistAsync(GuildId, PlaylistName, SongUrl))
            .ReturnsAsync(AddSongResult.Added);
        PlaylistServiceMock
            .Setup(service => service.RemoveSongFromPlaylistAsync(GuildId, PlaylistName, 1))
            .ReturnsAsync(RemoveSongResult.Removed);
        PlaylistServiceMock
            .Setup(service => service.RenamePlaylistAsync(GuildId, PlaylistName, RenamedPlaylistName))
            .ReturnsAsync(RenamePlaylistResult.Renamed);
        PlaylistServiceMock
            .Setup(service => service.DeletePlaylistAsync(GuildId, RenamedPlaylistName))
            .ReturnsAsync(DeletePlaylistResult.Deleted);

        var context = CreateContext();
        var command = new PlaylistSlashCommand(
            SlashCommandExecutor,
            Mock.Of<ISlashInteractionContextFactory>(),
            LocalizationServiceMock.Object);

        await command.ExecuteCreateAsync(context, PlaylistName);
        await command.ExecuteSaveAsync(context, PlaylistName, PlaylistUrl);
        await command.ExecuteListAsync(context);
        await command.ExecuteViewAsync(context, PlaylistName);
        await command.ExecuteAddSongAsync(context, PlaylistName, SongUrl);
        await command.ExecuteRemoveSongAsync(context, PlaylistName, 1);
        await command.ExecuteRenameAsync(context, PlaylistName, RenamedPlaylistName);
        await command.ExecuteDeleteAsync(context, RenamedPlaylistName, confirm: true);

        Assert.True(context.IsDeferred);
        Assert.Contains("Playlist 'road trip' created.", context.TextResponses);
        Assert.Contains("Playlist 'road trip' saved.", context.TextResponses);
        Assert.Contains(context.TextResponses, response =>
            response.Contains("Saved playlists:", StringComparison.Ordinal) &&
            response.Contains("1. road trip - 1 tracks", StringComparison.Ordinal));
        Assert.Contains(context.TextResponses, response =>
            response.Contains("Playlist 'road trip' (1 tracks):", StringComparison.Ordinal) &&
            response.Contains("1. Artist - Song (1:35)", StringComparison.Ordinal));
        Assert.Contains("Song added to playlist 'road trip'.", context.TextResponses);
        Assert.Contains("Track 1 removed from playlist 'road trip'.", context.TextResponses);
        Assert.Contains("Playlist 'road trip' renamed to 'renamed mix'.", context.TextResponses);
        Assert.Contains("Playlist 'renamed mix' deleted.", context.TextResponses);
    }

    [Fact]
    public async Task DeleteSlashCommand_WhenNotConfirmed_ShouldNotExecuteTextCommand()
    {
        var context = CreateContext();
        var command = new PlaylistSlashCommand(
            SlashCommandExecutor,
            Mock.Of<ISlashInteractionContextFactory>(),
            LocalizationServiceMock.Object);

        await command.ExecuteDeleteAsync(context, "@everyone", confirm: false);

        Assert.False(context.IsDeferred);
        Assert.Contains("Set confirm to true to delete playlist '@\u200Beveryone'.", context.TextResponses);
        PlaylistServiceMock.Verify(
            service => service.DeletePlaylistAsync(It.IsAny<ulong>(), It.IsAny<string>()),
            Times.Never);
    }
}
