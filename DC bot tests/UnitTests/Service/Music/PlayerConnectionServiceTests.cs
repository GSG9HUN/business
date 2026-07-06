using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music;

[Trait("Category", "Unit")]
public class PlayerConnectionServiceTests
{
    private readonly Mock<IAudioService> _audioServiceMock = new();
    private readonly Mock<IDiscordChannel> _channelMock = new();
    private readonly Mock<IDiscordGuild> _guildMock = new();
    private readonly Mock<ILogger<PlayerConnectionService>> _loggerMock = new();
    private readonly Mock<IDiscordMessage> _messageMock = new();
    private readonly Mock<IPlayerManager> _playerManagerMock = new();
    private readonly Mock<IResponseBuilder> _responseBuilderMock = new();
    private readonly PlayerConnectionService _service;
    private readonly Mock<IValidationService> _validationServiceMock = new();

    public PlayerConnectionServiceTests()
    {
        _guildMock.Setup(g => g.Id).Returns(111UL);
        _channelMock.Setup(c => c.Id).Returns(222UL);
        _channelMock.Setup(c => c.Guild).Returns(_guildMock.Object);
        _audioServiceMock.Setup(a => a.Players).Returns(_playerManagerMock.Object);

        _service = new PlayerConnectionService(
            _audioServiceMock.Object,
            _validationServiceMock.Object,
            _responseBuilderMock.Object,
            _loggerMock.Object);
    }

    #region Helper Methods

    private static void SetupConnectedPlayer(Mock<ILavalinkPlayer> playerMock)
    {
        playerMock.SetupGet(p => p.ConnectionState).Returns(new PlayerConnectionState(true, null));
    }

    private void SetupJoinAsyncWithInterface()
    {
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Returns(new ValueTask<LavalinkPlayer>((LavalinkPlayer)null!));
    }

    #endregion

    #region TryJoinAndValidateAsync Tests

    [Fact]
    public async Task TryJoinAndValidateAsync_NullChannel_ReturnsInvalid()
    {
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, null);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_GuildIdIsZero_ReturnsInvalid()
    {
        _guildMock.Setup(g => g.Id).Returns(0UL);

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ChannelIdIsZero_ReturnsInvalid()
    {
        _channelMock.Setup(c => c.Id).Returns(0UL);

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_PlayerValidationFails_ReturnsInvalid()
    {
        SetupJoinAsyncWithInterface();

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupJoinAsyncWithInterface();

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_AllValid_ReturnsValid()
    {
        SetupJoinAsyncWithInterface();

        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsHttpRequestException_ReturnsInvalid()
    {
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new HttpRequestException("400 Bad Request"));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsHttpRequestExceptionWithout400_ReturnsInvalid()
    {
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new HttpRequestException("500 Internal Server Error"));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsGenericException_ReturnsInvalid()
    {
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new InvalidOperationException("unexpected"));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ConnectionValidationFailsAllAttempts_ValidatesFiveTimes()
    {
        SetupJoinAsyncWithInterface();

        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        _validationServiceMock.Verify(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()), Times.Exactly(5));
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_WhenCancellationRequestedDuringRetry_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(10));

        var playerMock = new Mock<ILavalinkPlayer>();
        _playerManagerMock
            .Setup(p => p.GetPlayerAsync(111UL, cancellation.Token))
            .ReturnsAsync((ILavalinkPlayer?)null);
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), cancellation.Token))
            .Returns(new ValueTask<LavalinkPlayer>((LavalinkPlayer)null!));

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object, cancellation.Token));

        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(It.IsAny<IDiscordMessage>(), It.IsAny<string>()),
            Times.Never);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_DisconnectedExistingPlayer_DisconnectsPlayerBeforeJoin()
    {
        var stalePlayerMock = new Mock<ILavalinkPlayer>();
        stalePlayerMock.SetupGet(p => p.ConnectionState).Returns(new PlayerConnectionState(false, null));
        stalePlayerMock.SetupGet(p => p.State).Returns(PlayerState.NotPlaying);
        stalePlayerMock.SetupGet(p => p.VoiceChannelId).Returns(222UL);

        var joinedPlayerMock = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(joinedPlayerMock);

        _playerManagerMock
            .Setup(p => p.GetPlayerAsync(111UL, CancellationToken.None))
            .ReturnsAsync(stalePlayerMock.Object);
        stalePlayerMock
            .Setup(p => p.DisconnectAsync(CancellationToken.None))
            .Returns(ValueTask.CompletedTask);

        SetupJoinAsyncWithInterface();

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, joinedPlayerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(joinedPlayerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, joinedPlayerMock.Object));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.True(result.isValid);
        stalePlayerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
    }

    #endregion

    #region TryGetAndValidateExistingPlayerAsync Tests

    [Fact]
    public async Task TryJoinAndValidateRetryConnectionAttemptsAsync_ConnectionValidationSucceeds_ReturnsValid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .SetupSequence(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, string.Empty, null))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
    }

    [Fact]
    public async Task TryJoinAndValidateRetryConnectionAttemptsAsync_ConnectionValidationFailsAllAttempts_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_NullChannel_ReturnsInvalid()
    {
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, null);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerValidationFails_ReturnsInvalid()
    {
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerIsNull_ReturnsInvalid()
    {
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, null));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.BotIsNotConnectedError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ConnectionValidationSucceeds_ReturnsValid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(playerMock);

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
        Assert.Equal(playerMock.Object, result.connection);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ValidateConnectionThrows_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(playerMock);

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ThrowsAsync(new InvalidOperationException("connection-check-failed"));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerDisconnectedWithVoiceChannel_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        playerMock.SetupGet(p => p.ConnectionState).Returns(new PlayerConnectionState(false, null));
        playerMock.SetupGet(p => p.State).Returns(PlayerState.NotPlaying);
        playerMock.SetupGet(p => p.VoiceChannelId).Returns(222UL);

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        _validationServiceMock.Verify(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()), Times.Never);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }

    #endregion
}

