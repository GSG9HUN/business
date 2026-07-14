using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface;
using DC_bot.Interface.Service.Presentation;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

public abstract class PlaylistCommandTestBase
{
    protected const ulong GuildId = 123ul;
    protected const string PlaylistName = "mix";
    protected const string NewPlaylistName = "renamed";
    protected const string PlaylistUrl = "https://example.com/playlist";
    protected const string SongUrl = "https://example.com/song";
    protected const int TrackNumber = 1;

    protected readonly Mock<ICommandHelper> CommandHelperMock = new();
    protected readonly Mock<IDiscordMessage> MessageMock = new();
    protected readonly Mock<IPlaylistService> PlaylistServiceMock = new();
    protected readonly Mock<IResponseBuilder> ResponseBuilderMock = new();
    protected readonly ILocalizationService LocalizationService = CreateLocalizationService();

    protected PlaylistCommandTestBase()
    {
        var guildMock = new Mock<IDiscordGuild>();
        guildMock.SetupGet(guild => guild.Id).Returns(GuildId);

        var channelMock = new Mock<IDiscordChannel>();
        channelMock.SetupGet(channel => channel.Guild).Returns(guildMock.Object);

        MessageMock.SetupGet(message => message.Channel).Returns(channelMock.Object);

        CommandHelperMock
            .Setup(helper => helper.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, Mock.Of<IDiscordMember>()));

        CommandHelperMock
            .Setup(helper => helper.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .ReturnsAsync(PlaylistName);

        CommandHelperMock
            .Setup(helper => helper.TryParseSavePlaylistArguments(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.Is<string>(name => name == "renamePlaylist")))
            .ReturnsAsync((PlaylistName, NewPlaylistName));

        CommandHelperMock
            .Setup(helper => helper.TryParseSavePlaylistArguments(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.Is<string>(name => name == "savePlaylist")))
            .ReturnsAsync((PlaylistName, PlaylistUrl));

        CommandHelperMock
            .Setup(helper => helper.TryParseSavePlaylistArguments(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.Is<string>(name => name == "addSong")))
            .ReturnsAsync((PlaylistName, SongUrl));

        CommandHelperMock
            .Setup(helper => helper.TryParseSavePlaylistArguments(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.Is<string>(name => name == "removeSong")))
            .ReturnsAsync((PlaylistName, TrackNumber.ToString()));
    }

    protected void VerifyResponse(string responseKind, string expectedKey)
    {
        switch (responseKind)
        {
            case "success":
                ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
                    MessageMock.Object,
                    expectedKey,
                    It.IsAny<object[]>()),
                    Times.Once);
                break;
            case "warning":
                ResponseBuilderMock.Verify(response => response.SendWarningAsync(
                    MessageMock.Object,
                    expectedKey,
                    It.IsAny<object[]>()),
                    Times.Once);
                break;
            case "error":
                ResponseBuilderMock.Verify(response => response.SendErrorAsync(
                    MessageMock.Object,
                    expectedKey,
                    It.IsAny<object[]>()),
                    Times.Once);
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(responseKind), responseKind, null);
        }
    }

    private static ILocalizationService CreateLocalizationService()
    {
        var localization = new Mock<ILocalizationService>();
        localization.Setup(service => service.Get(It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<string, object[]>(FormatLocalization);
        localization.Setup(service => service.Get(It.IsAny<ulong>(), It.IsAny<string>(), It.IsAny<object[]>()))
            .Returns<ulong, string, object[]>((_, key, args) => FormatLocalization(key, args));
        return localization.Object;
    }

    private static string FormatLocalization(string key, object[] args)
    {
        return key switch
        {
            LocalizationKeys.ListPlaylistsCommandResponse => $"Saved playlists:{Environment.NewLine}{args[0]}",
            LocalizationKeys.ListPlaylistsCommandItem => $"{args[0]}. {args[1]} - {args[2]} tracks",
            LocalizationKeys.ViewPlaylistCommandResponse => $"Playlist '{args[0]}' ({args[1]} tracks):{Environment.NewLine}{args[2]}",
            LocalizationKeys.ViewPlaylistCommandTrack => $"{args[0]}. {args[1]} - {args[2]} ({args[3]})",
            LocalizationKeys.ViewPlaylistCommandMoreTracks => $"... and {args[0]} more tracks",
            _ => args.Length == 0 ? key : $"{key}:{string.Join("|", args)}"
        };
    }
}
