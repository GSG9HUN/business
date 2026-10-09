using DC_bot.Service.Music.MusicServices;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

[Trait("Category", "Unit")]
public class StalePlayerCleanupServiceTests : PlayerConnectionServiceTestBase
{
    [Fact]
    public async Task DisconnectBeforeJoinAsync_PlayerRecovers_DoesNotDisconnect()
    {
        var player = new Mock<ILavalinkPlayer>();
        player.SetupSequence(p => p.ConnectionState)
            .Returns(new PlayerConnectionState(false, null))
            .Returns(new PlayerConnectionState(true, null));
        PlayerManagerMock.Setup(p => p.GetPlayerAsync(111UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player.Object);
        var cleanup = new StalePlayerCleanupService(AudioServiceMock.Object,
            GuildBotStatusRepositoryMock.Object, LoggerMock.Object);

        await cleanup.DisconnectBeforeJoinAsync(111UL, CancellationToken.None);

        player.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Never);
        GuildBotStatusRepositoryMock.Verify(r => r.MarkDisconnectedVoiceAsync(
            111UL, It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisconnectBeforeJoinAsync_PlayerReplaced_DoesNotDisconnectEitherPlayer()
    {
        var oldPlayer = new Mock<ILavalinkPlayer>();
        var replacement = new Mock<ILavalinkPlayer>();
        PlayerManagerMock.SetupSequence(p => p.GetPlayerAsync(111UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(oldPlayer.Object).ReturnsAsync(replacement.Object);
        var cleanup = new StalePlayerCleanupService(AudioServiceMock.Object,
            GuildBotStatusRepositoryMock.Object, LoggerMock.Object);

        await cleanup.DisconnectBeforeJoinAsync(111UL, CancellationToken.None);

        oldPlayer.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Never);
        replacement.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisconnectBeforeJoinAsync_CanceledDuringGracePeriod_DoesNotDisconnect()
    {
        var player = new Mock<ILavalinkPlayer>();
        using var cancellation = new CancellationTokenSource();
        PlayerManagerMock.Setup(p => p.GetPlayerAsync(111UL, cancellation.Token))
            .ReturnsAsync(player.Object);
        var cleanup = new StalePlayerCleanupService(AudioServiceMock.Object,
            GuildBotStatusRepositoryMock.Object, LoggerMock.Object);
        var task = cleanup.DisconnectBeforeJoinAsync(111UL, cancellation.Token);
        cancellation.Cancel();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => task);
        player.Verify(p => p.DisconnectAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DisconnectBeforeJoinAsync_StaysDisconnected_PersistenceFailureDoesNotAbortCleanup()
    {
        var player = new Mock<ILavalinkPlayer>();
        PlayerManagerMock.Setup(p => p.GetPlayerAsync(111UL, It.IsAny<CancellationToken>()))
            .ReturnsAsync(player.Object);
        GuildBotStatusRepositoryMock.Setup(r => r.MarkDisconnectedVoiceAsync(111UL, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new InvalidOperationException("Database unavailable"));
        var cleanup = new StalePlayerCleanupService(AudioServiceMock.Object,
            GuildBotStatusRepositoryMock.Object, LoggerMock.Object);

        await cleanup.DisconnectBeforeJoinAsync(111UL, CancellationToken.None);

        player.Verify(p => p.DisconnectAsync(CancellationToken.None), Times.Once);
        PlayerManagerMock.Verify(p => p.GetPlayerAsync(111UL, CancellationToken.None), Times.Exactly(21));
    }
}