using DC_bot.Interface.Core;
using DC_bot.Interface.Discord;
using DC_bot.Interface.Service.Presentation;
using DC_bot.Service.Music.MusicServices;
using Lavalink4NET;
using Lavalink4NET.Players;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;

namespace DC_bot_tests.UnitTests.Service.Music.PlayerConnection;

public abstract class PlayerConnectionServiceTestBase
{
    protected readonly Mock<IAudioService> AudioServiceMock = new();
    protected readonly Mock<IDiscordChannel> ChannelMock = new();
    protected readonly Mock<IDiscordGuild> GuildMock = new();
    protected readonly Mock<ILogger<PlayerConnectionService>> LoggerMock = new();
    protected readonly Mock<IDiscordMessage> MessageMock = new();
    protected readonly Mock<IPlayerManager> PlayerManagerMock = new();
    protected readonly Mock<IResponseBuilder> ResponseBuilderMock = new();
    protected readonly PlayerConnectionService Service;
    protected readonly Mock<IValidationService> ValidationServiceMock = new();

    protected PlayerConnectionServiceTestBase()
    {
        GuildMock.Setup(g => g.Id).Returns(111UL);
        ChannelMock.Setup(c => c.Id).Returns(222UL);
        ChannelMock.Setup(c => c.Guild).Returns(GuildMock.Object);
        AudioServiceMock.Setup(a => a.Players).Returns(PlayerManagerMock.Object);

        Service = new PlayerConnectionService(
            AudioServiceMock.Object,
            ValidationServiceMock.Object,
            ResponseBuilderMock.Object,
            LoggerMock.Object);
    }

    protected static void SetupConnectedPlayer(Mock<ILavalinkPlayer> playerMock)
    {
        playerMock.SetupGet(p => p.ConnectionState).Returns(new PlayerConnectionState(true, null));
    }

    protected void SetupJoinAsyncWithInterface()
    {
        PlayerManagerMock
            .Setup(p => p.JoinAsync(111UL, 222UL, It.IsAny<PlayerFactory<LavalinkPlayer, LavalinkPlayerOptions>>(),
                It.IsAny<IOptions<LavalinkPlayerOptions>>(), CancellationToken.None))
            .Returns(new ValueTask<LavalinkPlayer>((LavalinkPlayer)null!));
    }
}
