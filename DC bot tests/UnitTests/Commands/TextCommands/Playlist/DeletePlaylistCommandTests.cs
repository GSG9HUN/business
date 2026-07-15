using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class DeletePlaylistCommandTests : PlaylistCommandTestBase
{
    [Theory]
    [InlineData(DeletePlaylistResult.Deleted, "success", LocalizationKeys.DeletePlaylistCommandDeleted)]
    [InlineData(DeletePlaylistResult.DoesNotExist, "warning", LocalizationKeys.DeletePlaylistCommandDoesNotExist)]
    [InlineData(DeletePlaylistResult.UnknownError, "error", LocalizationKeys.DeletePlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedResponse(
        DeletePlaylistResult result,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.DeletePlaylistAsync(GuildId, PlaylistName)).ReturnsAsync(result);
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("deletePlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private DeletePlaylistCommand CreateCommand()
    {
        return new DeletePlaylistCommand(
            Mock.Of<ILogger<DeletePlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
