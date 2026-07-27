using DC_bot.Commands.TextCommands.Playlist;
using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Music.PlaylistServiceInterface.Models;
using DC_bot.Interface.Service.Presentation;
using DC_bot_tests.TestHelperFiles;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Commands.TextCommands.Playlist;

[Trait("Category", "Unit")]
public class LoadPlaylistCommandTests : PlaylistCommandTestBase
{
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerServiceMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionServiceMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackServiceMock = new();
    private readonly Mock<ITrackSerializer> _trackSerializerMock = new();
    private readonly Mock<IMusicQueueService> _musicQueueServiceMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();

    public LoadPlaylistCommandTests()
    {
        _voiceChannelMock.SetupGet(channel => channel.Id).Returns(999ul);
        _voiceChannelMock.SetupGet(channel => channel.Name).Returns("voice");
        _voiceChannelMock.SetupGet(channel => channel.Guild).Returns(MessageMock.Object.Channel.Guild);

        var voiceStateMock = new Mock<IDiscordVoiceState>();
        voiceStateMock.SetupGet(state => state.Channel).Returns(_voiceChannelMock.Object);

        var memberMock = new Mock<IDiscordMember>();
        memberMock.SetupGet(member => member.VoiceState).Returns(voiceStateMock.Object);

        CommandHelperMock
            .Setup(helper => helper.TryValidateUserAsync(
                It.IsAny<IUserValidationService>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<IDiscordMessage>()))
            .ReturnsAsync(new UserValidationResult(true, string.Empty, memberMock.Object));

        _playerConnectionServiceMock
            .Setup(service => service.TryJoinAndValidateAsync(
                MessageMock.Object,
                _voiceChannelMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));
    }

    [Fact]
    public async Task ExecuteAsync_WhenLoadedAndPlayerIdle_EnqueuesTracksRegistersHandlerAndStartsPlayback()
    {
        var firstTrack = TrackTestHelper.CreateTrackWrapper("Artist A", "Title A", "track-a");
        var secondTrack = TrackTestHelper.CreateTrackWrapper("Artist B", "Title B", "track-b");
        SetupLoadedPlaylist("serialized-a", "serialized-b");
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("serialized-a", null)).Returns(firstTrack);
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("serialized-b", null)).Returns(secondTrack);

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        PlaylistServiceMock.Verify(service => service.LoadPlaylistAsync(GuildId, PlaylistName), Times.Once);
        _playerConnectionServiceMock.Verify(service => service.TryJoinAndValidateAsync(
            MessageMock.Object,
            _voiceChannelMock.Object,
            It.IsAny<CancellationToken>()), Times.Once);
        _playbackEventHandlerServiceMock.Verify(service => service.RegisterPlaybackFinishedHandler(
            GuildId,
            _playerMock.Object,
            MessageMock.Object.Channel), Times.Once);
        _musicQueueServiceMock.Verify(service => service.EnqueueMany(
            GuildId,
            It.Is<IReadOnlyCollection<ILavaLinkTrack>>(tracks =>
                tracks.Count == 2 &&
                tracks.ElementAt(0) == firstTrack &&
                tracks.ElementAt(1) == secondTrack)), Times.Once);
        _trackPlaybackServiceMock.Verify(service => service.TryPlayNextTrackAsync(
            _playerMock.Object,
            MessageMock.Object.Channel,
            GuildId), Times.Once);
        ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
            MessageMock.Object,
            LocalizationKeys.LoadPlaylistCommandLoaded,
            It.Is<object[]>(args => (string)args[0] == PlaylistName && (int)args[1] == 2)), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenPlayerAlreadyPlaying_OnlyQueuesPlaylist()
    {
        var queuedTrack = TrackTestHelper.CreateTrackWrapper("Artist", "Title", "track-id");
        var currentTrack = TrackTestHelper.CreateTrackWrapper("Current Artist", "Current Title", "current-id");
        _playerMock.Setup(player => player.CurrentTrack).Returns(currentTrack.ToLavalinkTrack());
        SetupLoadedPlaylist("serialized-a");
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("serialized-a", null)).Returns(queuedTrack);

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        _musicQueueServiceMock.Verify(service => service.EnqueueMany(
            GuildId,
            It.Is<IReadOnlyCollection<ILavaLinkTrack>>(tracks => tracks.Single() == queuedTrack)), Times.Once);
        _trackPlaybackServiceMock.Verify(service => service.TryPlayNextTrackAsync(
            It.IsAny<ILavalinkPlayer>(),
            It.IsAny<IDiscordChannel>(),
            It.IsAny<ulong>()), Times.Never);
    }

    [Theory]
    [InlineData(LoadPlaylistStatus.NotFound, "warning", LocalizationKeys.LoadPlaylistCommandNotFound)]
    [InlineData(LoadPlaylistStatus.EmptyPlaylist, "warning", LocalizationKeys.LoadPlaylistCommandEmptyPlaylist)]
    [InlineData(LoadPlaylistStatus.InvalidPlaylistName, "warning", LocalizationKeys.LoadPlaylistCommandInvalidPlaylistName)]
    [InlineData(LoadPlaylistStatus.UnknownError, "error", LocalizationKeys.LoadPlaylistCommandUnknownError)]
    public async Task ExecuteAsync_SendsExpectedNonSuccessResponse(
        LoadPlaylistStatus status,
        string responseKind,
        string expectedKey)
    {
        PlaylistServiceMock.Setup(service => service.LoadPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new LoadPlaylistResult(status, null));

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        VerifyResponse(responseKind, expectedKey);
        _playerConnectionServiceMock.Verify(service => service.TryJoinAndValidateAsync(
            It.IsAny<IDiscordMessage>(),
            It.IsAny<IDiscordChannel?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _musicQueueServiceMock.Verify(service => service.EnqueueMany(
            It.IsAny<ulong>(),
            It.IsAny<IReadOnlyCollection<ILavaLinkTrack>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoredTrackCannotBeDeserialized_SendsUnknownErrorAndDoesNotJoin()
    {
        SetupLoadedPlaylist("bad-track");
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("bad-track", null))
            .Throws(new FormatException("bad track"));

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        VerifyResponse("error", LocalizationKeys.LoadPlaylistCommandUnknownError);
        _playerConnectionServiceMock.Verify(service => service.TryJoinAndValidateAsync(
            It.IsAny<IDiscordMessage>(),
            It.IsAny<IDiscordChannel?>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _musicQueueServiceMock.Verify(service => service.EnqueueMany(
            It.IsAny<ulong>(),
            It.IsAny<IReadOnlyCollection<ILavaLinkTrack>>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenJoinValidationFails_DoesNotQueuePlaylist()
    {
        SetupLoadedPlaylist("serialized-a");
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("serialized-a", null))
            .Returns(TrackTestHelper.CreateTrackWrapper());
        _playerConnectionServiceMock
            .Setup(service => service.TryJoinAndValidateAsync(
                MessageMock.Object,
                _voiceChannelMock.Object,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((null, null, 0ul, false));

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        _musicQueueServiceMock.Verify(service => service.EnqueueMany(
            It.IsAny<ulong>(),
            It.IsAny<IReadOnlyCollection<ILavaLinkTrack>>()), Times.Never);
        ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
            It.IsAny<IDiscordMessage>(),
            It.IsAny<string>(),
            It.IsAny<object[]>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_EscapesMentionsInLoadedResponseButUsesRawNameForLookup()
    {
        CommandHelperMock
            .Setup(helper => helper.TryGetArgumentAsync(
                It.IsAny<IDiscordMessage>(),
                It.IsAny<IResponseBuilder>(),
                It.IsAny<ILogger>(),
                It.IsAny<string>()))
            .ReturnsAsync("@everyone");
        PlaylistServiceMock.Setup(service => service.LoadPlaylistAsync(GuildId, "@everyone"))
            .ReturnsAsync(new LoadPlaylistResult(
                LoadPlaylistStatus.Loaded,
                new PlaylistDto("@everyone", [new PlaylistTrackDto(1, "youtube", "serialized-a", "uri-a")])));
        _trackSerializerMock.Setup(serializer => serializer.Deserialize("serialized-a", null))
            .Returns(TrackTestHelper.CreateTrackWrapper());

        await CreateCommand().ExecuteAsync(MessageMock.Object);

        PlaylistServiceMock.Verify(service => service.LoadPlaylistAsync(GuildId, "@everyone"), Times.Once);
        ResponseBuilderMock.Verify(response => response.SendSuccessAsync(
            MessageMock.Object,
            LocalizationKeys.LoadPlaylistCommandLoaded,
            It.Is<object[]>(args => (string)args[0] == "@\u200Beveryone")), Times.Once);
    }

    [Fact]
    public void NameAndDescription_ReturnExpectedValues()
    {
        var command = CreateCommand();

        Assert.Equal("loadPlaylist", command.Name);
        Assert.False(string.IsNullOrWhiteSpace(command.Description));
    }

    private void SetupLoadedPlaylist(params string[] trackIdentifiers)
    {
        var tracks = trackIdentifiers
            .Select((identifier, index) => new PlaylistTrackDto(index + 1, "youtube", identifier, $"uri-{index}"))
            .Reverse()
            .ToList();

        PlaylistServiceMock.Setup(service => service.LoadPlaylistAsync(GuildId, PlaylistName))
            .ReturnsAsync(new LoadPlaylistResult(LoadPlaylistStatus.Loaded, new PlaylistDto(PlaylistName, tracks)));
    }

    private LoadPlaylistCommand CreateCommand()
    {
        return new LoadPlaylistCommand(
            Mock.Of<ILogger<LoadPlaylistCommand>>(),
            Mock.Of<IUserValidationService>(),
            ResponseBuilderMock.Object,
            LocalizationService,
            PlaylistServiceMock.Object,
            _musicQueueServiceMock.Object,
            _trackSerializerMock.Object,
            _playerConnectionServiceMock.Object,
            _playbackEventHandlerServiceMock.Object,
            _trackPlaybackServiceMock.Object,
            CommandHelperMock.Object);
    }
}
