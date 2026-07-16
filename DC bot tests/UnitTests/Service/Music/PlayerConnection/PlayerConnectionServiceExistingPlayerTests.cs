using DC_bot.Constants;
using DC_bot.Helper.Validation;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

[Trait("Category", "Unit")]
public class PlayerConnectionServiceExistingPlayerTests : PlayerConnectionServiceTestBase
{
    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_NullChannel_ReturnsInvalid()
    {
        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, null);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerValidationFails_ReturnsInvalid()
    {
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerIsNull_ReturnsInvalid()
    {
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, null));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.BotIsNotConnectedError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ConnectionValidationSucceeds_ReturnsValid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(playerMock);

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
        Assert.Equal(playerMock.Object, result.connection);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_ValidateConnectionThrows_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(playerMock);

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ThrowsAsync(new InvalidOperationException("connection-check-failed"));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryGetAndValidateExistingPlayerAsync_PlayerDisconnectedWithVoiceChannel_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        playerMock.SetupGet(p => p.ConnectionState).Returns(new PlayerConnectionState(false, null));
        playerMock.SetupGet(p => p.State).Returns(PlayerState.NotPlaying);
        playerMock.SetupGet(p => p.VoiceChannelId).Returns(222UL);

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(playerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await Service.TryGetAndValidateExistingPlayerAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        ValidationServiceMock.Verify(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()), Times.Never);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }
}
