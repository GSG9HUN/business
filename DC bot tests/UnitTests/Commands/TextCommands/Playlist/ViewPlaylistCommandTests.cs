using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Interface.Core;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class ViewPlaylistCommandTests : PlaylistCommandTestBase
{
    [Fact]
    public async Task ExecuteAsync_WhenViewed_FormatsFirstTenTracksAndOverflowCount()
    {
        var tracks = Enumerable.Range(1, 11)
            .Select(index => new PlaylistViewTrackDto(
                index,
                $"Title {index}",
                $"Author {index}",
                TimeSpan.FromSeconds(60 + index),
                $"uri-{index}"))
            .ToList();
        PlaylistServiceMock.Setup(service => service.ViewPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new ViewPlaylistResult(ViewPlaylistStatus.Viewed, PlaylistName, tracks));
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
            MessageMock.Object,
            LocalizationKeys.ViewPlaylistCommandResponse,
            It.Is<object[]>(args =>
                args.Length == 3 &&
                (string)args[0] == PlaylistName &&
                (int)args[1] == 11 &&
                args[2].ToString()!.Contains("1. Author 1 - Title 1 (1:01)", StringComparison.Ordinal) &&
                args[2].ToString()!.Contains("10. Author 10 - Title 10 (1:10)", StringComparison.Ordinal) &&
                args[2].ToString()!.Contains("... and 1 more tracks", StringComparison.Ordinal) &&
                !args[2].ToString()!.Contains("Title 11", StringComparison.Ordinal))),
            Times.Once);
    }

    [Theory]
    [InlineData(ViewPlaylistStatus.PlaylistDoesNotExist, "warning", LocalizationKeys.ViewPlaylistCommandPlaylistDoesNotExist)]
    [InlineData(ViewPlaylistStatus.EmptyPlaylist, "warning", LocalizationKeys.ViewPlaylistCommandEmptyPlaylist)]
    [InlineData(ViewPlaylistStatus.UnknownError, "error", LocalizationKeys.ViewPlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedNonSuccessResponse(
        ViewPlaylistStatus status,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.ViewPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new ViewPlaylistResult(status, PlaylistName, []));
        var command = CreateCommand();

        await command.ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("viewPlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private ViewPlaylistCommand CreateCommand()
    {
        return new ViewPlaylistCommand(
            Mock.Of<ILogger<ViewPlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            CommandHelperMock.Object);
    }
}
