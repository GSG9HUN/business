using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class ListPlaylistsCommandTests : PlaylistCommandTestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenListed_FormatsPlaylistSummaries()
    {
        PlaylistServiceMock.Setup(service => service.ListPlaylistsAsync(GuildId))
            .ReturnsAsync(new ListPlaylistsResult(
                ListPlaylistsStatus.Listed,
                [
                    new PlaylistSummaryDto("alpha", 2),
                    new PlaylistSummaryDto("beta", 5)
                ]));
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
            MessageMock.Object,
            LocalizationKeys.ListPlaylistsCommandResponse,
            It.Is<object[]>(args =>
                args.Length == 1 &&
                args[0].ToString()!.Contains("1. alpha - 2 tracks", StringComparison.Ordinal) &&
                args[0].ToString()!.Contains("2. beta - 5 tracks", StringComparison.Ordinal))),
            Times.Once);
    }

    [Theory]
    [InlineData(ListPlaylistsStatus.NoPlaylists, "warning", LocalizationKeys.ListPlaylistsCommandNoPlaylists)]
    [InlineData(ListPlaylistsStatus.UnknownError, "error", LocalizationKeys.ListPlaylistsCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedNonSuccessResponse(
        ListPlaylistsStatus status,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.ListPlaylistsAsync(GuildId))
            .ReturnsAsync(new ListPlaylistsResult(status, []));
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("listPlaylists", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private ListPlaylistsCommand CreateCommand()
    {
        return new ListPlaylistsCommand(
            Mock.Of<ILogger<ListPlaylistsCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
