using DC_bot.Interface.Service.Localization;
using DC_bot.Interface.Service.Music;
using DC_bot.Interface.Service.Persistence;
using DC_bot.Wrapper;
using Microsoft.Extensions.Logging;
using Moq;

namespace DC_bot_tests.UnitTests.Wrapper;

[Trait("Category", "Unit")]
public class DiscordClientEventHandlerTests
{
    private readonly DiscordClientEventHandler _eventHandler;
    private readonly Mock<ILavaLinkService> _lavaLinkServiceMock = new();
    private readonly Mock<ILocalizationService> _localizationServiceMock = new();
    private readonly Mock<ILogger<DiscordClientEventHandler>> _loggerMock = new();
    private readonly Mock<IGuildDataRepository> _guildDataRepositoryMock = new();

    public DiscordClientEventHandlerTests()
    {
        _loggerMock.Setup(x => x.IsEnabled(It.IsAny<LogLevel>())).Returns(true);

        _eventHandler = new DiscordClientEventHandler(_loggerMock.Object, _guildDataRepositoryMock.Object,
            _localizationServiceMock.Object, _lavaLinkServiceMock.Object);
    }

    #region OnGuildAvailable Tests

    [Fact]
    public void Constructor_InitializesWithProperDependencies()
    {
        var testEventHandler = new DiscordClientEventHandler(_loggerMock.Object, _guildDataRepositoryMock.Object,
            _localizationServiceMock.Object, _lavaLinkServiceMock.Object);

        Assert.NotNull(testEventHandler);
    }

    #endregion

    #region OnClientReady Tests

    [Fact]
    public async Task OnClientReady_LogsBotIsReady()
    {
        _lavaLinkServiceMock.Setup(l => l.ConnectAsync()).Returns(Task.CompletedTask);

        await _eventHandler.OnClientReady(null!, null!);

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Information),
                It.Is<EventId>(e => e.Id == 1502),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Bot is ready")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    [Fact]
    public async Task OnClientReady_ConnectsToLavalink()
    {
        _lavaLinkServiceMock.Setup(l => l.ConnectAsync()).Returns(Task.CompletedTask);

        await _eventHandler.OnClientReady(null!, null!);

        _lavaLinkServiceMock.Verify(l => l.ConnectAsync(), Times.Once);
    }

    [Fact]
    public async Task OnClientReady_WhenLavaLinkConnectThrows_LogsError()
    {
        var exception = new InvalidOperationException("Lavalink connection failed");
        _lavaLinkServiceMock.Setup(l => l.ConnectAsync()).ThrowsAsync(exception);

        await _eventHandler.OnClientReady(null!, null!);

        _loggerMock.Verify(
            x => x.Log(
                It.Is<LogLevel>(l => l == LogLevel.Error),
                It.Is<EventId>(e => e.Id == 1504),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Discord client event failed")),
                It.IsAny<Exception>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()
            ),
            Times.Once
        );
    }

    #endregion
}
