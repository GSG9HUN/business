using DC_bot.Commands.SlashCommands.Playlist;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.SlashCommands;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.SlashCommands.Playlist;

[Trait("Category", "Unit")]
public class PlaylistSlashCommandTests : SlashCommandTestBase
{
    private const string PlaylistName = "road trip";
    private const string RenamedPlaylistName = "renamed mix";
    private const string PlaylistUrl = "https://example.com/playlist";
    private const string SongUrl = "https://example.com/song";

    [Fact]
    public async Task Create_ShouldCreateInteractionContextAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Create(dsharpContext, PlaylistName);

        contextFactory.Verify(x => x.Create(dsharpContext), Times.Once);
        VerifyRequest(
            executor,
            "createPlaylist",
            slashContext.Object,
            PlaylistName,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task Save_ShouldQuoteNameAndRequireDeferredResponse()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Save(dsharpContext, PlaylistName, PlaylistUrl);

        VerifyRequest(
            executor,
            "savePlaylist",
            slashContext.Object,
            "\"road trip\" https://example.com/playlist",
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task List_ShouldDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.List(dsharpContext);

        VerifyRequest(
            executor,
            "listPlaylists",
            slashContext.Object,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task View_ShouldDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.View(dsharpContext, PlaylistName);

        VerifyRequest(
            executor,
            "viewPlaylist",
            slashContext.Object,
            PlaylistName,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task AddSong_ShouldQuoteNameAndRequireDeferredResponse()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.AddSong(dsharpContext, PlaylistName, SongUrl);

        VerifyRequest(
            executor,
            "addSong",
            slashContext.Object,
            "\"road trip\" https://example.com/song",
            requireGuild: true,
            defer: true,
            ensureDeferredResponse: true);
    }

    [Fact]
    public async Task RemoveSong_ShouldQuoteNameAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.RemoveSong(dsharpContext, PlaylistName, 2);

        VerifyRequest(
            executor,
            "removeSong",
            slashContext.Object,
            "\"road trip\" 2",
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task Rename_ShouldQuoteBothNamesAndDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Rename(dsharpContext, PlaylistName, RenamedPlaylistName);

        VerifyRequest(
            executor,
            "renamePlaylist",
            slashContext.Object,
            "\"road trip\" \"renamed mix\"",
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task Delete_WhenConfirmed_ShouldDelegateToExecutor()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new Mock<ISlashInteractionContext>();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext.Object);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Delete(dsharpContext, PlaylistName, confirm: true);

        VerifyRequest(
            executor,
            "deletePlaylist",
            slashContext.Object,
            PlaylistName,
            requireGuild: true,
            defer: true);
    }

    [Fact]
    public async Task Delete_WhenNotConfirmed_ShouldReturnConfirmationMessageWithoutExecuting()
    {
        var dsharpContext = CreateDSharpContext();
        var slashContext = new TestSlashInteractionContext();
        var executor = CreateModuleExecutor();
        var contextFactory = CreateContextFactory(dsharpContext, slashContext);
        var command = new PlaylistSlashCommand(executor.Object, contextFactory.Object, LocalizationService);

        await command.Delete(dsharpContext, "@everyone", confirm: false);

        Assert.Contains("Set confirm to true to delete playlist '@\u200Beveryone'.", slashContext.TextResponses);
        executor.Verify(
            x => x.ExecuteAsync(It.IsAny<SlashCommandExecutionRequest>()),
            Times.Never);
    }

    [Fact]
    public async Task ExecuteSaveAsync_WithSpacedName_ShouldRouteThroughTextParser()
    {
        var context = CreateContext();
        var command = new PlaylistSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>(), LocalizationService);
        PlaylistServiceMock
            .Setup(service => service.SavePlaylistAsync(123UL, PlaylistName, PlaylistUrl))
            .ReturnsAsync(SavePlaylistResult.Saved);

        await command.ExecuteSaveAsync(context, PlaylistName, PlaylistUrl);

        Assert.True(context.IsDeferred);
        Assert.Contains("Playlist 'road trip' saved.", context.TextResponses);
        PlaylistServiceMock.Verify(
            service => service.SavePlaylistAsync(123UL, PlaylistName, PlaylistUrl),
            Times.Once);
    }

    [Fact]
    public async Task ExecuteAddSongAsync_WithSpacedName_ShouldRouteThroughTextParser()
    {
        var context = CreateContext();
        var command = new PlaylistSlashCommand(SlashCommandExecutor, Mock.Of<ISlashInteractionContextFactory>(), LocalizationService);
        PlaylistServiceMock
            .Setup(service => service.AddSongToPlaylistAsync(123UL, PlaylistName, SongUrl))
            .ReturnsAsync(AddSongResult.Added);

        await command.ExecuteAddSongAsync(context, PlaylistName, SongUrl);

        Assert.True(context.IsDeferred);
        Assert.Contains("Song added to playlist 'road trip'.", context.TextResponses);
        PlaylistServiceMock.Verify(
            service => service.AddSongToPlaylistAsync(123UL, PlaylistName, SongUrl),
            Times.Once);
    }
}
