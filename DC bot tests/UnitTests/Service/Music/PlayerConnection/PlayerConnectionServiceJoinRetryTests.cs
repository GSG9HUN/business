using DC_bot.Constants;
using DC_bot.Helper.Validation;
using DC_bot.Interface.Discord;
using Lavalink4NET.Players;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

[Trait("Category", "Unit")]
public class PlayerConnectionServiceJoinRetryTests : PlayerConnectionServiceTestBase
{
    [Fact]
    public async Task TryJoinAndValidateAsync_ConnectionValidationFailsAllAttempts_ValidatesFiveTimes()
    {
        SetupJoinAsyncWithInterface();

        var playerMock = new Mock<ILavalinkPlayer>();
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        ValidationServiceMock.Verify(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()), Times.Exactly(5));
    }

    [Fact]
    public async Task TryJoinAndValidateAsync_WhenCancellationRequestedDuringRetry_PropagatesCancellation()
    {
        using var cancellation = new CancellationTokenSource();
        cancellation.CancelAfter(TimeSpan.FromMilliseconds(10));

        var playerMock = new Mock<ILavalinkPlayer>();
        PlayerManagerMock
            .Setup(p => p.GetPlayerAsync(111UL, cancellation.Token))
            .ReturnsAsync((ILavalinkPlayer?)null);
        PlayerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), cancellation.Token))
            .Returns(new ValueTask<LavalinkPlayer>((LavalinkPlayer)null!));

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, ValidationErrorKeys.BotIsNotConnectedError, null));

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() =>
            Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object, cancellation.Token));

        ResponseBuilderMock.Verify(
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

        PlayerManagerMock
            .Setup(p => p.GetPlayerAsync(111UL, CancellationToken.None))
            .ReturnsAsync(stalePlayerMock.Object);
        stalePlayerMock
            .Setup(p => p.DisconnectAsync(CancellationToken.None))
            .Returns(ValueTask.CompletedTask);

        SetupJoinAsyncWithInterface();

        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, joinedPlayerMock.Object));

        ValidationServiceMock
            .Setup(v => v.ValidateConnectionAsync(joinedPlayerMock.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, joinedPlayerMock.Object));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.True(result.isValid);
        stalePlayerMock.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
    }

    [Fact]
    public async Task TryJoinAndValidateRetryConnectionAttemptsAsync_ConnectionValidationSucceeds_ReturnsValid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
        ValidationServiceMock
            .Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, playerMock.Object));

        ValidationServiceMock
            .SetupSequence(v => v.ValidateConnectionAsync(It.IsAny<ILavalinkPlayer>()))
            .ReturnsAsync(new ConnectionValidationResult(false, string.Empty, null))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, playerMock.Object));

        var result = await Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);

        Assert.True(result.isValid);
        Assert.Equal(111UL, result.guildId);
    }

    [Fact]
    public async Task TryJoinAndValidateRetryConnectionAttemptsAsync_ConnectionValidationFailsAllAttempts_ReturnsInvalid()
    {
        var playerMock = new Mock<ILavalinkPlayer>();
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
}
