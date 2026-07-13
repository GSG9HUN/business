using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class RenamePlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(RenamePlaylistResult.Renamed, "success", LocalizationKeys.RenamePlaylistCommandRenamed)]
    [InlineData(RenamePlaylistResult.PlaylistDoesNotExist, "warning", LocalizationKeys.RenamePlaylistCommandPlaylistDoesNotExist)]
    [InlineData(RenamePlaylistResult.PlaylistAlreadyExists, "warning", LocalizationKeys.RenamePlaylistCommandPlaylistAlreadyExists)]
    [InlineData(RenamePlaylistResult.InvalidPlaylistName, "warning", LocalizationKeys.RenamePlaylistCommandInvalidPlaylistName)]
    [InlineData(RenamePlaylistResult.UnknownError, "error", LocalizationKeys.RenamePlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        RenamePlaylistResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.RenamePlaylistAsync(GuildId, PlaylistName, NewPlaylistName))
            .ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("renamePlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private RenamePlaylistCommand CreateCommand()
    {
        return new RenamePlaylistCommand(
            Mock.Of<ILogger<RenamePlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
