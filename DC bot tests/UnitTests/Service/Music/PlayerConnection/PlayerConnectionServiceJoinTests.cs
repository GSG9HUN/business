using DC_bot.Constants;
using DC_bot.Helper.Validation;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

[Trait("Category", "Unit")]
public class PlayerConnectionServiceJoinTests : PlayerConnectionServiceTestBase
{
    [Fact]
    public async Task TryJoinAndValidateAsync_NullChannel_ReturnsInvalid()
    {
        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, null);

        Assert.False(result.isValid);
        Assert.Null(result.connection);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.UserNotInVoiceChannel),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_GuildIdIsZero_ReturnsInvalid()
    {
        GuildMock.Setup(g => g.Id).Returns(0UL);

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ChannelIdIsZero_ReturnsInvalid()
    {
        ChannelMock.Setup(c => c.Id).Returns(0UL);

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_PlayerValidationFails_ReturnsInvalid()
    {
        SetupJoinAsyncWithInterface();

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, ValidationErrorKeys.LavalinkError, null));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_ConnectionValidationFails_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        SetupJoinAsyncWithInterface();

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.BotIsNotConnectedError),
            Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_AllValid_ReturnsValid()
    {
        SetupJoinAsyncWithInterface();

        var playerMock = new Mock<ILavalinkPlayer>();
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsHttpRequestException_ReturnsInvalid()
    {
        PlayerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new HttpRequestException("400 Bad Request"));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsHttpRequestExceptionWithout400_ReturnsInvalid()
    {
        PlayerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new HttpRequestException("500 Internal Server Error"));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_JoinThrowsGenericException_ReturnsInvalid()
    {
        PlayerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Throws(new InvalidOperationException("unexpected"));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.False(result.isValid);
        ResponseBuilderMock.Verify(
            r => r.SendValidationErrorAsync(MessageMock.Object, ValidationErrorKeys.LavalinkError), Times.Once);
    }
}
