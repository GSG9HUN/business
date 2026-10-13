using DC_bot.Helper.Validation;
using Lavalink4NET.Players;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

[Trait("Category", "Unit")]
public class PlayerConnectionServiceOperationTests : PlayerConnectionServiceTestBase
{
    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task ExecuteWithExistingPlayerAsync_JoinWaitsUntilOperationFinishes(bool operationFails)
    {
        var player = new Mock<ILavalinkPlayer>();
        SetupConnectedPlayer(player);
        SetupJoinAsyncWithInterface();
        ValidationServiceMock.Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(true, string.Empty, player.Object));
        ValidationServiceMock.Setup(v => v.ValidateConnectionAsync(player.Object))
            .ReturnsAsync(new ConnectionValidationResult(true, string.Empty, player.Object));
        var entered = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var release = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var operation = Service.ExecuteWithExistingPlayerAsync(MessageMock.Object, ChannelMock.Object,
            async (_, _) =>
            {
                entered.SetResult();
                await release.Task;
                if (operationFails) throw new InvalidOperationException("Disconnect failed");
            });
        await entered.Task.WaitAsync(TimeSpan.FromSeconds(5));
        var join = Service.TryJoinAndValidateAsync(MessageMock.Object, ChannelMock.Object);
        try
        {
            Assert.False(join.IsCompleted);
            PlayerManagerMock.Verify(p => p.GetPlayerAsync(111UL, It.IsAny<CancellationToken>()), Times.Never);
        }
        finally
        {
            release.TrySetResult();
            if (operationFails)
                await Assert.ThrowsAsync<InvalidOperationException>(() => operation.WaitAsync(TimeSpan.FromSeconds(5)));
            else
                await operation.WaitAsync(TimeSpan.FromSeconds(5));
            await join.WaitAsync(TimeSpan.FromSeconds(5));
        }

        Assert.True((await join).isValid);
    }

    [Fact]
    public async Task ExecuteWithExistingPlayerAsync_InvalidPlayer_DoesNotRunOperation()
    {
        ValidationServiceMock.Setup(v => v.ValidatePlayerAsync(AudioServiceMock.Object, 111UL))
            .ReturnsAsync(new PlayerValidationResult(false, "bot_is_not_connected_error", null));
        var called = false;
        await Service.ExecuteWithExistingPlayerAsync(MessageMock.Object, ChannelMock.Object, (_, _) =>
        {
            called = true;
            return Task.CompletedTask;
        });
        Assert.False(called);
    }
}