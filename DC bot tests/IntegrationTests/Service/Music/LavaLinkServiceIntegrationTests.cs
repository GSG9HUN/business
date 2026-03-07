using DC_bot.Exceptions.Music;
using DC_bot.Interface;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music.MusicServiceInterface;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music;
using DC_bot.Service.Music.MusicServices;
using DC_bot_tests.Helpers;
using Lavalink4NET;
using Lavalink4NET.Players;
using Lavalink4NET.Tracks;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.IntegrationTests.Service.Music;

public class LavaLinkServiceIntegrationTests
{
    private const ulong GuildId = 555UL;
    private const ulong VoiceChannelId = 777UL;

    private readonly MusicQueueService _queueService;
    private readonly RepeatService _repeatService;
    private readonly CurrentTrackService _currentTrackService;

    private readonly Mock<ILogger<LavaLinkService>> _loggerMock = new();
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly Mock<ILocalizationService> _localizationMock = new();
    private readonly Mock<ITrackNotificationService> _trackNotificationMock = new();
    private readonly Mock<IPlayerConnectionService> _playerConnectionMock = new();
    private readonly Mock<IPlaybackEventHandlerService> _playbackEventHandlerMock = new();
    private readonly Mock<ITrackPlaybackService> _trackPlaybackMock = new();

    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<IDiscordChannel> _voiceChannelMock = new();
    private readonly Mock<IDiscordChannel> _textChannelMock = new();
    private readonly Mock<IDiscordVoiceState> _voiceStateMock = new();
    private readonly Mock<IDiscordMember> _memberMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<ILavalinkPlayer> _playerMock = new();

    private readonly LavaLinkService _service;

    public LavaLinkServiceIntegrationTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _queueService = new MusicQueueService(new InMemoryFileSystem());
        _repeatService = new RepeatService();
        _currentTrackService = new CurrentTrackService();

        _guildMock.Setup(g => g.Id).Returns(GuildId);
        _voiceChannelMock.Setup(c => c.Id).Returns(VoiceChannelId);
        _voiceChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _voiceChannelMock.Setup(c => c.Name).Returns("voice");

        _textChannelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _textChannelMock.Setup(c => c.Name).Returns("text");

        _voiceStateMock.Setup(v => v.Channel).Returns(_voiceChannelMock.Object);
        _memberMock.Setup(m => m.VoiceState).Returns(_voiceStateMock.Object);

        _messageMock.SetupProperty(m => m.Channel, _textChannelMock.Object);

        _service = new LavaLinkService(
            _queueService,
            _loggerMock.Object,
            _audioServiceMock.Object,
            _responseBuilderMock.Object,
            _localizationMock.Object,
            _repeatService,
            _currentTrackService,
            _trackNotificationMock.Object,
            _playerConnectionMock.Object,
            _playbackEventHandlerMock.Object,
            _trackPlaybackMock.Object);
    }

    [Fact]
    public void Init_InitializesCurrentTrackAndRepeatState()
    {
        _service.Init(GuildId);

        Assert.Null(_currentTrackService.GetCurrentTrack(GuildId));
        Assert.False(_repeatService.IsRepeating(GuildId));
        Assert.False(_repeatService.IsRepeatingList(GuildId));
    }

    [Fact]
    public async Task ConnectAsync_WhenCalledTwice_StartsAudioOnlyOnce()
    {
        _audioServiceMock.Setup(a => a.StartAsync(default)).Returns(new ValueTask());

        await _service.ConnectAsync();
        await _service.ConnectAsync();

        _audioServiceMock.Verify(a => a.StartAsync(default), Times.Once);
    }

    [Fact]
    public async Task ConnectAsync_WhenAudioStartFails_ThrowsLavalinkOperationException()
    {
        _audioServiceMock.Setup(a => a.StartAsync(default)).ThrowsAsync(new InvalidOperationException("boom"));

        await Assert.ThrowsAsync<LavalinkOperationException>(() => _service.ConnectAsync());
    }

    [Fact]
    public async Task StartPlayingQueue_WithQueuedTrack_PlaysNotifiesAndSetsCurrentTrack()
    {
        _service.Init(GuildId);
        var queuedTrack = CreateTrack("Artist A", "Title A", "id-a");
        _queueService.Enqueue(GuildId, new FakeQueueTrack(queuedTrack));

        _playerConnectionMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playbackEventHandlerMock.Verify(h => h.RegisterPlaybackFinishedHandler(GuildId, _playerMock.Object, _textChannelMock.Object), Times.Once);
        _playerMock.Verify(p => p.PlayAsync(It.Is<LavalinkTrack>(t => t.Title == "Title A"), It.IsAny<TrackPlayProperties>(), default), Times.Once);
        _trackNotificationMock.Verify(n => n.NotifyNowPlayingAsync(_textChannelMock.Object, It.Is<LavalinkTrack>(t => t.Title == "Title A")), Times.Once);

        var current = _currentTrackService.GetCurrentTrack(GuildId);
        Assert.NotNull(current);
        Assert.Equal("Title A", current.Title);
    }

    [Fact]
    public async Task StartPlayingQueue_WhenQueueEmpty_DoesNotPlay()
    {
        _service.Init(GuildId);

        _playerConnectionMock
            .Setup(p => p.TryJoinAndValidateAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.StartPlayingQueue(_messageMock.Object, _textChannelMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.PlayAsync(It.IsAny<LavalinkTrack>(), It.IsAny<TrackPlayProperties>(), default), Times.Never);
        _trackNotificationMock.Verify(n => n.NotifyNowPlayingAsync(It.IsAny<IDiscordChannel>(), It.IsAny<LavalinkTrack>()), Times.Never);
    }

    [Fact]
    public async Task LeaveVoiceChannel_WhenCurrentTrackExists_StopsCleansAndDisconnects()
    {
        _playerMock.Setup(p => p.CurrentTrack).Returns(CreateTrack("Artist", "Current", "id-current"));

        _playerConnectionMock
            .Setup(p => p.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _voiceChannelMock.Object))
            .ReturnsAsync((_playerMock.Object, _voiceChannelMock.Object, GuildId, true));

        await _service.LeaveVoiceChannel(_messageMock.Object, _memberMock.Object);

        _playerMock.Verify(p => p.StopAsync(default), Times.Once);
        _playbackEventHandlerMock.Verify(h => h.CleanupGuildAsync(GuildId), Times.Once);
        _playerMock.Verify(p => p.DisconnectAsync(default), Times.Once);
    }

    private static LavalinkTrack CreateTrack(string author, string title, string identifier)
    {
        return new LavalinkTrack
        {
            Author = author,
            Title = title,
            Identifier = identifier,
            Duration = TimeSpan.FromSeconds(100)
        };
    }

    private sealed class FakeQueueTrack(LavalinkTrack track) : ILavaLinkTrack
    {
        public string Title => track.Title;
        public string Author => track.Author;

        public LavalinkTrack ToLavalinkTrack() => track;

        public override string ToString()
        {
            return track.Identifier;
        }
    }
}