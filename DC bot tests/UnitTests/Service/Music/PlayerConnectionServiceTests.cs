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

    private void SetupJoinAsyncWithInterface()
    {
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), default))
            .Returns(new ValueTask<LavalinkPlayer>((LavalinkPlayer)null!));
    }

    #endregion

    #region TryJoinAndValidateAsync Tests

    [Fact]
    public async Task TryJoinAndValidateAsync_NullChannel_ReturnsInvalid()
    {
        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, null);

        // Assert
        Assert.False(result.isValid);
        Assert.Null(result.connection);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_GuildIdIsZero_ReturnsInvalid()
    {
        // Arrange
        _guildMock.Setup(g => g.Id).Returns(0UL);

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ChannelIdIsZero_ReturnsInvalid()
    {
        // Arrange
        _channelMock.Setup(c => c.Id).Returns(0UL);

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_PlayerValidationFails_ReturnsInvalid()
    {
        // Arrange
        SetupJoinAsyncWithInterface();

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        // Arrange
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupJoinAsyncWithInterface();

        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_AllValid_ReturnsValid()
    {
        // Arrange
        SetupJoinAsyncWithInterface();

        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsHttpRequestException_ReturnsInvalid()
    {
        // Arrange
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), default))
            .Throws(new HttpRequestException("400 Bad Request"));

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsGenericException_ReturnsInvalid()
    {
        // Arrange
        _playerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), default))
            .Throws(new InvalidOperationException("unexpected"));

        // Act
        var result = await _service.TryJoinAndValidateAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    #endregion

    #region TryGetAndValidateExistingPlayerAsync Tests

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_NullChannel_ReturnsInvalid()
    {
        // Act
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, null);

        // Assert
        Assert.False(result.isValid);
        Assert.Null(result.connection);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerValidationFails_ReturnsInvalid()
    {
        // Arrange
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        // Act
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerNotLavalinkPlayer_ReturnsInvalid()
    {
        // Arrange
        var genericPlayerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, genericPlayerMock.Object));

        // Act
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        // Arrange
        var playerMock = new Mock<ILavalinkPlayer>();
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        _validationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        // Act
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_Exception_ReturnsInvalid()
    {
        // Arrange
        _validationServiceMock
            .Setup(v => v.ValidatePlayerAsync(_audioServiceMock.Object, 111UL))
            .ThrowsAsync(new InvalidOperationException("unexpected"));

        // Act
        var result = await _service.TryGetAndValidateExistingPlayerAsync(_messageMock.Object, _channelMock.Object);

        // Assert
        Assert.False(result.isValid);
        _responseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(_messageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    #endregion
}